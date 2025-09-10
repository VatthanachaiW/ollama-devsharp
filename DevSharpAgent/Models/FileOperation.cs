using System.Text.Json.Serialization;

namespace DevSharpAgent.Models;

public enum FileOperationType
{
    Read,
    Write,
    Create,
    Delete,
    List,
    Execute
}

public class FileOperation
{
    [JsonPropertyName("operation")]
    public string Operation { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("arguments")]
    public string[]? Arguments { get; set; }

    public FileOperationType GetOperationType()
    {
        return Operation.ToLowerInvariant() switch
        {
            "read" or "read_file" => FileOperationType.Read,
            "write" or "write_file" => FileOperationType.Write,
            "create" or "create_file" => FileOperationType.Create,
            "delete" or "delete_file" => FileOperationType.Delete,
            "list" or "list_files" => FileOperationType.List,
            "execute" or "run_command" => FileOperationType.Execute,
            _ => FileOperationType.Read
        };
    }
}