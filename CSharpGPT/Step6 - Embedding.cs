using Azure;
using Azure.AI.OpenAI;
using Microsoft.SemanticMemory;

var options = new OpenAIClientOptions(OpenAIClientOptions.ServiceVersion.V2023_09_01_Preview);

var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
var credential = new AzureKeyCredential(key);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_BASE");
var openAIClient = new OpenAIClient(new Uri(endpoint), credential, options);

var memory = new MemoryClientBuilder()
	.WithOpenAIDefaults(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
	.BuildServerlessClient();

if (false)
{
	var currentDirectory = Directory.GetCurrentDirectory();

// add all files in /Sessions
	var files = Directory.GetFiles("Sessions").Select(x => Path.Combine(currentDirectory, x));
	Dictionary<string, string> sessions = new();

	foreach (var file in files)
	{
		Console.WriteLine($"Processing file {file}");
		var fileKey = Path.GetFileNameWithoutExtension(file);
		var txt = File.ReadAllText(file);
		await memory.ImportTextAsync(txt, fileKey);
		sessions[fileKey] = txt;
	}
}


Console.WriteLine("Processing done. Ask away");

ChatMessage msg = new(ChatRole.System, "You are a overly friendly AI chatbot at the Azure Saturday Netherlands conference.");

ChatCompletionsOptions completionOptions = new();
completionOptions.Messages.Add(msg);

var result = openAIClient.GetChatCompletions("chatgptplayground", completionOptions);

var firstChoice = result.Value.Choices.FirstOrDefault();

Console.WriteLine(firstChoice.Message.Content);

while (true)
{
	Console.ForegroundColor = ConsoleColor.Yellow;
	var userInput = Console.ReadLine();
	ChatMessage userMsg = new(ChatRole.User, userInput);
	completionOptions.Messages.Add(userMsg);

	// get relevant sessions, if any
	var searchResult = await memory.SearchAsync(userInput);
	//var orderedPartitions = searchResult.Results.SelectMany(x => x.Partitions).Distinct().OrderByDescending(p => p.Relevance);
	//var topPartitions = orderedPartitions.Take(2).Select(x => x.Text);
	//var hint = String.Join("\n", topPartitions);

	var orderedLinks = searchResult.Results.OrderByDescending(c => c.Partitions.Max(p => p.Relevance))
		.Select(c => c.Link).Distinct();

	//var hint = String.Join("\n\n", orderedLinks.Select(l => sessions[l]));
	
	//Console.WriteLine($"Search results: {hint}");
	//completionOptions.Messages.Add(new ChatMessage(ChatRole.System,
	//	$"These sessions may be relevant to the user's query.: \n{hint}\n\n"));

	result = openAIClient.GetChatCompletions("chatgptplayground", completionOptions);
	firstChoice = result.Value.Choices.FirstOrDefault();
	completionOptions.Messages.Add(firstChoice.Message);

	Console.ForegroundColor = ConsoleColor.White;
	Console.WriteLine(firstChoice.Message.Content);
	Console.WriteLine();
}


return;