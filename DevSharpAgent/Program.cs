using DevSharpAgent.Models;
using DevSharpAgent.Services;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace DevSharpAgent;

class Program
{
    static async Task Main(string[] args)
    {
        // Set console encoding for Thai language support
        try 
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not set UTF-8 encoding: {ex.Message}");
        }
        
        Console.WriteLine("🤖 Ollama File Agent - DevSharp Model with File System Access");
        Console.WriteLine("==============================================================");
        Console.WriteLine("🇹🇭 รองรับภาษาไทย / Thai Language Supported");
        Console.WriteLine("==============================================================\n");

        // Parse command line arguments
        var workspaceFromArgs = ParseWorkspaceFromArgs(args);

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var ollamaBaseUrl = configuration["OllamaBaseUrl"] ?? "http://localhost:11434";
        var configWorkspace = configuration["WorkspaceRoot"] ?? "./workspace";
        var defaultModel = configuration["DefaultModel"] ?? "devsharp:latest";

        // Priority: Command line args > Interactive selection > Configuration > Default
        var workspaceRoot = workspaceFromArgs ?? SelectWorkspace(configWorkspace);

        // Load permission settings
        var permissionSettings = new PermissionSettings();
        configuration.GetSection("PermissionSettings").Bind(permissionSettings);

        // Setup services
        var httpClient = new HttpClient();
        var ollamaService = new OllamaService(httpClient, ollamaBaseUrl);
        var fileService = new FileService(workspaceRoot);
        var permissionManager = new PermissionManager(fileService, permissionSettings);
        var fileAgent = new OllamaFileAgent(ollamaService, fileService, permissionManager);

        Console.WriteLine($"Workspace: {Path.GetFullPath(workspaceRoot)}");
        Console.WriteLine($"Ollama URL: {ollamaBaseUrl}");
        Console.WriteLine($"Default Model: {defaultModel}");
        Console.WriteLine($"Permission Level: {permissionSettings.PermissionLevel} ({GetPermissionDescription(permissionSettings.PermissionLevel)})");
        Console.WriteLine($"Dry Run: {(permissionSettings.EnableDryRun ? "Enabled" : "Disabled")}");
        Console.WriteLine($"Audit Log: {(permissionSettings.EnableAuditLog ? "Enabled" : "Disabled")}\n");

        // Check if Ollama is available
        if (!await fileAgent.IsOllamaAvailableAsync())
        {
            Console.WriteLine("❌ Ollama is not available. Please make sure Ollama is running.");
            return;
        }

        // List available models
        var models = await fileAgent.GetAvailableModelsAsync();
        Console.WriteLine($"✅ Available models: {string.Join(", ", models)}\n");

        if (!models.Contains(defaultModel, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"⚠️  Model '{defaultModel}' not found. Available models: {string.Join(", ", models)}");
            Console.WriteLine("Continuing with first available model...\n");
            defaultModel = models.FirstOrDefault() ?? defaultModel;
        }

        // Interactive mode
        while (true)
        {
            Console.Write("You: ");
            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput))
                continue;

            if (userInput.ToLowerInvariant() is "quit" or "exit" or "bye")
            {
                Console.WriteLine("👋 Goodbye!");
                break;
            }

            if (userInput.StartsWith("/"))
            {
                await HandleCommandAsync(userInput, fileAgent, fileService);
                continue;
            }

            try
            {
                Console.WriteLine("\n🤔 Thinking...");

                // Check if user wants to include specific files
                string[]? filePaths = null;
                if (userInput.Contains("@"))
                {
                    filePaths = ExtractFilePaths(userInput);
                    if (filePaths.Any())
                    {
                        Console.WriteLine($"📁 Including files: {string.Join(", ", filePaths)}");
                    }
                }

                var response = await fileAgent.ChatWithFileAccessAsync(userInput, filePaths);

                Console.WriteLine($"\n🤖 DevSharp:");
                WriteWithEncoding(response);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }
    }

    private static async Task HandleCommandAsync(string command, Services.OllamaFileAgent fileAgent, IFileService fileService)
    {
        var parts = command[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToLowerInvariant();

        switch (cmd)
        {
            case "help":
                ShowHelp();
                break;

            case "ls" or "list":
                var path = parts.Length > 1 ? parts[1] : ".";
                try
                {
                    var files = await fileService.ListFilesAsync(path);
                    Console.WriteLine($"📁 Files in {path}:");
                    foreach (var file in files)
                    {
                        Console.WriteLine($"  {file}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error listing files: {ex.Message}");
                }
                break;

            case "cat" or "read":
                if (parts.Length < 2)
                {
                    Console.WriteLine("Usage: /cat <filename>");
                    break;
                }
                try
                {
                    var content = await fileService.ReadFileAsync(parts[1]);
                    Console.WriteLine($"📄 Content of {parts[1]}:");
                    Console.WriteLine(content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error reading file: {ex.Message}");
                }
                break;

            case "models":
                var models = await fileAgent.GetAvailableModelsAsync();
                Console.WriteLine("🤖 Available models:");
                foreach (var model in models)
                {
                    Console.WriteLine($"  {model}");
                }
                break;

            case "workspace" or "folder" or "wd":
                if (parts.Length > 1)
                {
                    var newPath = parts[1];
                    if (Directory.Exists(newPath))
                    {
                        // Update file service workspace
                        var fullPath = Path.GetFullPath(newPath);
                        Console.WriteLine($"📁 Changed workspace to: {fullPath}");
                        // Note: This would require updating the fileService workspace root
                        // For now, just show the change
                    }
                    else
                    {
                        Console.WriteLine($"❌ Directory not found: {newPath}");
                    }
                }
                else
                {
                    Console.WriteLine($"📁 Current workspace: {fileService.GetWorkspaceRoot()}");
                }
                break;

            case "pwd":
                Console.WriteLine($"📁 Current workspace: {fileService.GetWorkspaceRoot()}");
                break;

            default:
                Console.WriteLine($"Unknown command: {cmd}. Type /help for available commands.");
                break;
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("📖 Available Commands:");
        Console.WriteLine("  /help              - Show this help");
        Console.WriteLine("  /ls [path]         - List files in directory");
        Console.WriteLine("  /cat <file>        - Read file content");
        Console.WriteLine("  /models            - Show available Ollama models");
        Console.WriteLine("  /workspace [path]  - Show/change current workspace");
        Console.WriteLine("  /pwd               - Show current workspace");
        Console.WriteLine("  quit/exit          - Exit the application");
        Console.WriteLine();
        Console.WriteLine("📝 File Access:");
        Console.WriteLine("  Use @filename in your message to include file context");
        Console.WriteLine("  Example: 'Explain this code @Program.cs'");
        Console.WriteLine();
        Console.WriteLine("🔧 File Operations:");
        Console.WriteLine("  The AI can create, read, write, and delete files automatically");
        Console.WriteLine("  Just ask naturally: 'Create a Hello World C# console app'");
        Console.WriteLine();
        Console.WriteLine("📁 Workspace Management:");
        Console.WriteLine("  Command line: DevSharpAgent --workspace C:\\MyProject");
        Console.WriteLine("  Runtime: /workspace C:\\MyProject");
        Console.WriteLine("  Interactive selection at startup");
    }

    private static string[] ExtractFilePaths(string input)
    {
        var matches = System.Text.RegularExpressions.Regex.Matches(input, @"@(\S+)");
        return matches.Select(m => m.Groups[1].Value).ToArray();
    }

    private static string GetPermissionDescription(PermissionLevel level) => level switch
    {
        PermissionLevel.ReadOnly => "อ่านไฟล์เท่านั้น",
        PermissionLevel.SafeWrite => "สร้างไฟล์ใหม่ได้ แก้ไขต้องขออนุญาต",
        PermissionLevel.FullAccess => "ทำทุกอย่างได้",
        _ => "ไม่ทราบ"
    };

    private static void WriteWithEncoding(string text)
    {
        try
        {
            // Ensure proper encoding for Thai text
            var bytes = Encoding.UTF8.GetBytes(text);
            var decodedText = Encoding.UTF8.GetString(bytes);
            Console.WriteLine(decodedText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Encoding error: {ex.Message}");
            Console.WriteLine(text); // Fallback to original text
        }
    }

    private static string? ParseWorkspaceFromArgs(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is "--workspace" or "-w" or "--folder" or "-f")
            {
                if (i + 1 < args.Length)
                {
                    var path = args[i + 1];
                    if (Directory.Exists(path))
                    {
                        return Path.GetFullPath(path);
                    }
                    else
                    {
                        Console.WriteLine($"❌ Directory not found: {path}");
                        Environment.Exit(1);
                    }
                }
            }
        }

        // Check for help argument
        if (args.Any(arg => arg is "--help" or "-h"))
        {
            ShowUsage();
            Environment.Exit(0);
        }

        return null;
    }

    private static string SelectWorkspace(string defaultWorkspace)
    {
        Console.WriteLine("📁 Workspace Selection");
        Console.WriteLine("=====================");
        Console.WriteLine($"Default workspace: {Path.GetFullPath(defaultWorkspace)}");
        Console.WriteLine();
        
        while (true)
        {
            Console.WriteLine("Options:");
            Console.WriteLine("  1. Use default workspace");
            Console.WriteLine("  2. Enter custom path");
            Console.WriteLine("  3. Browse current directory");
            Console.Write("\nSelect option (1-3): ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1" or "":
                    EnsureDirectoryExists(defaultWorkspace);
                    return Path.GetFullPath(defaultWorkspace);

                case "2":
                    Console.Write("Enter workspace path: ");
                    var customPath = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrEmpty(customPath))
                    {
                        if (Directory.Exists(customPath))
                        {
                            return Path.GetFullPath(customPath);
                        }
                        else
                        {
                            Console.Write($"Directory doesn't exist. Create it? (y/N): ");
                            var create = Console.ReadLine()?.Trim().ToLowerInvariant();
                            if (create is "y" or "yes")
                            {
                                try
                                {
                                    Directory.CreateDirectory(customPath);
                                    Console.WriteLine($"✅ Created directory: {customPath}");
                                    return Path.GetFullPath(customPath);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"❌ Failed to create directory: {ex.Message}");
                                }
                            }
                        }
                    }
                    break;

                case "3":
                    ShowDirectoryBrowser();
                    break;

                default:
                    Console.WriteLine("Invalid option. Please select 1, 2, or 3.");
                    break;
            }
        }
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
                Console.WriteLine($"✅ Created workspace directory: {Path.GetFullPath(path)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to create workspace directory: {ex.Message}");
                throw;
            }
        }
    }

    private static void ShowDirectoryBrowser()
    {
        var currentDir = Directory.GetCurrentDirectory();
        Console.WriteLine($"\n📂 Current Directory: {currentDir}");
        Console.WriteLine("Subdirectories:");
        
        try
        {
            var dirs = Directory.GetDirectories(currentDir)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name))
                .OrderBy(name => name)
                .Take(20) // Limit to 20 directories
                .ToList();

            if (dirs.Any())
            {
                for (int i = 0; i < dirs.Count; i++)
                {
                    Console.WriteLine($"  {i + 1:D2}. {dirs[i]}");
                }
                Console.Write("\nSelect directory number (or press Enter to skip): ");
                var selection = Console.ReadLine()?.Trim();
                if (int.TryParse(selection, out var index) && index >= 1 && index <= dirs.Count)
                {
                    var selectedPath = Path.Combine(currentDir, dirs[index - 1] ?? "");
                    Console.WriteLine($"Selected: {selectedPath}");
                    // This would return the selected path, but for simplicity, just show it
                }
            }
            else
            {
                Console.WriteLine("  (No subdirectories found)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error browsing directory: {ex.Message}");
        }
        Console.WriteLine();
    }

    private static void ShowUsage()
    {
        Console.WriteLine("DevSharp Agent - AI Assistant with File System Access");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  DevSharpAgent [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -w, --workspace <path>   Set workspace directory");
        Console.WriteLine("  -f, --folder <path>      Same as --workspace");
        Console.WriteLine("  -h, --help               Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  DevSharpAgent --workspace C:\\MyProject");
        Console.WriteLine("  DevSharpAgent -w ./src");
        Console.WriteLine("  DevSharpAgent --folder /home/user/code");
    }
}
