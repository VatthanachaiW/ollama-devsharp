using DevSharpAgent.Models;

namespace DevSharpAgent.Services;

/// <summary>
/// Interface สำหรับจัดการสิทธิ์การเข้าถึงไฟล์
/// </summary>
public interface IPermissionManager
{
    /// <summary>
    /// ตรวจสอบว่าการดำเนินการได้รับอนุญาตหรือไม่
    /// </summary>
    Task<PermissionResult> CheckPermissionAsync(FileOperation operation);

    /// <summary>
    /// ขออนุญาตจากผู้ใช้สำหรับการดำเนินการที่เสี่ยง
    /// </summary>
    Task<bool> RequestUserPermissionAsync(FileOperation operation, OperationRisk risk);

    /// <summary>
    /// แสดงตัวอย่างการดำเนินการก่อนดำเนินการจริง (Dry Run)
    /// </summary>
    Task<string> PreviewOperationAsync(FileOperation operation);

    /// <summary>
    /// บันทึกการดำเนินการไว้ใน audit log
    /// </summary>
    Task LogOperationAsync(FileOperation operation, PermissionResult result, string outcome);

    /// <summary>
    /// ประเมินระดับความเสี่ยงของการดำเนินการ
    /// </summary>
    OperationRisk EvaluateRisk(FileOperation operation);

    /// <summary>
    /// ตรวจสอบว่าไฟล์หรือคำสั่งอยู่ในรายการที่ถูกบล็อกหรือไม่
    /// </summary>
    bool IsBlocked(FileOperation operation);

    /// <summary>
    /// อัพเดทการตั้งค่าสิทธิ์
    /// </summary>
    void UpdateSettings(PermissionSettings settings);

    /// <summary>
    /// รับการตั้งค่าสิทธิ์ปัจจุบัน
    /// </summary>
    PermissionSettings GetCurrentSettings();
}