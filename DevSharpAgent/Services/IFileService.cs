namespace DevSharpAgent.Services;

public interface IFileService
{
    Task<string> ReadFileAsync(string path);
    Task WriteFileAsync(string path, string content);
    Task CreateFileAsync(string path, string content);
    Task DeleteFileAsync(string path);
    Task<string[]> ListFilesAsync(string path);
    Task<string> ExecuteCommandAsync(string command, string[] arguments);
    bool FileExists(string path);
    bool DirectoryExists(string path);
    string GetWorkspaceRoot();
}