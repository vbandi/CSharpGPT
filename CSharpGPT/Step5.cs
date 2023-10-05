using System.Globalization;
using System.Speech.Synthesis;
using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Utilities.FunctionCalling;

var options = new OpenAIClientOptions(OpenAIClientOptions.ServiceVersion.V2023_09_01_Preview); 

var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
var credential = new AzureKeyCredential(key);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_BASE");

var openAIClient = new OpenAIClient(new Uri(endpoint), credential, options);

ChatCompletionsOptions completionOptions = new();
completionOptions.Messages.Add(new(ChatRole.System, "You are an aggressive AI assistant at the Azure Saturday Netherlands conference."));

var calculator = new Calculator();
completionOptions.Functions = FunctionCallingHelper.GetFunctionDefinitions(calculator);

Console.ForegroundColor = ConsoleColor.Yellow;
completionOptions.Messages.Add(new(ChatRole.User, Console.ReadLine()));

while (true)
{
	var result = openAIClient.GetChatCompletions("gpt4444", completionOptions);
	var firstChoice = result.Value.Choices.FirstOrDefault();

	if (firstChoice.FinishReason == CompletionsFinishReason.Stopped)
	{
		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine(firstChoice.Message.Content);
		completionOptions.Messages.Add(firstChoice.Message);
		Console.WriteLine();
		new SpeechSynthesizer().SpeakAsync(firstChoice.Message.Content);

		Console.ForegroundColor = ConsoleColor.Yellow;
		completionOptions.Messages.Add(new(ChatRole.User, Console.ReadLine()));
	}
	else if (firstChoice.FinishReason == CompletionsFinishReason.FunctionCall)
	{
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine($"{firstChoice.Message.FunctionCall.Name}({firstChoice.Message.FunctionCall.Arguments})");

		var calcResult = FunctionCallingHelper.CallFunction<float>(firstChoice.Message.FunctionCall, calculator);
		Console.WriteLine($" = {calcResult}");
		firstChoice.Message.Content = calcResult.ToString(CultureInfo.InvariantCulture);
		completionOptions.Messages.Add(firstChoice.Message);

		
	}
}


return;