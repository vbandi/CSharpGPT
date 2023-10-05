//using System.Text.Json;
//using Azure;
//using Azure.AI.OpenAI;

//var options = new OpenAIClientOptions(OpenAIClientOptions.ServiceVersion.V2023_09_01_Preview);

//var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
//var credential = new AzureKeyCredential(key);

//var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_BASE");
//var openAIClient = new OpenAIClient(new Uri(endpoint), credential, options);

//ChatMessage msg = new(ChatRole.System, "You are a overly friendly AI chatbot at the Azure Saturday Netherlands conference.");

//ChatCompletionsOptions completionOptions = new();
//completionOptions.Messages.Add(msg);

//var dalleHelper = new DalleHelper(openAIClient);
//completionOptions.Functions = new List<FunctionDefinition>() { dalleHelper.GetGenerateImageFunctionDefinition() };

//var result = openAIClient.GetChatCompletions("chatgptplayground", completionOptions);
//var firstChoice = result.Value.Choices.FirstOrDefault();
//Console.WriteLine(firstChoice.Message.Content);



//while (true)
//{
//	Console.ForegroundColor = ConsoleColor.Yellow;
//	var userInput = Console.ReadLine();
//	ChatMessage userMsg = new(ChatRole.User, userInput);
//	completionOptions.Messages.Add(userMsg);

//	result = openAIClient.GetChatCompletions("chatgptplayground", completionOptions);
//	firstChoice = result.Value.Choices.FirstOrDefault();
//	completionOptions.Messages.Add(firstChoice.Message);

//	if (firstChoice.FinishReason == CompletionsFinishReason.Stopped)
//	{
//		Console.ForegroundColor = ConsoleColor.White;
//		Console.WriteLine(firstChoice.Message.Content);
//		Console.WriteLine();
//	}
//	else if (firstChoice.FinishReason == CompletionsFinishReason.FunctionCall)
//	{
//		Console.ForegroundColor = ConsoleColor.Cyan;
//		Console.WriteLine($"{firstChoice.Message.FunctionCall.Name}({firstChoice.Message.FunctionCall.Arguments})");

//		if (firstChoice.Message.FunctionCall.Name == "GenerateImage")
//		{
//			string argumentsJson = firstChoice.Message.FunctionCall.Arguments;
//			var arguments = JsonSerializer.Deserialize<Dictionary<string, string>>(argumentsJson);
//			dalleHelper.GenerateImage(arguments["prompt"]);
//		}
//	}
//}


//return;