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
completionOptions.Messages.Add(new(ChatRole.System, "You are an aggressive AI assistant at the Azure Saturday Netherlands conference. Don't use profanities though."));

var calculator = new Calculator();
completionOptions.Functions = FunctionCallingHelper.GetFunctionDefinitions<Calculator>();

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
		var synth = new SpeechSynthesizer();
		synth.Rate = 3;
		_ = synth.SpeakAsync(firstChoice.Message.Content);

		Console.ForegroundColor = ConsoleColor.Yellow;
		completionOptions.Messages.Add(new(ChatRole.User, Console.ReadLine()));
	}
	else if (firstChoice.FinishReason == CompletionsFinishReason.FunctionCall)
	{
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine($"{firstChoice.Message.FunctionCall.Name}({firstChoice.Message.FunctionCall.Arguments})");

		var calcResult = FunctionCallingHelper.CallFunction<float>(firstChoice.Message.FunctionCall, calculator);
		Console.WriteLine($" = {calcResult}");

		var functionMessage = new ChatMessage(ChatRole.Function, calcResult.ToString());
		functionMessage.Name = firstChoice.Message.FunctionCall.Name;

		completionOptions.Messages.Add(functionMessage);
	}
}


return;