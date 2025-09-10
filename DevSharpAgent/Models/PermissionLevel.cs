using System.Text.Json.Serialization;

namespace DevSharpAgent.Models;

/// <summary>
/// ระดับสิทธิ์การเข้าถึงไฟล์สำหรับ AI
/// </summary>
public enum PermissionLevel
{
    /// <summary>
    /// อ่านไฟล์และแสดงรายการได้เท่านั้น
    /// </summary>
    ReadOnly = 0,

    /// <summary>
    /// สร้างไฟล์ใหม่ได้ แต่การแก้ไข/ลบต้องขออนุญาต
    /// </summary>
    SafeWrite = 1,

    /// <summary>
    /// ทำทุกอย่างได้โดยไม่ต้องขออนุญาต (สำหรับผู้เชี่ยวชาญ)
    /// </summary>
    FullAccess = 2
}

/// <summary>
/// ระดับความเสี่ยงของการดำเนินการไฟล์
/// </summary>
public enum OperationRisk
{
    /// <summary>
    /// ปลอดภัย - อ่านข้อมูลเท่านั้น
    /// </summary>
    Safe,

    /// <summary>
    /// ปานกลาง - สร้างไฟล์หรือข้อมูลใหม่
    /// </summary>
    Moderate,

    /// <summary>
    /// อันตราย - แก้ไขหรือลบข้อมูลที่มีอยู่แล้ว
    /// </summary>
    Dangerous
}

/// <summary>
/// ผลลัพธ์การขออนุญาต
/// </summary>
public class PermissionResult
{
    public bool IsAllowed { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool RequiredConfirmation { get; set; }

    public static PermissionResult Allow(string reason = "")
        => new() { IsAllowed = true, Reason = reason };

    public static PermissionResult Deny(string reason)
        => new() { IsAllowed = false, Reason = reason };

    public static PermissionResult RequireConfirmation(string reason)
        => new() { IsAllowed = false, RequiredConfirmation = true, Reason = reason };
}

/// <summary>
/// การตั้งค่าการขออนุญาต
/// </summary>
public class PermissionSettings
{
    [JsonPropertyName("permissionLevel")]
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.SafeWrite;

    [JsonPropertyName("enableDryRun")]
    public bool EnableDryRun { get; set; } = true;

    [JsonPropertyName("enableAuditLog")]
    public bool EnableAuditLog { get; set; } = true;

    [JsonPropertyName("allowedExtensions")]
    public string[] AllowedExtensions { get; set; } = { ".cs", ".txt", ".json", ".md", ".xml" };

    [JsonPropertyName("blockedPaths")]
    public string[] BlockedPaths { get; set; } = { "bin/", "obj/", ".git/", "node_modules/" };

    [JsonPropertyName("dangerousCommands")]
    public string[] DangerousCommands { get; set; } = { "rm", "del", "format", "fdisk", "shutdown" };
}