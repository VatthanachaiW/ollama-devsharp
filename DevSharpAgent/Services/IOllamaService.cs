using DevSharpAgent.Models;

namespace DevSharpAgent.Services;

public interface IOllamaService
{
    Task<string> GenerateAsync(string prompt, string model = "devsharp");
    Task<OllamaResponse> GenerateDetailedAsync(string prompt, string model = "devsharp");
    Task<bool> IsModelAvailableAsync(string model);
    Task<string[]> GetAvailableModelsAsync();
}