using System.Diagnostics;
using System.Text;

namespace DevSharpAgent.Services;

public class FileService : IFileService
{
    private readonly string _workspaceRoot;

    public FileService(string workspaceRoot)
    {
        _workspaceRoot = Path.GetFullPath(workspaceRoot);
        Directory.CreateDirectory(_workspaceRoot);
    }

    public async Task<string> ReadFileAsync(string path)
    {
        var fullPath = GetSafePath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {path}");

        return await File.ReadAllTextAsync(fullPath, Encoding.UTF8);
    }

    public async Task WriteFileAsync(string path, string content)
    {
        var fullPath = GetSafePath(path);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(fullPath, content, Encoding.UTF8);
    }

    public async Task CreateFileAsync(string path, string content)
    {
        var fullPath = GetSafePath(path);
        if (File.Exists(fullPath))
            throw new InvalidOperationException($"File already exists: {path}");

        await WriteFileAsync(path, content);
    }

    public Task DeleteFileAsync(string path)
    {
        var fullPath = GetSafePath(path);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        else if (Directory.Exists(fullPath))
            Directory.Delete(fullPath, true);

        return Task.CompletedTask;
    }

    public Task<string[]> ListFilesAsync(string path)
    {
        var fullPath = GetSafePath(path);
        
        if (!Directory.Exists(fullPath))
            return Task.FromResult(Array.Empty<string>());

        var files = Directory.GetFileSystemEntries(fullPath)
            .Select(f => Path.GetRelativePath(_workspaceRoot, f))
            .ToArray();

        return Task.FromResult(files);
    }

    public async Task<string> ExecuteCommandAsync(string command, string[] arguments)
    {
        using var process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.WorkingDirectory = _workspaceRoot;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        if (arguments.Length > 0)
        {
            foreach (var arg in arguments)
                process.StartInfo.ArgumentList.Add(arg);
        }

        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
                output.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
                error.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        var result = output.ToString();
        if (process.ExitCode != 0 && error.Length > 0)
            result += $"\nError: {error}";

        return result.Trim();
    }

    public bool FileExists(string path)
    {
        var fullPath = GetSafePath(path);
        return File.Exists(fullPath);
    }

    public bool DirectoryExists(string path)
    {
        var fullPath = GetSafePath(path);
        return Directory.Exists(fullPath);
    }

    private string GetSafePath(string path)
    {
        // ป้องกัน path traversal attacks
        var normalizedPath = Path.GetFullPath(Path.Combine(_workspaceRoot, path));
        
        if (!normalizedPath.StartsWith(_workspaceRoot))
            throw new UnauthorizedAccessException($"Access denied to path outside workspace: {path}");

        return normalizedPath;
    }

    public string GetWorkspaceRoot()
    {
        return _workspaceRoot;
    }
}