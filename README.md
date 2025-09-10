# DevSharp - ผู้ช่วย AI สำหรับนักพัฒนา C#/.NET

<div align="center">

🤖 **DevSharp AI Assistant** - นักพัฒนา C#/.NET ที่ขับเคลื่อนด้วย AI  
🚀 **ระบบจัดการไฟล์อัตโนมัติ** - การทำงานกับไฟล์แบบอัจฉริยะ  
🇹🇭 **รองรับภาษาไทย** - ตอบโต้และเข้าใจภาษาไทยได้อย่างสมบูรณ์

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Ollama](https://img.shields.io/badge/Ollama-llama3.1%3A8b-green.svg)

</div>

## 🎯 ภาพรวมโปรเจค

DevSharp เป็นระบบ AI Assistant ที่เชี่ยวชาญด้าน C#/.NET Development ประกอบด้วย 2 ส่วนหลัก:

### 1. DevSharp Model (Ollama)
- โมเดล AI ที่ปรับแต่งมาเฉพาะ C#/.NET Development
- ฐานจาก **llama3.1:8b** พร้อมการปรับแต่งเฉพาะทาง
- มีตัวอย่างโค้ด C# มากกว่า **8,482 บรรทัด**
- รองรับทั้งภาษาไทยและอังกฤษ

### 2. DevSharpAgent (File Operations)
- แอปพลิเคชัน C# Console สำหรับจัดการไฟล์
- เชื่อมต่อกับ Ollama Model เพื่อทำงานร่วมกัน
- ระบบความปลอดภัยและ Permission Management
- รองรับคำสั่งแบบ Interactive Chat

## ✨ ความสามารถหลัก

### 🧠 AI Development Assistant
- **Design Patterns**: Repository, Factory, Strategy, Observer, Decorator, SOLID
- **Architecture**: Clean Architecture, DDD, CQRS, Event Sourcing, Microservices
- **Modern .NET**: ASP.NET Core, EF Core, gRPC, SignalR, Minimal APIs, Blazor
- **Testing**: Unit Testing, Integration Testing, Performance Testing (NBomber), BDD (SpecFlow)
- **Cloud Native**: Azure, AWS, Kubernetes, Docker, Service Mesh
- **Security**: OAuth2/JWT, API Security, Authentication & Authorization

### 📁 File Operations
- **อ่านไฟล์**: `READ_FILE(path)` - อ่านเนื้อหาไฟล์
- **เขียนไฟล์**: `WRITE_FILE(path, content)` - สร้าง/แก้ไขไฟล์
- **ลบไฟล์**: `DELETE_FILE(path)` - ลบไฟล์
- **แสดงรายการ**: `LIST_FILES(path)` - แสดงไฟล์ในโฟลเดอร์
- **รันคำสั่ง**: `EXECUTE(command, args)` - รันคำสั่ง dotnet, build, test

### 🛡️ ระบบความปลอดภัย
- **Workspace Isolation**: จำกัดการเข้าถึงไฟล์ในขอบเขตที่กำหนด
- **Path Traversal Protection**: ป้องกันการเข้าถึงไฟล์นอกขอบเขต
- **Permission Management**: ระบบจัดการสิทธิ์การเข้าถึง
- **Safe Command Execution**: การรันคำสั่งในสภาพแวดล้อมที่ปลอดภัย

## 🚀 การติดตั้งและใช้งาน

### ข้อกำหนดระบบ
- **.NET 9.0** หรือใหม่กว่า
- **Ollama** ติดตั้งและรันอยู่ที่ localhost:11434
- **Windows/Linux/Mac** รองรับทุกแพลตฟอร์ม

### 1. ติดตั้ง Ollama และสร้าง DevSharp Model

```bash
# ติดตั้ง Ollama (หากยังไม่มี)
# Windows: ดาวน์โหลดจาก https://ollama.ai
# macOS: brew install ollama
# Linux: curl https://ollama.ai/install.sh | sh

# สร้าง DevSharp Model
ollama create devsharp -f Modelfile

# หรือสร้าง Simple Version
ollama create devsharp-simple -f Modelfile.simple
```

### 2. เรียกใช้ DevSharpAgent

```bash
# Clone repository
git clone <repository-url>
cd devsharp

# Build และ Run DevSharpAgent
cd DevSharpAgent
dotnet build
dotnet run
```

### 3. การกำหนดค่า (Configuration)

แก้ไขไฟล์ `DevSharpAgent/appsettings.json`:

```json
{
  "OllamaBaseUrl": "http://localhost:11434",
  "WorkspaceRoot": "./workspace",
  "DefaultModel": "devsharp",
  "PermissionSettings": {
    "AllowedFileOperations": ["READ", "WRITE", "DELETE", "LIST"],
    "RestrictedPaths": [".git", "node_modules"]
  }
}
```

## 💡 ตัวอย่างการใช้งาน

### การสร้างโปรเจกต์ใหม่
```bash
# เริ่ม DevSharpAgent
cd DevSharpAgent
dotnet run

# ใน Chat Interface
You: สร้าง Hello World C# console application พร้อม unit test
```

DevSharp จะสร้างไฟล์ต่อไปนี้อัตโนมัติ:
- `Program.cs` - Hello World application
- `HelloWorld.Tests/HelloWorldTests.cs` - Unit tests
- `HelloWorld.csproj` - Project file

### การวิเคราะห์และปรับปรุงโค้ด
```bash
You: อ่านไฟล์นี้ @Program.cs แล้วปรับปรุงให้เป็น Clean Architecture
```

### การใช้คำสั่งพิเศษ
```bash
You: /help           # แสดงความช่วยเหลือ
You: /ls             # แสดงไฟล์ในไดเร็กทอรี
You: /cat Program.cs # อ่านเนื้อหาไฟล์
You: /models         # แสดง Ollama models
You: /workspace "C:\MyProject"  # เปลี่ยน workspace
You: /pwd            # แสดง workspace ปัจจุบัน
```

### การรวมไฟล์ใน Context
```bash
You: อธิบายโค้ดนี้ @Services/UserService.cs @Models/User.cs แล้วเขียน unit test
```

## 🏗️ สถาปัตยกรรมระบบ

### DevSharp Model
```
Modelfile
├── System Prompt (15+ years C# expert)
├── Temperature: 0.12 (consistent code generation)
├── Context Window: 8192 tokens
├── Examples: 8,482+ lines of C# patterns
└── Response Format: Thai/English auto-detection
```

### DevSharpAgent Architecture
```
DevSharpAgent/
├── Models/
│   ├── FileOperation.cs      # File operation models
│   ├── OllamaRequest.cs      # API request models
│   ├── OllamaResponse.cs     # API response models
│   └── PermissionLevel.cs    # Permission system
├── Services/
│   ├── FileService.cs        # File operations with security
│   ├── OllamaService.cs      # HTTP client for Ollama API
│   ├── OllamaFileAgent.cs    # Main orchestration service
│   └── PermissionManager.cs  # Security and access control
└── Program.cs                # Entry point with workspace selection
```

### การทำงานของระบบ
1. **User Input** → DevSharpAgent รับคำสั่งจากผู้ใช้
2. **AI Processing** → ส่งคำสั่งไปยัง DevSharp Model ใน Ollama
3. **Response Analysis** → วิเคราะห์การตอบกลับและตรวจหา File Operations
4. **File Execution** → ดำเนินการไฟล์ตาม JSON format ที่กำหนด
5. **Security Check** → ตรวจสอบสิทธิ์และความปลอดภัย
6. **Result Display** → แสดงผลลัพธ์ให้ผู้ใช้

## 🎯 กรณีการใช้งาน

### สำหรับนักพัฒนา C#/.NET
- **สร้างโปรเจกต์ใหม่**: Console, Web API, Blazor, gRPC
- **วิเคราะห์โค้ด**: Code review, refactoring, optimization
- **เขียน Unit Tests**: xUnit, NUnit, MSTest พร้อม mocking
- **ปรับปรุง Architecture**: Clean Architecture, DDD, CQRS

### สำหรับ Team Lead / Architect
- **Design Patterns**: Implementation และ best practices
- **Performance Tuning**: Memory optimization, caching strategies
- **Security Review**: Authentication, authorization, security patterns
- **Cloud Migration**: Azure, AWS integration patterns

### สำหรับ DevOps
- **CI/CD Pipelines**: GitHub Actions, Azure DevOps
- **Containerization**: Docker, Kubernetes deployment
- **Monitoring**: OpenTelemetry, Application Insights
- **Infrastructure**: ARM templates, Terraform

## 🔧 คำสั่งพัฒนาที่สำคัญ

### Model Management
```bash
# สร้าง/อัปเดต model
ollama create devsharp -f Modelfile

# รัน model แบบ interactive
ollama run devsharp "สร้าง Web API พร้อม Entity Framework"

# แสดงข้อมูล model
ollama show devsharp

# ลบ model
ollama rm devsharp
```

### DevSharpAgent Development
```bash
# Build และ run
cd DevSharpAgent
dotnet build
dotnet run

# Run พร้อม workspace
dotnet run -- --workspace "C:\MyProject"
dotnet run -- -w "./src"

# Development mode
dotnet watch run

# Clean build
dotnet clean
```

### Testing และ Development
```bash
# สร้าง test project
dotnet new xunit -n MyProject.Tests
dotnet add MyProject.Tests reference MyProject

# Run tests
dotnet test
dotnet test --filter "Category=Integration"

# Entity Framework
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 🌟 ตัวอย่าง Output

### การสร้าง Web API
```bash
You: สร้าง Product Web API พร้อม Entity Framework และ Unit Tests

DevSharp: สร้าง Product Web API พร้อม EF Core และ xUnit Tests

📁 Files Created:
- Controllers/ProductController.cs
- Models/Product.cs  
- Data/ApplicationDbContext.cs
- Tests/ProductControllerTests.cs
- Program.cs
- appsettings.json

🔧 Commands to run:
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.InMemoryDatabase
dotnet build
dotnet test
dotnet run
```

## 🤝 การสนับสนุน

### ภาษาไทยใน Console
หากพบปัญหาการแสดงผลภาษาไทย:

**Windows:**
```cmd
chcp 65001
```

**PowerShell/Terminal:**
- ตั้งค่า Font: Cascadia Code, Consolas, หรือ Noto Sans Thai

**Linux/Mac:**
```bash
export LC_ALL=th_TH.UTF-8
export LANG=th_TH.UTF-8
```

### การแก้ปัญหา
1. **Ollama ไม่ทำงาน**: ตรวจสอบ `ollama serve` รันอยู่
2. **Model ไม่พบ**: รัน `ollama list` เพื่อตรวจสอบ
3. **Permission Error**: ตรวจสอบ workspace permissions
4. **Thai แสดงผลผิด**: ตั้งค่า console encoding

## 📊 สถิติโปรเจค

- **Modelfile**: 8,482+ บรรทัด C# examples
- **DevSharpAgent**: 12 ไฟล์ C# (~2,500 บรรทัด)
- **รองรับ Patterns**: 15+ Design Patterns
- **Technology Stack**: 50+ .NET libraries และ frameworks
- **Test Examples**: xUnit, NBomber, SpecFlow, k6

## 📄 License

MIT License - ดู [LICENSE](LICENSE) สำหรับรายละเอียด

## 🙏 Credits

สร้างโดย AI Assistant เพื่อช่วยเหลือชุมชนนักพัฒนา C#/.NET ในประเทศไทย

---

<div align="center">

**DevSharp** - Your Intelligent C#/.NET Development Companion  
พัฒนาด้วย ❤️ สำหรับชุมชน C#/.NET

[🚀 เริ่มต้นใช้งาน](#-การติดตั้งและใช้งาน) | [📖 คู่มือ](CLAUDE.md) | [🔧 DevSharpAgent](DevSharpAgent/README.md)

</div>