//using System.Text;
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

//var result = openAIClient.GetChatCompletions("chatgptplayground", completionOptions);

//var firstChoice = result.Value.Choices.FirstOrDefault();

//Console.WriteLine(firstChoice.Message.Content);

//while (true)
//{
//	Console.ForegroundColor = ConsoleColor.Yellow;
//	var userInput = Console.ReadLine();
//	ChatMessage userMsg = new(ChatRole.User, userInput);
//	completionOptions.Messages.Add(userMsg);
//	//completionOptions.ChoiceCount = 3;

	
//	Console.ForegroundColor = ConsoleColor.White;
//	var streamingResult = openAIClient.GetChatCompletionsStreamingAsync("chatgptplayground", completionOptions);

//	StringBuilder answerBuilder = new();
//	await foreach (var choice in streamingResult.Result.Value.GetChoicesStreaming())
//	{
//		//Console.WriteLine($"Choice #{choice.Index}");

//		await foreach (var message in choice.GetMessageStreaming())
//		{
//			Console.Write(message.Content);
//			answerBuilder.Append(message.Content);
//		}

//		Console.WriteLine();
//		Console.WriteLine();
//	}

//	completionOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, answerBuilder.ToString()));

//}


//return;