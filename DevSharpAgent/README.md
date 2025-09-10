# Ollama File Agent

เครื่องมือที่ช่วยให้ Ollama model (DevSharp) สามารถอ่านเขียนไฟล์ได้เหมือน Claude Code

## ความสามารถ

### 🤖 AI Assistant with File Access
- ใช้ DevSharp model ที่เป็นผู้เชี่ยวชาญด้าน C#/.NET
- สามารถอ่าน เขียน สร้าง และลบไฟล์ได้
- รันคำสั่งระบบได้
- มี workspace ที่ปลอดภัยป้องกัน path traversal

### 📁 File Operations
- `READ_FILE(path)` - อ่านไฟล์
- `WRITE_FILE(path, content)` - เขียน/สร้างไฟล์
- `DELETE_FILE(path)` - ลบไฟล์
- `LIST_FILES(path)` - แสดงรายการไฟล์
- `EXECUTE(command, args)` - รันคำสั่ง

### 🎯 Interactive Features
- Chat แบบโต้ตอบ
- รวมไฟล์เข้าใน context ด้วย `@filename`
- คำสั่งพิเศษ: `/help`, `/ls`, `/cat`, `/models`
- Auto-detection ของ file operations

## การติดตั้งและใช้งาน

### 1. Build และ Run
```bash
cd OllamaFileAgent
dotnet build
dotnet run
```

### 2. Configuration
แก้ไข `appsettings.json`:
```json
{
  "OllamaBaseUrl": "http://localhost:11434",
  "WorkspaceRoot": "./workspace", 
  "DefaultModel": "devsharp"
}
```

### 3. การระบุ Workspace (ไดเร็กทอรีทำงาน)

#### วิธีที่ 1: Command Line Arguments
```bash
# ระบุ workspace ด้วย --workspace หรือ -w
dotnet run -- --workspace "C:\MyProject"
dotnet run -- -w "./src"

# ดู help
dotnet run -- --help
```

#### วิธีที่ 2: Interactive Selection
เมื่อเริ่มโปรแกรม จะมีตัวเลือก:
1. ใช้ workspace default
2. พิมพ์ path เอง
3. เลือกจากไดเร็กทอรีย่อย

#### วิธีที่ 3: Runtime Commands
```
You: /workspace "C:\NewProject"  # เปลี่ยน workspace
You: /pwd                       # แสดง workspace ปัจจุบัน
```

### 4. ตัวอย่างการใช้งาน

#### การสร้างโปรเจกต์ใหม่
```
You: สร้าง Hello World C# console app
```

AI จะสร้างไฟล์ `.cs` พร้อมโค้ดให้อัตโนมัติ

#### การอ่านไฟล์และแก้ไข
```
You: อธิบายโค้ดนี้ @Program.cs แล้วปรับปรุงให้ดีขึ้น
```

AI จะอ่านไฟล์ อธิบายโค้ด แล้วสร้างเวอร์ชันที่ปรับปรุงแล้ว

#### การใช้คำสั่งพิเศษ
```
You: /ls              # แสดงไฟล์ในไดเร็กทอรี
You: /cat Program.cs  # อ่านไฟล์
You: /models          # แสดง model ที่มี
You: /workspace       # แสดง workspace ปัจจุบัน
You: /help            # แสดงคำสั่งทั้งหมด
```

## โครงสร้างโปรเจกต์

```
OllamaFileAgent/
├── Models/
│   ├── OllamaRequest.cs    # Request model
│   ├── OllamaResponse.cs   # Response model
│   └── FileOperation.cs    # File operation model
├── Services/
│   ├── IFileService.cs     # File service interface
│   ├── FileService.cs      # File operations implementation
│   ├── IOllamaService.cs   # Ollama service interface
│   ├── OllamaService.cs    # Ollama API client
│   └── OllamaFileAgent.cs  # Main agent class
├── Program.cs              # Entry point
├── appsettings.json        # Configuration
└── README.md              # This file
```

## Security Features

- **Workspace Isolation**: ทุกการดำเนินการไฟล์จำกัดอยู่ใน workspace
- **Path Traversal Protection**: ป้องกันการเข้าถึงไฟล์นอก workspace
- **Safe Command Execution**: รันคำสั่งใน controlled environment

## Requirements

- .NET 9.0+
- Ollama running on localhost:11434
- DevSharp model installed in Ollama

## การแก้ปัญหาภาษาไทย / Thai Language Issues

หากพบปัญหาการแสดงผลภาษาไทยใน console:

### Windows Command Prompt
```cmd
chcp 65001
```

### Windows Terminal / PowerShell
ตั้งค่า Font เป็น font ที่รองรับภาษาไทย เช่น:
- Cascadia Code
- Consolas
- Noto Sans Thai

### Linux/Mac Terminal
ตั้งค่า locale:
```bash
export LC_ALL=th_TH.UTF-8
export LANG=th_TH.UTF-8
```

## ตัวอย่าง File Operations Format

AI จะใช้รูปแบบนี้ในการดำเนินการไฟล์:

```fileop
{"operation": "WRITE_FILE", "path": "HelloWorld.cs", "content": "using System;\n\nclass Program\n{\n    static void Main()\n    {\n        Console.WriteLine(\"Hello World!\");\n    }\n}"}
```

Application จะ parse และดำเนินการไฟล์อัตโนมัติ