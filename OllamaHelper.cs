using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class OllamaHelper
{
	private readonly string _endpoint;
	private readonly string _modelName;

	public OllamaHelper(string endpoint, string modelName)
	{
		_endpoint = endpoint;
		_modelName = modelName;
	}

	public async Task<string?> SuggestSelectorAsync(string html, string failedSelector, string description)
	{
		using var client = new HttpClient();
		var systemPrompt = "You are an expert in web automation and CSS selectors. Given HTML and a failed selector, suggest a new CSS selector for the intended element. Output only the selector.";
		var userPrompt = $"HTML:\n{html}\nFailed Selector: '{failedSelector}' for: {description}";

		var requestBody = new
		{
			model = _modelName,
			messages = new object[]
			{
				new { role = "system", content = systemPrompt },
				new { role = "user", content = userPrompt }
			},
			max_tokens = 32,
			temperature = 0.2
		};

		var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
		var response = await client.PostAsync(_endpoint, content);
		response.EnsureSuccessStatusCode();
		var responseString = await response.Content.ReadAsStringAsync();

		using var doc = JsonDocument.Parse(responseString);
		var selector = doc.RootElement
			.GetProperty("choices")[0]
			.GetProperty("message")
			.GetProperty("content")
			.GetString()
			?.Trim();
		return selector;
	}
}