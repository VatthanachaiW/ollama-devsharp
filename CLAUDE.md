# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This is an Ollama model configuration repository for "DevSharp" - a specialized C#/.NET development assistant. The repository contains two main components:

1. **DevSharp Model Configuration** (~8,482 lines): Comprehensive Modelfile with extensive C# examples and patterns
2. **DevSharpAgent**: C# console application that provides file operation capabilities to Ollama models

### Repository Structure
```
devsharp/
├── Modelfile              # Main model definition (8,482 lines) with extensive C# examples
├── Modelfile.simple       # Simplified version of the model
├── DevSharpAgent/         # C# console app for file operations
│   ├── Models/           # Data models for API communication
│   ├── Services/         # Core services (File, Ollama, Agent)
│   ├── Program.cs        # Main entry point
│   ├── appsettings.json  # Configuration file
│   ├── DevSharpAgent.csproj  # Project file
│   ├── DevSharpAgent.sln     # Solution file
│   └── README.md         # Component documentation
├── CLAUDE.md             # This guidance file
└── .gitignore           # Git ignore rules for .NET projects
```

## Core Model Configuration

### DevSharp Model
- **Model Base**: llama3.1:8b
- **Temperature**: 0.12 (low for consistent, accurate code generation)
- **Context Window**: 8192 tokens
- **Language Support**: Bilingual (Thai/English)

### DevSharpAgent Configuration
- **Target Framework**: .NET 9.0
- **Ollama Base URL**: http://localhost:11434
- **Default Model**: devsharp
- **Workspace Root**: ./workspace (security isolation)

## Development Commands

### Model Management
```bash
# Create/update the DevSharp model (full version)
ollama create devsharp -f Modelfile

# Create/update the DevSharp model (simplified version)
ollama create devsharp-simple -f Modelfile.simple

# Run the model interactively
ollama run devsharp

# List available models
ollama list

# Show model information and parameters
ollama show devsharp

# Remove the model
ollama rm devsharp
```

### DevSharpAgent Development
```bash
# Build the application
cd DevSharpAgent
dotnet build

# Run the file agent (default workspace)
cd DevSharpAgent
dotnet run

# Run with specific workspace
cd DevSharpAgent
dotnet run -- --workspace "C:\MyProject"
dotnet run -- -w "./src"

# Show help
cd DevSharpAgent
dotnet run -- --help

# Run in development mode with hot reload
cd DevSharpAgent
dotnet watch run

# Clean build artifacts
cd DevSharpAgent
dotnet clean
```

### Testing C# Code Examples
```bash
# Create new class library
dotnet new classlib -n [ProjectName]

# Create xUnit test project
dotnet new xunit -n [ProjectName].Tests

# Add project reference
dotnet add [ProjectName].Tests reference [ProjectName]

# Run tests
dotnet test

# Run specific test categories
dotnet test --filter "Category=Performance"
dotnet test --filter "Category=LoadTest" 
dotnet test --filter "Category=BDD"
dotnet test --filter "Category=Security"
dotnet test --filter "Category=Integration"

# Kubernetes commands
kubectl apply -f k8s/
kubectl get pods -n product-api
kubectl logs -l app=product-api -n product-api

# Docker commands
docker build -t productapi:latest .
docker run -p 8080:8080 productapi:latest

# Entity Framework commands
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations remove
dotnet ef database drop

# Run application
dotnet run
```

### Package Management
Common packages used in examples:
```bash
# Core packages
dotnet add package Microsoft.AspNetCore.Mvc
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.InMemoryDatabase
dotnet add package MediatR
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions

# Modern .NET Features packages
dotnet add package Grpc.AspNetCore
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools
dotnet add package Microsoft.AspNetCore.SignalR
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# Testing packages
dotnet add package Moq
dotnet add package Microsoft.AspNetCore.Mvc.Testing

# Advanced Testing packages
dotnet add package NBomber
dotnet add package NBomber.Http
dotnet add package SpecFlow
dotnet add package SpecFlow.xUnit
dotnet add package SpecFlow.Tools.MsBuild.Generation
dotnet add package FluentAssertions

# Cloud Native packages
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add package Azure.Identity
dotnet add package Azure.Storage.Blobs
dotnet add package Azure.Messaging.ServiceBus
dotnet add package Microsoft.Extensions.Azure
dotnet add package AspNetCore.HealthChecks.AzureStorage
dotnet add package Microsoft.ApplicationInsights.AspNetCore

# Security packages
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Microsoft.AspNetCore.Authorization
dotnet add package Microsoft.AspNetCore.RateLimiting

# Performance packages
dotnet add package Microsoft.Extensions.Caching.Memory
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add package StackExchange.Redis
dotnet add package Microsoft.Extensions.ObjectPool
dotnet add package System.Buffers

# Advanced EF Core packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Analyzers
dotnet add package System.Text.Json
```

## Model Architecture & Expertise

The model is configured with expertise in:

### Design Patterns
- Repository, Factory, Strategy, Observer, Decorator
- SOLID principles implementation
- Dependency Injection patterns

### Architecture Patterns
- Clean Architecture
- Domain-Driven Design (DDD)
- Command Query Responsibility Segregation (CQRS)
- Event Sourcing
- Microservices architecture

### .NET Technologies
- ASP.NET Core
- Entity Framework Core
- MediatR for CQRS
- AutoMapper for object mapping
- FluentValidation for business rules

### Modern .NET Features
- **gRPC**: High-performance RPC communication with Protocol Buffers
- **SignalR**: Real-time web functionality for bidirectional communication
- **Minimal APIs**: Lightweight, performance-focused HTTP APIs
- **Blazor**: Web UI framework using C# instead of JavaScript
- **Worker Services**: Background services and hosted services
- **Health Checks**: Application health monitoring and diagnostics

### Advanced Testing
- **Performance Testing**: NBomber for load simulations, stress testing, and database performance validation
- **Load Testing**: k6 integration with C# test runners for comprehensive performance validation
- **BDD Testing**: SpecFlow with Gherkin scenarios for user story validation and acceptance testing
- **Integration Testing**: WebApplicationFactory for end-to-end API testing

### Cloud Native
- **Azure Services**: Blob Storage, Service Bus, Key Vault, Application Insights integration
- **AWS Services**: S3, SQS, Lambda, CloudWatch integration patterns
- **Kubernetes**: Pod deployment, service configuration, ingress, horizontal pod autoscaling
- **Docker**: Multi-stage builds, security best practices, container optimization

### Security
- **OAuth2/OpenID Connect**: Advanced token management, refresh token rotation, custom authorization policies
- **JWT Advanced Patterns**: Token revocation, claims-based authorization, security middleware
- **API Security**: Rate limiting, permission-based access control, resource owner validation
- **Authentication & Authorization**: Multi-level security policies, comprehensive security logging

### Performance
- **Memory Optimization**: Span<T>, Memory<T>, ArrayPool, StringBuilder pooling, stack allocation
- **Caching Strategies**: Multi-level caching (L1/L2), Redis integration, cache invalidation patterns
- **Database Performance**: Query optimization, bulk operations, pagination, connection pooling
- **Application Profiling**: Real-time performance monitoring, GC metrics tracking, operation timing

### Advanced EF Core
- **Complex Queries**: Split queries, compiled queries, complex joins, aggregations, temporal queries
- **Query Optimization**: Query splitting, projections, AsNoTracking, batch operations
- **Global Query Filters**: Soft deletes, multi-tenancy, row-level security
- **Owned Types**: Value objects, complex type mapping, table splitting
- **Value Converters**: Custom data type conversions, JSON columns, coordinate mapping
- **Advanced Configuration**: Temporal tables, computed columns, shadow properties, audit logging

## Response Format

The model follows a structured response format:
1. Short summary of the solution
2. Working code blocks with file names
3. Unit tests (xUnit framework)
4. Steps to run (dotnet CLI commands)
5. Notes on patterns used and complexity analysis

## File Organization

Examples in the Modelfile use this structure:
```
src/
  Controllers/
  Domain/
    Entities/
    Services/
  Infrastructure/
    Repositories/
tests/
  [ProjectName].Tests/
```

## Integrated Development Workflow

### Using DevSharp with DevSharpAgent
```bash
# 1. Start Ollama service (ensure running)
ollama serve

# 2. Create/update the DevSharp model
ollama create devsharp -f Modelfile

# 3. Run the DevSharpAgent for file operations
cd DevSharpAgent
dotnet run
```

### Testing the Complete System
```bash
# Test model directly
ollama run devsharp "สร้าง Hello World console app"

# Test through file agent (interactive mode)
cd DevSharpAgent
dotnet run
# Then use commands like: /help, /ls, @filename, etc.
```

### Model Configuration Details
- **Base Model**: llama3.1:8b (corrected from llama4)
- **Temperature**: 0.12 (optimized for consistent code generation)
- **Context Window**: 8192 tokens
- **Response Language**: Auto-detects Thai/English based on input

### Updating Models
```bash
# Update main model
ollama stop devsharp  # if running
ollama rm devsharp
ollama create devsharp -f Modelfile

# Update simple model
ollama rm devsharp-simple
ollama create devsharp-simple -f Modelfile.simple
```

## DevSharpAgent Architecture

The DevSharpAgent provides file operation capabilities to Ollama models through a structured service architecture:

### Core Services
- **FileService**: Handles file operations with workspace isolation and path traversal protection
- **OllamaService**: HTTP client for Ollama API communication with streaming response support  
- **DevSharpAgent**: Main orchestration service that combines file operations with AI responses

### Special Features
- **Interactive Chat**: Support for `@filename` syntax to include files in context
- **Special Commands**: `/help`, `/ls`, `/cat`, `/models` for direct system interaction
- **Auto File Detection**: Automatically detects and executes file operations from AI responses
- **Security**: All file operations restricted to workspace directory
- **JSON-based File Operations**: Structured format for AI to perform file operations

### File Operation Format
AI responses can include JSON blocks for file operations:
```json
{"operation": "WRITE_FILE", "path": "Program.cs", "content": "..."}
{"operation": "READ_FILE", "path": "Program.cs"}
{"operation": "DELETE_FILE", "path": "old-file.cs"}  
{"operation": "LIST_FILES", "path": "./src"}
{"operation": "EXECUTE", "command": "dotnet", "args": ["build"]}
```

The model includes extensive examples covering:

### Traditional Patterns
- Factorial functions and basic algorithms
- CRUD APIs with controllers and repositories
- Repository and Unit of Work patterns
- Strategy and Factory patterns for payment processing
- Clean Architecture with CQRS implementation
- Domain-Driven Design (DDD) with Aggregate Roots
- Entity Framework Core optimization techniques
- SOLID principles implementation
- Dependency Injection patterns

### Modern .NET Features Examples
- **gRPC Services**: Product management with Protocol Buffers, streaming, and proper error handling
- **SignalR Hubs**: Real-time chat application with authentication, groups, and message persistence
- **Minimal APIs**: Lightweight Product CRUD with validation, authentication, and OpenAPI documentation
- **Microservices Architecture**: Complete implementation with API Gateway, message bus, circuit breakers, and distributed tracing

### Advanced Testing Examples
- **Performance Testing (NBomber)**: API performance testing with load simulations, database performance validation, custom metrics collection, and detailed HTML/CSV reporting with percentile analysis
- **Load Testing (k6)**: JavaScript-based load testing with C# integration, multiple test scenarios (load/stress/spike), custom metrics, and HTML report generation
- **BDD Testing (SpecFlow)**: Gherkin feature files with comprehensive step definitions, scenario hooks, test data builders, and WebApplicationFactory integration for acceptance testing

### Cloud Native Examples
- **Azure Cloud Native Application**: Complete implementation with Azure Blob Storage, Service Bus, Key Vault integration, Kubernetes deployment with health checks, horizontal pod autoscaling, and Application Insights monitoring

### Security Examples
- **Advanced OAuth2/JWT System**: Comprehensive security implementation with custom token service, refresh token rotation, token revocation middleware, permission-based authorization handlers, resource owner policies, and rate limiting

### Performance Examples
- **Memory Optimization System**: High-performance data processing using Span<T>, Memory<T>, ArrayPool, StringBuilder pooling, multi-level caching (L1/L2), database optimization with bulk operations, and real-time performance monitoring with GC metrics tracking

### Advanced EF Core Examples
- **Advanced EF Core Implementation**: Complex queries with split query optimization, compiled queries for performance, global query filters for soft deletes and multi-tenancy, owned types for value objects, value converters for complex data types, temporal tables, automatic audit logging, and hierarchical data querying with comprehensive testing coverage