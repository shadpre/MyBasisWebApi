# ??? Project Structure - Before & After

## ?? Current Structure (Before Refactoring)

```
MyBasisWebApi/                                  ?? Needs renaming
??? MyBasisWebApi/                              ? Should be MyBasisWebApi_Presentation
?   ??? Controllers/
?   ?   ??? AccountController.cs
?   ?   ??? RolesController.cs
?   ??? Program.cs                              ?? Has security issues (CORS)
?   ??? appsettings.json                        ?? Missing AllowedOrigins config
?   ??? appsettings.Development.json
?
??? BLL/                                        ? Should be MyBasisWebApi_Logic
?   ??? Repos/
?   ?   ??? GenericRepository.cs                ? MUST DELETE (anti-pattern)
?   ?   ??? AuthManager.cs                      ? Fixed
?   ??? Interfaces/
?   ?   ??? IGenericRepository.cs               ? MUST DELETE (anti-pattern)
?   ?   ??? IAuthManager.cs
?   ??? Handlers/                               ? Good structure (CQRS)
?   ?   ??? Commands/
?   ?   ?   ??? Register/
?   ?   ?   ?   ??? RegisterCommand.cs
?   ?   ?   ?   ??? RegisterCommandHandler.cs   ? Fixed
?   ?   ?   ??? RefreshToken/
?   ?   ??? Queries/
?   ?       ??? Login/
?   ??? DTO/                                    ?? Should be Models/
?   ?   ??? Users/
?   ?       ??? ApiUserDto.cs
?   ?       ??? LoginDto.cs
?   ?       ??? AuthResponseDto.cs              ? Fixed (record syntax)
?   ??? Validation/                             ? Good
?   ?   ??? Users/
?   ??? Exceptions/                             ? Good
?   ??? Configuration/                          ? Good
?   ??? MapperConfig.cs
?   ??? Middleware/                             ?? Should be in Presentation
?       ??? ExceptionMiddleware.cs
?
??? DAL/                                        ? Should be MyBasisWebApi_DataAccess
?   ??? MyDbContext.cs                          ?? Should be in DbContext/ folder
?   ??? ApiUser.cs                              ?? Should be in Logic/Entities/Users/
?   ??? Configurations/                         ?? Typo: "Confirgurations"
?   ?   ??? RoleConfiguration.cs
?   ??? Migrations/                             ? Good
?
??? Domain/                                     ? Should be merged into Logic/Entities/
?   ??? Common/
?       ??? BaseEntity.cs                       ? Improved documentation
?       ??? IEntity.cs                          ? Improved documentation
?
??? MyBasisWebApi.Tests/                        ? Good naming
```

---

## ? Target Structure (After Refactoring)

```
MyBasisWebApi/                                  ? Solution folder
??? MyBasisWebApi_Presentation/                 ? Renamed
?   ??? Controllers/
?   ?   ??? V1/                                 ? API versioning
?   ?   ?   ??? AccountController.cs
?   ?   ?   ??? RolesController.cs
?   ?   ??? V2/                                 ? Future versions
?   ??? Middleware/                             ? Moved from Logic
?   ?   ??? ExceptionMiddleware.cs
?   ??? Program.cs                              ? Fixed CORS
?   ??? appsettings.json                        ? Has AllowedOrigins
?   ??? appsettings.Development.json            ? Local secrets
?   ??? appsettings.Production.json             ? Production config
?
??? MyBasisWebApi_Logic/                        ? Renamed from BLL
?   ??? Entities/                               ? Domain entities (from Domain + DAL)
?   ?   ??? Common/
?   ?   ?   ??? BaseEntity.cs                   ? From Domain
?   ?   ?   ??? IEntity.cs                      ? From Domain
?   ?   ??? Users/
?   ?       ??? ApiUser.cs                      ? Moved from DAL
?   ?
?   ??? Services/                               ? Business services
?   ?   ??? Authentication/
?   ?       ??? IAuthenticationService.cs       ? Replaces IAuthManager
?   ?       ??? AuthenticationService.cs        ? Replaces AuthManager
?   ?
?   ??? Handlers/                               ? MediatR CQRS handlers
?   ?   ??? Commands/
?   ?   ?   ??? Register/
?   ?   ?   ?   ??? RegisterCommand.cs
?   ?   ?   ?   ??? RegisterHandler.cs
?   ?   ?   ?   ??? RegisterResult.cs           ? Explicit result type
?   ?   ?   ??? RefreshToken/
?   ?   ?   ?   ??? RefreshTokenCommand.cs
?   ?   ?   ?   ??? RefreshTokenHandler.cs
?   ?   ?   ??? ...
?   ?   ?
?   ?   ??? Queries/
?   ?   ?   ??? Login/
?   ?   ?       ??? LoginQuery.cs
?   ?   ?       ??? LoginHandler.cs
?   ?   ?       ??? LoginResult.cs              ? Explicit result type
?   ?   ?
?   ?   ??? Notifications/                      ? Domain events
?   ?   ?   ??? UserRegistered/
?   ?   ?       ??? UserRegisteredNotification.cs
?   ?   ?       ??? SendWelcomeEmailHandler.cs
?   ?   ?
?   ?   ??? Behaviors/                          ? Pipeline behaviors
?   ?       ??? ValidationBehavior.cs
?   ?       ??? LoggingBehavior.cs
?   ?       ??? TransactionBehavior.cs
?   ?
?   ??? Validation/                             ? FluentValidation validators
?   ?   ??? Users/
?   ?       ??? LoginDtoValidator.cs
?   ?       ??? ApiUserDtoValidator.cs
?   ?
?   ??? Models/                                 ? Renamed from DTO
?   ?   ??? Requests/                           ? Request DTOs
?   ?   ?   ??? LoginRequest.cs
?   ?   ?   ??? RegisterRequest.cs
?   ?   ??? Responses/                          ? Response DTOs
?   ?   ?   ??? AuthResponse.cs
?   ?   ?   ??? UserResponse.cs
?   ?   ??? Dtos/                               ? Internal DTOs
?   ?       ??? PagedResult.cs
?   ?
?   ??? Mapping/                                ? AutoMapper profiles
?   ?   ??? UserMappingProfile.cs
?   ?
?   ??? Interfaces/                             ? Service contracts
?   ?   ??? IAuthenticationService.cs
?   ?
?   ??? Exceptions/                             ? Domain exceptions
?   ?   ??? NotFoundException.cs
?   ?   ??? BadRequestException.cs
?   ?   ??? ValidationException.cs
?   ?
?   ??? Configuration/                          ? Strongly-typed config
?       ??? JwtSettings.cs
?       ??? CorsSettings.cs
?
??? MyBasisWebApi_DataAccess/                   ? Renamed from DAL
?   ??? DbContext/                              ? EF Core context
?   ?   ??? MyDbContext.cs
?   ?
?   ??? Configurations/                         ? EF Core entity configs
?   ?   ??? Users/
?   ?   ?   ??? ApiUserConfiguration.cs         ? IEntityTypeConfiguration
?   ?   ??? Roles/
?   ?       ??? RoleConfiguration.cs
?   ?
?   ??? Repositories/                           ? Optional: Aggregate-specific only
?   ?   ??? Users/
?   ?       ??? IUserRepository.cs              ? Domain operations
?   ?       ??? UserRepository.cs               ? EF Core implementation
?   ?
?   ??? Migrations/                             ? EF Core migrations
?   ?   ??? 20250205131712_init.cs
?   ?   ??? 20250205132318_init1.cs
?   ?   ??? ...
?   ?
?   ??? Interfaces/                             ? Data access contracts
?       ??? IUserRepository.cs
?
??? MyBasisWebApi.Tests/                        ? Unit & integration tests
    ??? Unit/
    ?   ??? Handlers/
    ?   ?   ??? RegisterHandlerTests.cs
    ?   ?   ??? LoginHandlerTests.cs
    ?   ??? Services/
    ?       ??? AuthenticationServiceTests.cs
    ??? Integration/
        ??? AccountControllerTests.cs
        ??? TestWebApplicationFactory.cs

```

---

## ?? Migration Path

### Phase 1: Security Fixes (30 min)
```
NO file moves, just content updates:
- appsettings.json: Add AllowedOrigins
- Program.cs: Update CORS config
```

### Phase 2: Delete Anti-Patterns (2-3 hours)
```
DELETE:
- BLL/Repos/GenericRepository.cs
- BLL/Interfaces/IGenericRepository.cs

UPDATE:
- All services using IGenericRepository
- Program.cs DI registration
```

### Phase 3: Rename Projects (1-2 hours)
```
RENAME (in Visual Studio):
- MyBasisWebApi ? MyBasisWebApi_Presentation
- BLL ? MyBasisWebApi_Logic
- DAL ? MyBasisWebApi_DataAccess

UPDATE:
- All namespaces
- All using statements
- All .csproj references
```

### Phase 4: Reorganize Files (2-3 hours)
```
MOVE:
Domain/Common/* ? MyBasisWebApi_Logic/Entities/Common/
DAL/ApiUser.cs ? MyBasisWebApi_Logic/Entities/Users/
DAL/MyDbContext.cs ? MyBasisWebApi_DataAccess/DbContext/
BLL/Middleware/* ? MyBasisWebApi_Presentation/Middleware/
BLL/DTO/ ? MyBasisWebApi_Logic/Models/

CREATE:
MyBasisWebApi_Logic/Services/
MyBasisWebApi_DataAccess/Configurations/Users/

RENAME:
DAL/Confirgurations/ ? Configurations/
```

---

## ?? Namespace Changes

### Before
```csharp
using BLL;
using BLL.Repos;
using BLL.DTO.Users;
using BLL.Handlers.Commands.Register;
using DAL;
using Domain.Common;
```

### After
```csharp
using MyBasisWebApi.Logic;
using MyBasisWebApi.Logic.Services.Authentication;
using MyBasisWebApi.Logic.Models.Responses;
using MyBasisWebApi.Logic.Handlers.Commands.Register;
using MyBasisWebApi.DataAccess;
using MyBasisWebApi.DataAccess.DbContext;
using MyBasisWebApi.Logic.Entities.Common;
using MyBasisWebApi.Logic.Entities.Users;
```

---

## ?? Benefits of New Structure

### Clear Layer Separation
- ? Presentation layer handles HTTP concerns only
- ? Logic layer contains all business logic
- ? DataAccess layer handles persistence only

### Better Discoverability
- ? New developers understand structure immediately
- ? Files are grouped by feature and responsibility
- ? Naming follows consistent conventions

### Follows Standards
- ? Matches ScanitechDanmark coding standards
- ? No anti-patterns (generic repository)
- ? Proper entity encapsulation
- ? Clear aggregate boundaries

### Easier Testing
- ? Mock-friendly service interfaces
- ? Clear dependency direction
- ? Testable without infrastructure

### Better Maintainability
- ? Single Responsibility Principle
- ? Clear ownership of code
- ? Easy to find and modify code

---

## ?? File Count Summary

### Current
- Total Projects: 4
- Total Files: ~50
- Need Relocation: ~8 files
- Need Deletion: 2 files
- Need Renaming: 3 projects

### After Refactoring
- Total Projects: 3 (Domain merged into Logic)
- Total Files: ~50 (same, just reorganized)
- New Folders: ~8
- Clearer Structure: 100%

---

## ? Quick Validation

After completing refactoring, verify:

### Project Names
```bash
# Should see these in Solution Explorer:
? MyBasisWebApi_Presentation
? MyBasisWebApi_Logic
? MyBasisWebApi_DataAccess
? MyBasisWebApi.Tests

? MyBasisWebApi (old name)
? BLL (old name)
? DAL (old name)
? Domain (merged)
```

### Folder Structure
```bash
# Logic layer entities:
? MyBasisWebApi_Logic/Entities/Common/BaseEntity.cs
? MyBasisWebApi_Logic/Entities/Users/ApiUser.cs

# DataAccess DbContext:
? MyBasisWebApi_DataAccess/DbContext/MyDbContext.cs

# Middleware in Presentation:
? MyBasisWebApi_Presentation/Middleware/ExceptionMiddleware.cs

# NO generic repository:
? GenericRepository.cs (should not exist!)
? IGenericRepository.cs (should not exist!)
```

### Build Success
```bash
dotnet build
# Should complete without errors
```

---

**Use this guide as a visual reference while following CHECKLIST.md!**

Good luck! ??
