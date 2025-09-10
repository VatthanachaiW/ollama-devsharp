using System.Text;
using System.Text.Json;
using DevSharpAgent.Models;

namespace DevSharpAgent.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public OllamaService(HttpClient httpClient, string baseUrl = "http://localhost:11434")
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<string> GenerateAsync(string prompt, string model = "devsharp")
    {
        var response = await GenerateDetailedAsync(prompt, model);
        return response.Response;
    }

    public async Task<OllamaResponse> GenerateDetailedAsync(string prompt, string model = "devsharp")
    {
        var request = new OllamaRequest
        {
            Model = model,
            Prompt = prompt,
            Stream = false
        };

        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var json = JsonSerializer.Serialize(request, options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseJson);

        return ollamaResponse ?? throw new InvalidOperationException("Failed to deserialize Ollama response");
    }

    public async Task<bool> IsModelAvailableAsync(string model)
    {
        try
        {
            var models = await GetAvailableModelsAsync();
            return models.Contains(model, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public async Task<string[]> GetAvailableModelsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty("models", out var modelsElement))
            {
                return modelsElement.EnumerateArray()
                    .Select(m => m.GetProperty("name").GetString() ?? "")
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToArray();
            }

            return Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}