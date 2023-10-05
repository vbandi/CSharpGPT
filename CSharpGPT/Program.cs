/*using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;


var options = new OpenAIClientOptions(OpenAIClientOptions.ServiceVersion.V2023_09_01_Preview);

var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
var credential = new AzureKeyCredential(key!);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_BASE");
var openAIClient = new OpenAIClient(new Uri(endpoint!), credential, options);

FunctionCallingTestHelpers.CalculatorDemo(openAIClient);
return;

var dalleHelper = new DalleHelper(openAIClient);

ChatMessage msg = new(ChatRole.System, "You are a overly friendly AI chatbot at the Azure Saturday Netherlands conference."); 

ChatCompletionsOptions completionOptions = new();
completionOptions.Messages.Add(msg);

FunctionDefinition generateImageFunction = new()
{
	Name = "GenerateImage",
	Description = "Generates a new image based on the prompt.",
	Parameters = BinaryData.FromObjectAsJson(new
	{
		Type = "object",
		Properties = new
		{
			Prompt = new
			{
				Type = "string",
				Description = "A detailed description of the image."
			}
		},
		Required = new[] { "prompt" }
	}, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
};

completionOptions.Functions.Add(generateImageFunction);

while (true)
{
	var result = openAIClient.GetChatCompletions("chatgptplayground", completionOptions);
	var firstChoice = result?.Value.Choices.FirstOrDefault();

	if (firstChoice != null && firstChoice.FinishReason == CompletionsFinishReason.Stopped)
	{
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine(firstChoice.Message.Content);
		completionOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, firstChoice.Message.Content));
	}
	else if (firstChoice?.FinishReason == CompletionsFinishReason.FunctionCall)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine($"{firstChoice.Message.FunctionCall.Name}({firstChoice.Message.FunctionCall.Arguments})");

		if (firstChoice.Message.FunctionCall.Name == "GenerateImage")
		{
			string argumentsJson = firstChoice.Message.FunctionCall.Arguments.ToString();
			var arguments = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(argumentsJson);
			dalleHelper.GenerateImage(arguments["prompt"]);
		}
	}

	Console.ForegroundColor = ConsoleColor.White;
	Console.Write("You: ");
	var input = Console.ReadLine();
	completionOptions.Messages.Add(new ChatMessage(ChatRole.User, input));
}*/