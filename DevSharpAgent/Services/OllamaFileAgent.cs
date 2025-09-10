using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DevSharpAgent.Models;

namespace DevSharpAgent.Services;

public class OllamaFileAgent
{
    private readonly IOllamaService _ollamaService;
    private readonly IFileService _fileService;
    private readonly IPermissionManager _permissionManager;

    public OllamaFileAgent(IOllamaService ollamaService, IFileService fileService, IPermissionManager permissionManager)
    {
        _ollamaService = ollamaService;
        _fileService = fileService;
        _permissionManager = permissionManager;
    }

    public async Task<string> ChatWithFileAccessAsync(string userPrompt, string[]? filePaths = null)
    {
        // เตรียม context จากไฟล์ที่ระบุ
        var context = await BuildFileContextAsync(filePaths);
        
        // สร้าง enhanced prompt ที่มีความสามารถในการจัดการไฟล์
        var enhancedPrompt = BuildEnhancedPrompt(userPrompt, context);
        
        // ส่งไปยัง Ollama
        var response = await _ollamaService.GenerateAsync(enhancedPrompt);
        
        // ประมวลผล file operations ที่อาจมีใน response
        var processedResponse = await ProcessFileOperationsAsync(response);
        
        return processedResponse;
    }

    private async Task<Dictionary<string, string>> BuildFileContextAsync(string[]? filePaths)
    {
        var context = new Dictionary<string, string>();
        
        if (filePaths == null || filePaths.Length == 0)
            return context;

        foreach (var path in filePaths)
        {
            try
            {
                if (_fileService.FileExists(path))
                {
                    var content = await _fileService.ReadFileAsync(path);
                    context[path] = content;
                }
                else if (_fileService.DirectoryExists(path))
                {
                    var files = await _fileService.ListFilesAsync(path);
                    context[path] = $"Directory contents: {string.Join(", ", files)}";
                }
            }
            catch (Exception ex)
            {
                context[path] = $"Error reading file: {ex.Message}";
            }
        }

        return context;
    }

    private string BuildEnhancedPrompt(string userPrompt, Dictionary<string, string> fileContext)
    {
        var promptBuilder = new StringBuilder();
        
        // เพิ่มคำแนะนำเกี่ยวกับ file operations
        promptBuilder.AppendLine("You are a helpful coding assistant with file system access. You can perform the following operations:");
        promptBuilder.AppendLine("- READ_FILE(path): อ่านไฟล์");
        promptBuilder.AppendLine("- WRITE_FILE(path, content): เขียน/สร้างไฟล์");
        promptBuilder.AppendLine("- DELETE_FILE(path): ลบไฟล์");
        promptBuilder.AppendLine("- LIST_FILES(path): แสดงรายการไฟล์");
        promptBuilder.AppendLine("- EXECUTE(command, args): รันคำสั่ง");
        promptBuilder.AppendLine();
        
        promptBuilder.AppendLine("Format file operations like this (IMPORTANT: escape newlines as \\n in JSON content):");
        promptBuilder.AppendLine("```fileop");
        promptBuilder.AppendLine("{\"operation\": \"WRITE_FILE\", \"path\": \"example.cs\", \"content\": \"// code here\\nusing System;\\n\\nclass Program { }\"}");
        promptBuilder.AppendLine("```");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("CRITICAL: In JSON content field, always escape:");
        promptBuilder.AppendLine("- Newlines as \\n");
        promptBuilder.AppendLine("- Quotes as \\\""); 
        promptBuilder.AppendLine("- Backslashes as \\\\");
        promptBuilder.AppendLine();

        // เพิ่ม file context ถ้ามี
        if (fileContext.Any())
        {
            promptBuilder.AppendLine("Current file context:");
            foreach (var (path, content) in fileContext)
            {
                promptBuilder.AppendLine($"=== {path} ===");
                promptBuilder.AppendLine(content);
                promptBuilder.AppendLine();
            }
        }

        // เพิ่ม user prompt
        promptBuilder.AppendLine("User request:");
        promptBuilder.AppendLine(userPrompt);

        return promptBuilder.ToString();
    }

    private async Task<string> ProcessFileOperationsAsync(string response)
    {
        // หา file operations ใน response
        var fileOpPattern = @"```fileop\s*\n(.*?)\n```";
        var matches = Regex.Matches(response, fileOpPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        var processedResponse = new StringBuilder(response);
        var operationResults = new List<string>();

        foreach (Match match in matches)
        {
            try
            {
                var operationJson = match.Groups[1].Value.Trim();
                
                // Fix JSON escaping issues for multiline content
                var fixedJson = FixJsonEscaping(operationJson);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = false
                };
                
                var operation = JsonSerializer.Deserialize<FileOperation>(fixedJson, options);

                if (operation != null)
                {
                    // ตรวจสอบสิทธิ์ก่อนดำเนินการ
                    var permissionResult = await _permissionManager.CheckPermissionAsync(operation);
                    
                    if (!permissionResult.IsAllowed && permissionResult.RequiredConfirmation)
                    {
                        // ขอสิทธิ์จากผู้ใช้
                        var risk = _permissionManager.EvaluateRisk(operation);
                        var userApproved = await _permissionManager.RequestUserPermissionAsync(operation, risk);
                        
                        if (!userApproved)
                        {
                            operationResults.Add($"❌ {operation.Operation} {operation.Path}: ถูกปฏิเสธโดยผู้ใช้");
                            await _permissionManager.LogOperationAsync(operation, permissionResult, "DENIED by user");
                            processedResponse.Replace(match.Value, "");
                            continue;
                        }
                    }
                    else if (!permissionResult.IsAllowed)
                    {
                        operationResults.Add($"❌ {operation.Operation} {operation.Path}: {permissionResult.Reason}");
                        await _permissionManager.LogOperationAsync(operation, permissionResult, "BLOCKED by policy");
                        processedResponse.Replace(match.Value, "");
                        continue;
                    }

                    // ดำเนินการ
                    try
                    {
                        var result = await ExecuteFileOperationAsync(operation);
                        operationResults.Add($"✅ {operation.Operation} {operation.Path}: {result}");
                        await _permissionManager.LogOperationAsync(operation, permissionResult, "SUCCESS");
                    }
                    catch (Exception ex)
                    {
                        operationResults.Add($"❌ {operation.Operation} {operation.Path}: {ex.Message}");
                        await _permissionManager.LogOperationAsync(operation, permissionResult, $"FAILED: {ex.Message}");
                    }
                    
                    // ลบ file operation block ออกจาก response
                    processedResponse.Replace(match.Value, "");
                }
            }
            catch (Exception ex)
            {
                var debugInfo = $"Original JSON: {match.Groups[1].Value.Trim()}";
                operationResults.Add($"❌ File operation failed: {ex.Message}");
                operationResults.Add($"🔍 Debug: {debugInfo}");
            }
        }

        // เพิ่มผลลัพธ์การดำเนินการไฟล์
        if (operationResults.Any())
        {
            processedResponse.AppendLine("\n=== File Operations Results ===");
            foreach (var result in operationResults)
            {
                processedResponse.AppendLine(result);
            }
        }

        return processedResponse.ToString().Trim();
    }

    private async Task<string> ExecuteFileOperationAsync(FileOperation operation)
    {
        var operationType = operation.GetOperationType();

        switch (operationType)
        {
            case FileOperationType.Read:
                return await _fileService.ReadFileAsync(operation.Path);
            
            case FileOperationType.Write:
            case FileOperationType.Create:
                await _fileService.WriteFileAsync(operation.Path, operation.Content ?? "");
                return "File written successfully";
            
            case FileOperationType.Delete:
                await _fileService.DeleteFileAsync(operation.Path);
                return "File deleted successfully";
            
            case FileOperationType.List:
                var files = await _fileService.ListFilesAsync(operation.Path);
                return $"Files found: {string.Join(", ", files)}";
            
            case FileOperationType.Execute:
                var args = operation.Arguments ?? Array.Empty<string>();
                return await _fileService.ExecuteCommandAsync(operation.Path, args);
            
            default:
                throw new NotSupportedException($"Operation {operation.Operation} not supported");
        }
    }

    public async Task<bool> IsOllamaAvailableAsync()
    {
        try
        {
            var models = await _ollamaService.GetAvailableModelsAsync();
            return models.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    public Task<string[]> GetAvailableModelsAsync()
    {
        return _ollamaService.GetAvailableModelsAsync();
    }

    private static string FixJsonEscaping(string json)
    {
        try
        {
            // First, try to parse as-is to see if it's already valid
            JsonDocument.Parse(json);
            return json;
        }
        catch (JsonException)
        {
            // If parsing fails, try to fix common escaping issues
            var fixedJson = json;
            
            // Fix unescaped newlines in content field
            var contentPattern = @"""content"":\s*""([^""]*?)""";
            var matches = Regex.Matches(fixedJson, contentPattern, RegexOptions.Singleline);
            
            foreach (Match match in matches)
            {
                var originalContent = match.Groups[1].Value;
                var escapedContent = EscapeJsonString(originalContent);
                fixedJson = fixedJson.Replace(match.Value, $@"""content"": ""{escapedContent}""");
            }
            
            // Try alternative parsing approaches
            if (!IsValidJson(fixedJson))
            {
                // Try to reconstruct the JSON manually
                fixedJson = ReconstructJson(json);
            }
            
            return fixedJson;
        }
    }

    private static string EscapeJsonString(string input)
    {
        return input
            .Replace("\\", "\\\\")  // Escape backslashes first
            .Replace("\"", "\\\"")   // Escape quotes
            .Replace("\n", "\\n")    // Escape newlines
            .Replace("\r", "\\r")    // Escape carriage returns
            .Replace("\t", "\\t")    // Escape tabs
            .Replace("\b", "\\b")    // Escape backspaces
            .Replace("\f", "\\f");   // Escape form feeds
    }

    private static bool IsValidJson(string json)
    {
        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string ReconstructJson(string malformedJson)
    {
        // Extract basic components using regex
        var operationMatch = Regex.Match(malformedJson, @"""operation"":\s*""([^""]+)""");
        var pathMatch = Regex.Match(malformedJson, @"""path"":\s*""([^""]+)""");
        
        // For content, we need to be more careful due to multiline issues
        var contentMatch = Regex.Match(malformedJson, @"""content"":\s*""(.*?)""(?=,\s*[""}\]]|$)", RegexOptions.Singleline);
        
        if (!operationMatch.Success || !pathMatch.Success)
        {
            throw new JsonException("Could not extract required operation and path fields");
        }

        var operation = operationMatch.Groups[1].Value;
        var path = pathMatch.Groups[1].Value;
        
        var reconstructed = new StringBuilder();
        reconstructed.Append("{");
        reconstructed.Append($@"""operation"": ""{operation}""");
        reconstructed.Append($@", ""path"": ""{path}""");
        
        if (contentMatch.Success)
        {
            var content = contentMatch.Groups[1].Value;
            var escapedContent = EscapeJsonString(content);
            reconstructed.Append($@", ""content"": ""{escapedContent}""");
        }
        
        // Check for arguments field
        var argsMatch = Regex.Match(malformedJson, @"""arguments"":\s*(\[[^\]]*\])", RegexOptions.Singleline);
        if (argsMatch.Success)
        {
            reconstructed.Append($@", ""arguments"": {argsMatch.Groups[1].Value}");
        }
        
        reconstructed.Append("}");
        
        return reconstructed.ToString();
    }
}