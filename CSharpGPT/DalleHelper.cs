using Azure.AI.OpenAI;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

internal class DalleHelper
{
	OpenAIClient _openAIClient;

	public DalleHelper(OpenAIClient openAIClient)
	{
		_openAIClient = openAIClient;
	}

	public void GenerateImage(string prompt)
	{
		var opt = new ImageGenerationOptions(prompt);
		opt.Size = ImageSize.Size512x512;
		var result = _openAIClient.GetImageGenerations(opt);
		ShowBitmaps(result.Value);
	}

	private void ShowBitmaps(ImageGenerations imageGenerations)
	{
		foreach (ImageLocation imageLocation in imageGenerations.Data)
		{
			var url = imageLocation.Url;
			// open a browser to the URL
			Process.Start(new ProcessStartInfo(url.ToString()) { UseShellExecute = true });
		}
	}

	public FunctionDefinition GetGenerateImageFunctionDefinition()
	{
		return new FunctionDefinition()
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
	}

}