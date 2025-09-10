using System.Text;
using DevSharpAgent.Models;

namespace DevSharpAgent.Services;

/// <summary>
/// จัดการสิทธิ์การเข้าถึงไฟล์สำหรับ AI
/// </summary>
public class PermissionManager : IPermissionManager
{
    private readonly IFileService _fileService;
    private PermissionSettings _settings;
    private readonly string _auditLogPath;

    public PermissionManager(IFileService fileService, PermissionSettings? settings = null)
    {
        _fileService = fileService;
        _settings = settings ?? new PermissionSettings();
        _auditLogPath = Path.Combine(_fileService.GetWorkspaceRoot(), "audit.log");
    }

    public Task<PermissionResult> CheckPermissionAsync(FileOperation operation)
    {
        // ตรวจสอบว่าถูกบล็อกหรือไม่
        if (IsBlocked(operation))
        {
            return Task.FromResult(PermissionResult.Deny($"การดำเนินการ {operation.Operation} ถูกบล็อกโดย security policy"));
        }

        var risk = EvaluateRisk(operation);
        
        // ตรวจสอบตาม permission level
        var result = _settings.PermissionLevel switch
        {
            PermissionLevel.ReadOnly => risk == OperationRisk.Safe 
                ? PermissionResult.Allow("Read-only operation")
                : PermissionResult.Deny("Permission level ไม่อนุญาตให้แก้ไขไฟล์"),

            PermissionLevel.SafeWrite => risk switch
            {
                OperationRisk.Safe => PermissionResult.Allow("Safe operation"),
                OperationRisk.Moderate => PermissionResult.Allow("Moderate operation allowed"),
                OperationRisk.Dangerous => PermissionResult.RequireConfirmation(
                    $"การดำเนินการ {operation.Operation} มีความเสี่ยง ต้องได้รับอนุญาตจากผู้ใช้"),
                _ => PermissionResult.Deny("Unknown risk level")
            },

            PermissionLevel.FullAccess => PermissionResult.Allow("Full access granted"),

            _ => PermissionResult.Deny("Invalid permission level")
        };

        return Task.FromResult(result);
    }

    public async Task<bool> RequestUserPermissionAsync(FileOperation operation, OperationRisk risk)
    {
        var riskIcon = risk switch
        {
            OperationRisk.Moderate => "⚠️ ",
            OperationRisk.Dangerous => "🚨",
            _ => "ℹ️ "
        };

        Console.WriteLine();
        Console.WriteLine($"{riskIcon} AI ขออนุญาต:");
        Console.WriteLine($"   การดำเนินการ: {operation.Operation}");
        Console.WriteLine($"   เป้าหมาย: {operation.Path}");
        
        if (operation.Operation.ToUpperInvariant() == "WRITE_FILE" && !string.IsNullOrEmpty(operation.Content))
        {
            var preview = operation.Content.Length > 200 
                ? operation.Content[..200] + "..."
                : operation.Content;
            Console.WriteLine($"   เนื้อหา: {preview}");
        }
        
        if (operation.Operation.ToUpperInvariant() == "EXECUTE")
        {
            Console.WriteLine($"   คำสั่ง: {operation.Path}");
            if (operation.Arguments?.Any() == true)
            {
                Console.WriteLine($"   อาร์กิวเมนต์: {string.Join(" ", operation.Arguments)}");
            }
        }

        Console.WriteLine($"   ระดับเสี่ยง: {GetRiskDescription(risk)}");
        Console.WriteLine();
        
        // แสดงตัวอย่างถ้าเปิดใช้ dry run
        if (_settings.EnableDryRun)
        {
            var preview = await PreviewOperationAsync(operation);
            Console.WriteLine($"🔍 ตัวอย่าง: {preview}");
            Console.WriteLine();
        }

        Console.Write("🤔 อนุญาต? (y/n/a=always): ");
        var response = Console.ReadLine()?.ToLowerInvariant().Trim();

        switch (response)
        {
            case "y" or "yes":
                return true;
            case "a" or "always":
                // อัพเกรดเป็น FullAccess ชั่วคราว (สำหรับ session นี้)
                Console.WriteLine("✅ เปลี่ยนเป็น Full Access สำหรับ session นี้");
                _settings.PermissionLevel = PermissionLevel.FullAccess;
                return true;
            case "n" or "no" or "":
            default:
                return false;
        }
    }

    public async Task<string> PreviewOperationAsync(FileOperation operation)
    {
        try
        {
            return operation.GetOperationType() switch
            {
                FileOperationType.Read => _fileService.FileExists(operation.Path) 
                    ? $"อ่านไฟล์ {operation.Path} ({await GetFileSizeAsync(operation.Path)} bytes)"
                    : $"ไฟล์ {operation.Path} ไม่พบ",
                
                FileOperationType.Write or FileOperationType.Create => 
                    _fileService.FileExists(operation.Path)
                        ? $"แก้ไขไฟล์เดิม {operation.Path} ({await GetFileSizeAsync(operation.Path)} -> {operation.Content?.Length ?? 0} bytes)"
                        : $"สร้างไฟล์ใหม่ {operation.Path} ({operation.Content?.Length ?? 0} bytes)",
                
                FileOperationType.Delete => _fileService.FileExists(operation.Path)
                    ? $"ลบไฟล์ {operation.Path} ({await GetFileSizeAsync(operation.Path)} bytes)"
                    : $"ไฟล์ {operation.Path} ไม่พบ (ไม่สามารถลบได้)",
                
                FileOperationType.List => _fileService.DirectoryExists(operation.Path)
                    ? $"แสดงรายการไฟล์ในโฟลเดอร์ {operation.Path}"
                    : $"โฟลเดอร์ {operation.Path} ไม่พบ",
                
                FileOperationType.Execute => 
                    $"รันคำสั่ง: {operation.Path} {string.Join(" ", operation.Arguments ?? Array.Empty<string>())}",
                
                _ => $"การดำเนินการ {operation.Operation} ไม่รู้จัก"
            };
        }
        catch (Exception ex)
        {
            return $"ไม่สามารถแสดงตัวอย่างได้: {ex.Message}";
        }
    }

    public async Task LogOperationAsync(FileOperation operation, PermissionResult result, string outcome)
    {
        if (!_settings.EnableAuditLog) return;

        try
        {
            var logEntry = new StringBuilder();
            logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {operation.Operation} {operation.Path}");
            logEntry.AppendLine($"  Permission: {(result.IsAllowed ? "ALLOWED" : "DENIED")} - {result.Reason}");
            logEntry.AppendLine($"  Outcome: {outcome}");
            logEntry.AppendLine($"  Risk Level: {EvaluateRisk(operation)}");
            logEntry.AppendLine($"  User Permission Level: {_settings.PermissionLevel}");
            logEntry.AppendLine();

            await File.AppendAllTextAsync(_auditLogPath, logEntry.ToString());
        }
        catch
        {
            // ไม่ให้ audit log error ไปกระทบการทำงานหลัก
        }
    }

    public OperationRisk EvaluateRisk(FileOperation operation)
    {
        var operationType = operation.GetOperationType();

        return operationType switch
        {
            FileOperationType.Read or FileOperationType.List => OperationRisk.Safe,
            
            FileOperationType.Write or FileOperationType.Create => 
                _fileService.FileExists(operation.Path) ? OperationRisk.Dangerous : OperationRisk.Moderate,
            
            FileOperationType.Delete => OperationRisk.Dangerous,
            
            FileOperationType.Execute => IsDangerousCommand(operation.Path) 
                ? OperationRisk.Dangerous 
                : OperationRisk.Moderate,
            
            _ => OperationRisk.Dangerous
        };
    }

    public bool IsBlocked(FileOperation operation)
    {
        var path = operation.Path.ToLowerInvariant();
        
        // ตรวจสอบ blocked paths
        if (_settings.BlockedPaths.Any(blockedPath => 
            path.Contains(blockedPath.ToLowerInvariant())))
        {
            return true;
        }

        // ตรวจสอบ file extension (สำหรับการเขียนไฟล์)
        if (operation.GetOperationType() == FileOperationType.Write ||
            operation.GetOperationType() == FileOperationType.Create)
        {
            var extension = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(extension) && 
                !_settings.AllowedExtensions.Contains(extension.ToLowerInvariant()))
            {
                return true;
            }
        }

        // ตรวจสอบ dangerous commands
        if (operation.GetOperationType() == FileOperationType.Execute)
        {
            return IsDangerousCommand(operation.Path);
        }

        return false;
    }

    public void UpdateSettings(PermissionSettings settings)
    {
        _settings = settings;
    }

    public PermissionSettings GetCurrentSettings()
    {
        return _settings;
    }

    private async Task<long> GetFileSizeAsync(string path)
    {
        try
        {
            if (_fileService.FileExists(path))
            {
                var content = await _fileService.ReadFileAsync(path);
                return content.Length;
            }
        }
        catch { }
        return 0;
    }

    private bool IsDangerousCommand(string command)
    {
        var cmd = Path.GetFileNameWithoutExtension(command).ToLowerInvariant();
        return _settings.DangerousCommands.Any(dangerous => 
            cmd.Contains(dangerous.ToLowerInvariant()));
    }

    private static string GetRiskDescription(OperationRisk risk) => risk switch
    {
        OperationRisk.Safe => "ปลอดภัย (อ่านข้อมูลเท่านั้น)",
        OperationRisk.Moderate => "ปานกลาง (สร้างข้อมูลใหม่)",
        OperationRisk.Dangerous => "อันตราย (แก้ไข/ลบข้อมูลเดิม)",
        _ => "ไม่ทราบ"
    };
}