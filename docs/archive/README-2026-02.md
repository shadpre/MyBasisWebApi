# My Basis Web API

A production-ready RESTful Web API built with .NET 9, implementing authentication, authorization, and best practices according to ScanitechDanmark coding standards.

## ?? Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Documentation](#api-documentation)
- [Security](#security)
- [Development](#development)
- [Testing](#testing)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

## ?? Overview

This Web API provides user authentication and authorization using JWT tokens. It follows a three-layer architecture (Presentation, Logic, DataAccess) and implements the CQRS pattern using MediatR.

### Key Technologies

- **.NET 9.0** - Latest stable .NET framework
- **ASP.NET Core Identity** - User management and authentication
- **JWT Bearer Authentication** - Stateless token-based security
- **Entity Framework Core** - ORM for database access (we own the schema)
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Request validation
- **AutoMapper** - DTO to entity mapping
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation and testing

## ??? Architecture

The project follows **three-layer architecture** as per ScanitechDanmark standards:

```
MyBasisWebApi/
??? MyBasisWebApi_Presentation/    # Entry Point (API Controllers, Startup)
??? MyBasisWebApi_Logic/           # Business Logic (Services, Handlers, DTOs)
??? MyBasisWebApi_DataAccess/      # Data Access (DbContext, Entities, Migrations)
```

### Layer Responsibilities

**Presentation Layer** (`MyBasisWebApi_Presentation`)
- API Controllers (thin, delegate to MediatR)
- Program.cs (dependency injection, middleware pipeline)
- Configuration files (appsettings.json)

**Logic Layer** (`MyBasisWebApi_Logic`)
- MediatR Handlers (Commands, Queries)
- Business logic and orchestration
- DTOs and mapping profiles
- FluentValidation validators
- Interfaces and service contracts

**DataAccess Layer** (`MyBasisWebApi_DataAccess`)
- Entity Framework DbContext
- Domain entities
- Entity configurations
- Database migrations

## ? Features

### Authentication & Authorization
- ? User registration with password hashing
- ? User login with JWT token generation
- ? JWT refresh token support
- ? Role-based authorization
- ? ASP.NET Core Identity integration

### API Features
- ? RESTful API design
- ? Swagger/OpenAPI documentation
- ? Global exception handling
- ? Request validation with FluentValidation
- ? Response caching
- ? Rate limiting (IP-based)
- ? CORS configuration
- ? OData query support ($select, $filter, $orderby)

### Development Features
- ? Comprehensive XML documentation
- ? Structured logging with Serilog
- ? Nullable reference types
- ? Async/await throughout
- ? CQRS pattern with MediatR

## ?? Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server LocalDB](https://docs.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) or SQL Server
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or VS Code
- [Postman](https://www.postman.com/) or similar tool for API testing (optional)

## ?? Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/shadpre/MyBasisWebApi.git
cd MyBasisWebApi
```

### 2. Update Connection String

Edit `MyBasisWebApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MyDbConnectionString": "Server=(localdb)\\mssqllocaldb;Database=MyAPIDB;Trusted_Connection=True;MultipleActiveResultSets=True"
  }
}
```

### 3. Apply Database Migrations

```bash
cd MyBasisWebApi
dotnet ef database update --project ../DAL/MyBasisWebApi_DataAccess.csproj
```

This creates the database with:
- ASP.NET Core Identity tables (Users, Roles, etc.)
- Default roles (Admin, User)

### 4. Run the Application

```bash
dotnet run --project MyBasisWebApi/MyBasisWebApi_Presentation.csproj
```

Or press **F5** in Visual Studio.

### 5. Access Swagger UI

Open your browser and navigate to:
```
https://localhost:7000/swagger
```

## ?? Configuration

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "MyDbConnectionString": "your-connection-string"
  },
  "AllowedOrigins": [
    "https://localhost:5001",
    "https://yourdomain.com"
  ],
  "JwtSettings": {
    "Issuer": "MyAPI",
    "Audience": "MyAPIClient",
    "ExpiryMinutes": 10,
    "Key": "your-secret-key-minimum-32-characters"
  },
  "IpRateLimitOptions": {
    "EnableEndpointRateLimiting": true,
    "GeneralRules": [
      {
        "Endpoint": "*:/*",
        "Period": "5s",
        "Limit": 100
      }
    ]
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "./logs/log-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

### Environment-Specific Configuration

Create `appsettings.Development.json` or `appsettings.Production.json` for environment-specific overrides:

```json
{
  "JwtSettings": {
    "Key": "different-key-for-production"
  },
  "AllowedOrigins": [
    "https://production-domain.com"
  ]
}
```

### User Secrets (Recommended for Development)

Store sensitive data in user secrets:

```bash
dotnet user-secrets init --project MyBasisWebApi/MyBasisWebApi_Presentation.csproj
dotnet user-secrets set "JwtSettings:Key" "your-secret-key" --project MyBasisWebApi/MyBasisWebApi_Presentation.csproj
```

## ?? API Documentation

### Swagger UI

Interactive API documentation available at: `https://localhost:7000/swagger`

### Authentication Flow

#### 1. Register a New User

**POST** `/api/account/register`

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** `200 OK` (empty body on success)

#### 2. Login

**POST** `/api/account/login`

```json
{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** `200 OK`
```json
{
  "userId": "user-guid",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-string"
}
```

#### 3. Use Token in Requests

Include the token in the Authorization header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### 4. Refresh Token

**POST** `/api/account/refreshtoken`

```json
{
  "userId": "user-guid",
  "token": "old-access-token",
  "refreshToken": "current-refresh-token"
}
```

**Response:** New tokens

## ?? Security

### JWT Token Security

- **Signing Algorithm:** HMAC-SHA256
- **Token Expiration:** Configurable (default: 10 minutes)
- **Refresh Token:** Long-lived, stored securely
- **Secret Key:** Minimum 32 characters, stored in configuration

### Password Security

- ASP.NET Core Identity handles password hashing
- Uses **PBKDF2** with salt by default
- Passwords are never logged or exposed

### CORS Configuration

CORS is configured to only allow specific origins:

```csharp
// In appsettings.json
"AllowedOrigins": [
  "https://localhost:5001",
  "https://yourdomain.com"
]
```

?? **Never use `AllowAnyOrigin()` in production!**

### Rate Limiting

IP-based rate limiting protects against API abuse:
- Default: 100 requests per 5 seconds per IP
- Returns `429 Too Many Requests` when limit exceeded

## ?? Development

### Project Structure

```
MyBasisWebApi_Logic/
??? Configuration/          # Strongly-typed settings
??? DTO/                    # Data Transfer Objects
??? Exceptions/             # Domain exceptions
??? Handlers/
?   ??? Commands/          # State-changing operations
?   ?   ??? Register/
?   ?   ??? RefreshToken/
?   ??? Queries/           # Read-only operations
?   ?   ??? Login/
?   ??? Behaviors/         # MediatR pipeline behaviors
??? Interfaces/            # Service contracts
??? Middleware/            # Custom middleware
??? Services/              # Service implementations
??? Validation/            # FluentValidation validators
```

### Adding a New Feature

1. **Create Command/Query** in `Handlers/Commands/` or `Handlers/Queries/`
2. **Create Handler** implementing `IRequestHandler<TRequest, TResponse>`
3. **Create Validator** using FluentValidation in `Validation/`
4. **Add Controller Action** that sends request via MediatR
5. **Update Swagger** annotations if needed

### Code Standards

This project follows **ScanitechDanmark Coding Standards**. Key requirements:

? **Comprehensive XML documentation** on all public types and members  
? **Inline comments** explaining WHY, not WHAT  
? **Sealed classes** by default  
? **Async/await** throughout  
? **CancellationToken** on all async methods  
? **Nullable reference types** enabled  
? **Fail fast** - validate dependencies in constructors  

### Database Migrations

Create a new migration:

```bash
dotnet ef migrations add MigrationName --project DAL/MyBasisWebApi_DataAccess.csproj --startup-project MyBasisWebApi/MyBasisWebApi_Presentation.csproj
```

Apply migrations:

```bash
dotnet ef database update --project DAL/MyBasisWebApi_DataAccess.csproj --startup-project MyBasisWebApi/MyBasisWebApi_Presentation.csproj
```

Generate SQL script for production:

```bash
dotnet ef migrations script --idempotent --output migration.sql --project DAL/MyBasisWebApi_DataAccess.csproj --startup-project MyBasisWebApi/MyBasisWebApi_Presentation.csproj
```

## ?? Testing

### Manual Testing with Swagger

1. Start the application
2. Navigate to `https://localhost:7000/swagger`
3. Click "Authorize" and enter your JWT token
4. Test API endpoints interactively

### Testing with Postman

1. Import the API into Postman
2. Create environment variables:
   - `baseUrl`: `https://localhost:7000`
   - `token`: (set after login)
3. Use `{{baseUrl}}` and `{{token}}` in requests

### Unit Testing (Optional)

Add xUnit test project:

```bash
dotnet new xunit -n MyBasisWebApi.Tests
dotnet add MyBasisWebApi.Tests/MyBasisWebApi.Tests.csproj reference BLL/MyBasisWebApi_Logic.csproj
```

## ?? Deployment

### Configuration Checklist

Before deploying to production:

- [ ] Update `JwtSettings:Key` with a strong secret (32+ characters)
- [ ] Configure `AllowedOrigins` for your domain
- [ ] Update connection string for production database
- [ ] Set `Serilog` to appropriate log level (Warning or Error)
- [ ] Configure rate limiting based on expected traffic
- [ ] Review and adjust `JwtSettings:ExpiryMinutes` (balance security vs UX)
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`

### Deployment Options

**Azure App Service:**
```bash
az webapp up --name your-app-name --resource-group your-rg --runtime "DOTNET|9.0"
```

**Docker:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyBasisWebApi_Presentation.dll"]
```

## ?? Troubleshooting

### Database Connection Issues

**Problem:** Cannot connect to database

**Solutions:**
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database exists: `dotnet ef database update`
- Check firewall rules

### JWT Token Issues

**Problem:** 401 Unauthorized errors

**Solutions:**
- Verify token is included in Authorization header: `Bearer {token}`
- Check token hasn't expired (default 10 minutes)
- Ensure `JwtSettings` match between token generation and validation
- Refresh token if expired

### Migration Issues

**Problem:** `dotnet ef` command not found

**Solution:**
```bash
dotnet tool install --global dotnet-ef
```

**Problem:** Migration fails with database locked

**Solution:**
- Stop the application
- Close all database connections
- Retry migration

### Logging

Check logs in `./logs/log-YYYYMMDD.txt` for detailed error information.

Enable verbose logging temporarily:

```json
// appsettings.Development.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

## ?? Support

For issues or questions:

1. Check existing GitHub issues
2. Review the [ScanitechDanmark Coding Standards](.github/copilot-instructions.md)
3. Contact the development team

## ?? License

[Your License Here]

## ?? Acknowledgments

Built following **ScanitechDanmark Coding Standards** for enterprise .NET applications.

---

**Version:** 1.0.0  
**Last Updated:** 2026-02-06  
**Maintained By:** ScanitechDanmark Development Team
