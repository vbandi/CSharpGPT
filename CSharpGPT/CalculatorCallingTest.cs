using System.Globalization;
using Azure.AI.OpenAI;
using OpenAI.Utilities.FunctionCalling;

public static partial class FunctionCallingTestHelpers
{
	public static void CalculatorDemo(OpenAIClient openAIClient)
	{
		var calculator = new Calculator();

		ChatCompletionsOptions completionOptions = new();
		ChatMessage systemPrompt = new(ChatRole.System, "You are an aggressive AI assistant.");
		completionOptions.Messages.Add(systemPrompt);

		completionOptions.Functions = FunctionCallingHelper.GetFunctionDefinitions<Calculator>();

		while (true)
		{
			var result = openAIClient.GetChatCompletions("gpt4444", completionOptions);
			var firstChoice = result?.Value.Choices.FirstOrDefault();

			if (firstChoice != null && firstChoice.FinishReason == CompletionsFinishReason.Stopped)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine(firstChoice.Message.Content);
				completionOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, firstChoice.Message.Content));

				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("You: ");
				var input = Console.ReadLine();
				completionOptions.Messages.Add(new ChatMessage(ChatRole.User, input));
			}
			else if (firstChoice?.FinishReason == CompletionsFinishReason.FunctionCall)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;

				Console.WriteLine(
					$"{firstChoice.Message.FunctionCall.Name}({firstChoice.Message.FunctionCall.Arguments})");

				var functionCall = firstChoice.Message.FunctionCall;
				var functionCallResult = FunctionCallingHelper.CallFunction<float>(functionCall, calculator);

				Console.WriteLine($"Result: {functionCallResult}\n");

				firstChoice.Message.Content = functionCallResult.ToString(CultureInfo.CurrentCulture);
				completionOptions.Messages.Add(firstChoice.Message);
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Something went wrong.");
			}
		}
	}
}