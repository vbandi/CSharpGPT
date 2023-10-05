using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.AI.OpenAI;

namespace OpenAI.Utilities.FunctionCalling;

/// <summary>
///     Helper methods for Function Calling
/// </summary>
public static class FunctionCallingHelper
{
	/// <summary>
	///     Returns a <see cref="FunctionDefinition" /> from the provided method, using any
	///     <see cref="FunctionDescriptionAttribute" /> and <see cref="ParameterDescriptionAttribute" /> attributes
	/// </summary>
	/// <param name="methodInfo">the method to create the <see cref="FunctionDefinition" /> from</param>
	/// <returns>the <see cref="FunctionDefinition" /> created.</returns>
	public static FunctionDefinition GetFunctionDefinition(MethodInfo methodInfo)
	{
		var methodDescriptionAttribute = methodInfo.GetCustomAttribute<FunctionDescriptionAttribute>();

		var callableFunction = new CallableFunction(
			methodDescriptionAttribute?.Name ?? methodInfo.Name,
			methodDescriptionAttribute?.Description);

		var parameters = methodInfo.GetParameters().ToList();

		if (parameters.Count > 0)
			callableFunction.Parameters = new();

		foreach (var parameter in parameters)
		{
			var parameterDescriptionAttribute = parameter.GetCustomAttribute<ParameterDescriptionAttribute>();
			var description = parameterDescriptionAttribute?.Description;
			var required = parameterDescriptionAttribute?.Required ?? true;

			CallableFunctionParameter argument;

			switch (parameter.ParameterType, parameterDescriptionAttribute?.Type == null)
			{
				case (_, false):
					argument = new CallableFunctionParameter(parameterDescriptionAttribute!.Type!, description);
					break;

				case ({ } t, _) when t.IsAssignableFrom(typeof(int)):
					argument = new CallableFunctionParameter("integer", description, required);
					break;
				case ({ } t, _) when t.IsAssignableFrom(typeof(float)):
					argument = new CallableFunctionParameter("number", description, required);
					break;
				case ({ } t, _) when t.IsAssignableFrom(typeof(bool)):
					argument = new CallableFunctionParameter("boolean", description, required);
					break;
				case ({ } t, _) when t.IsAssignableFrom(typeof(string)):
					argument = new CallableFunctionParameter("string", description, required);
					break;
				case ({ IsEnum: true }, _):

					var enumValues = string.IsNullOrEmpty(parameterDescriptionAttribute?.Enum)
						? Enum.GetNames(parameter.ParameterType).ToList()
						: parameterDescriptionAttribute.Enum.Split(",").Select(x => x.Trim()).ToList();

					argument =
						new CallableFunctionParameter("string", description, required, enumValues);

					break;
				default:
					throw new Exception($"Parameter type '{parameter.ParameterType}' not supported");
			}

			var paramName = parameterDescriptionAttribute?.Name ?? parameter.Name!;

			callableFunction.Parameters!.Properties!.Add(paramName, argument);
		}


		List<string>? requiredParameters = callableFunction.Parameters?.Properties
			.Where(kvp => kvp.Value.Required)
			.Select(kvp => kvp.Key)
			.ToList();

		if (requiredParameters is { Count: > 0 })
			callableFunction.Parameters!.RequiredProperties = requiredParameters;

		FunctionDefinition result = new()
		{
			Name = callableFunction.Name,
			Description = callableFunction.Description,
			Parameters = callableFunction.Parameters?.Properties.Count > 0
				? BinaryData.FromObjectAsJson(callableFunction.Parameters)
				: null
		};

		return result;
	}

	/// <summary>
	///     Enumerates the methods in the provided object, and a returns a <see cref="List{FunctionDefinition}" /> of
	///     <see cref="FunctionDefinition" /> for all methods
	///     marked with a <see cref="FunctionDescriptionAttribute" />
	/// </summary>
	/// <param name="obj">the object to analyze</param>
	public static List<FunctionDefinition> GetFunctionDefinitions(object obj)
	{
		var type = obj.GetType();
		return GetFunctionDefinitions(type);
	}

	/// <summary>
	///     Enumerates the methods in the provided type, and a returns a <see cref="List{FunctionDefinition}" /> of
	///     <see cref="FunctionDefinition" /> for all methods
	/// </summary>
	/// <typeparam name="T">The type to analyze</typeparam>
	/// <returns></returns>
	public static List<FunctionDefinition> GetFunctionDefinitions<T>()
	{
		return GetFunctionDefinitions(typeof(T));
	}

	/// <summary>
	///     Enumerates the methods in the provided type, and a returns a <see cref="List{FunctionDefinition}" /> of
	///     <see cref="FunctionDefinition" /> for all methods
	/// </summary>
	/// <param name="type">The type to analyze</param>
	public static List<FunctionDefinition> GetFunctionDefinitions(Type type)
	{
		var methods = type.GetMethods();

		var result = methods
			.Select(method => new
			{
				method,
				methodDescriptionAttribute = method.GetCustomAttribute<FunctionDescriptionAttribute>()
			})
			.Where(t => t.methodDescriptionAttribute != null)
			.Select(t => GetFunctionDefinition(t.method)).ToList();

		return result;
	}

	/// <summary>
	///     Calls the function on the provided object, using the provided <see cref="FunctionCall" /> and returns the result of
	///     the call
	/// </summary>
	/// <param name="functionCall">The FunctionCall provided by the LLM</param>
	/// <param name="obj">the object with the method / function to be executed</param>
	/// <typeparam name="T">The return type</typeparam>
	public static T? CallFunction<T>(FunctionCall functionCall, object obj)
	{
		if (functionCall == null)
		{
			throw new ArgumentNullException(nameof(functionCall));
		}

		if (functionCall.Name == null)
		{
			throw new InvalidFunctionCallException("Function Name is null");
		}

		if (obj == null)
		{
			throw new ArgumentNullException(nameof(obj));
		}

		var methodInfo = obj.GetType().GetMethod(functionCall.Name);

		if (methodInfo == null)
		{
			throw new InvalidFunctionCallException($"Method '{functionCall.Name}' on type '{obj.GetType()}' not found");
		}

		if (!methodInfo.ReturnType.IsAssignableTo(typeof(T)))
		{
			throw new InvalidFunctionCallException(
				$"Method '{functionCall.Name}' on type '{obj.GetType()}' has return type '{methodInfo.ReturnType}' but expected '{typeof(T)}'");
		}

		var parameters = methodInfo.GetParameters().ToList();
		
		var arguments = !string.IsNullOrWhiteSpace(functionCall.Arguments) ? 
			JsonSerializer.Deserialize<Dictionary<string, object>>(functionCall.Arguments) : new Dictionary<string, object>();
			
		var args = new List<object?>();

		foreach (var parameter in parameters)
		{
			var parameterDescriptionAttribute =
				parameter.GetCustomAttribute<ParameterDescriptionAttribute>();

			var name = parameterDescriptionAttribute?.Name ?? parameter.Name!;
			var argument = arguments.FirstOrDefault(x => x.Key == name);

			if (argument.Key == null)
			{
				throw new Exception($"Argument '{name}' not found");
			}

			var value = parameter.ParameterType.IsEnum ? Enum.Parse(parameter.ParameterType, argument.Value.ToString()!) : ((JsonElement)argument.Value).Deserialize(parameter.ParameterType);

			args.Add(value);
		}

		var result = (T?)methodInfo.Invoke(obj, args.ToArray());
		return result;
	}

	/// <summary>
	/// Calls the async function on the provided object, using the provided <see cref="FunctionCall" /> and returns the result of
	/// the call asynchronously
	/// </summary>
	public static Task<T> CallFunctionAsync<T>(FunctionCall functionCall, object obj)
	{
		return CallFunction<Task<T>>(functionCall, obj)!;
	}
}


public class CallableFunction
{
	[JsonPropertyName("name")]
	public string Name { get; set; }
	
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("parameters")]
	public CallableFunctionParameters? Parameters { get; set; }

	public CallableFunction(string name, string? description = null)
	{
		Name = name;
		Description = description;
	}
}

public class CallableFunctionParameters
{
	[JsonPropertyName("type")]
	public string Type { get; set; } = "object";

	[JsonPropertyName("properties")]
	public Dictionary<string, CallableFunctionParameter> Properties { get; set; } = new();

	[JsonPropertyName("required")]
	public List<string>? RequiredProperties { get; set; }
}

public class CallableFunctionParameter
{
	[JsonPropertyName("type")]
	public string Type { get; set; }

	[JsonPropertyName("description")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Description { get; set; }

	[JsonPropertyName("enum")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public List<string>? Enum { get; set; }

	[JsonPropertyName("default")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public object? Default { get; set; }

	[JsonPropertyName("required")]
	[JsonIgnore]
	public bool Required { get; set; }

	public CallableFunctionParameter(string type, string? description = null, bool required = true, List<string>? @enum = null, object? @default = null)
	{
		Type = type;
		Description = description;
		Enum = @enum;
		Default = @default;
		Required = required;
	}
}