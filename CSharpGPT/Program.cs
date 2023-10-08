//using System.Text.Json;
//using System.Text.Json.Serialization;
//using Azure;
//using Azure.AI.OpenAI;

//var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_BASE");
//var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
//AzureKeyCredential credential = new(key);

//OpenAIClient openAiClient = new(new Uri(endpoint), credential);

//DalleHelper dalleHelper = new(openAiClient);

//ChatCompletionsOptions completionOptions = new();
//completionOptions.Messages.Add(new(ChatRole.System, "You are a friendly assistant at the Azure Saturday Netherlands conference."));
//completionOptions.Functions = new List<FunctionDefinition> {dalleHelper.GetGenerateImageFunctionDefinition() };

//while (true)
//{
//	Console.ForegroundColor = ConsoleColor.Yellow;
//	var userInput = Console.ReadLine();
//	ChatMessage userMessage = new(ChatRole.User, userInput);
//	completionOptions.Messages.Add(userMessage);

//	var result = openAiClient.GetChatCompletions("chatgptplayground", completionOptions);
//	var firstChoice = result.Value.Choices.FirstOrDefault();

//	if (firstChoice.FinishReason == CompletionsFinishReason.FunctionCall)
//	{
//		Console.ForegroundColor = ConsoleColor.Cyan;
//		Console.WriteLine($"{firstChoice.Message.FunctionCall.Name}({firstChoice.Message.FunctionCall.Arguments})");

//		if (firstChoice.Message.FunctionCall.Name == "GenerateImage")
//		{
//			var arguments = JsonSerializer.Deserialize<Dictionary<string, string>>(firstChoice.Message.FunctionCall.Arguments);
//			dalleHelper.GenerateImage(arguments["prompt"]);
//		}
//	}

//	Console.ForegroundColor = ConsoleColor.White;
//	Console.WriteLine(firstChoice.Message.Content);
//	completionOptions.Messages.Add(firstChoice.Message);
//}