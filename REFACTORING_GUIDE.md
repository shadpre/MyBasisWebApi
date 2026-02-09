# MyBasisWebApi - Complete Refactoring Guide

## Overview
This guide will help you refactor the MyBasisWebApi project to fully comply with ScanitechDanmark coding standards.

## Critical Issues to Fix

### 1. ? Generic Repository Anti-Pattern
**Current:** `GenericRepository<T>` in BLL\Repos
**Issue:** Copilot instructions explicitly state "X No generic repositories"
**Action:** DELETE `GenericRepository.cs` and `IGenericRepository.cs`
**Replacement:** Use DbContext directly in services or create aggregate-specific repositories

### 2. ? Project Naming Convention
**Current Structure:**
```
- MyBasisWebApi (Presentation)
- BLL (Business Logic)
- DAL (Data Access)
- Domain (Domain Entities)
```

**Target Structure:**
```
- MyBasisWebApi_Presentation
- MyBasisWebApi_Logic  
- MyBasisWebApi_DataAccess
- MyBasisWebApi.Tests (optional)
```

### 3. ? CORS Security Issue
**Current:** `AllowAll` policy
**Fix:** Use explicit allowed origins from configuration

### 4. ? Missing/Incomplete Documentation
Many classes lack comprehensive XML documentation and inline WHY comments.

---

## Step-by-Step Refactoring Instructions

### Phase 1: Backup and Preparation
1. Commit all current changes to git
2. Create a new branch: `git checkout -b refactor/compliance`
3. Backup the current solution

### Phase 2: Project Renaming (Manual Steps Required)

#### A. Rename Projects Using Visual Studio
1. **Right-click** on `MyBasisWebApi` project ? Rename to `MyBasisWebApi_Presentation`
2. **Right-click** on `BLL` project ? Rename to `MyBasisWebApi_Logic`
3. **Right-click** on `DAL` project ? Rename to `MyBasisWebApi_DataAccess`
4. **Delete** the `Domain` project (we'll move entities to Logic layer)

#### B. Update Project References
After renaming, update all project references in `.csproj` files.

#### C. Rename Folders to Match Projects
1. Rename folder `MyBasisWebApi` to `MyBasisWebApi_Presentation`
2. Rename folder `BLL` to `MyBasisWebApi_Logic`
3. Rename folder `DAL` to `MyBasisWebApi_DataAccess`

### Phase 3: Move Domain Entities
Move all entities from Domain project to Logic layer:
- Move `Domain\Common\BaseEntity.cs` ? `MyBasisWebApi_Logic\Entities\Common\BaseEntity.cs`
- Move `Domain\Common\IEntity.cs` ? `MyBasisWebApi_Logic\Entities\Common\IEntity.cs`
- Move `DAL\ApiUser.cs` ? `MyBasisWebApi_Logic\Entities\Users\ApiUser.cs`

### Phase 4: Delete Generic Repository
1. Delete `BLL\Repos\GenericRepository.cs`
2. Delete `BLL\Interfaces\IGenericRepository.cs`
3. Update services to use DbContext directly or specific repositories

### Phase 5: Reorganize Folder Structure

**MyBasisWebApi_Logic Structure:**
```
MyBasisWebApi_Logic/
??? Entities/                          # Domain entities
?   ??? Common/
?   ?   ??? BaseEntity.cs
?   ?   ??? IEntity.cs
?   ??? Users/
?       ??? ApiUser.cs
??? Services/                          # Business services
?   ??? Authentication/
?       ??? AuthenticationService.cs
??? Handlers/                          # MediatR handlers
?   ??? Commands/
?   ?   ??? Register/
?   ?   ??? RefreshToken/
?   ?   ??? ...
?   ??? Queries/
?   ?   ??? Login/
?   ??? Notifications/
?   ??? Behaviors/
??? Validation/                        # FluentValidation validators
?   ??? Users/
??? Models/                           # DTOs
?   ??? Requests/
?   ??? Responses/
?   ??? Dtos/
??? Mapping/
?   ??? UserMappingProfile.cs
??? Interfaces/
?   ??? IAuthenticationService.cs
??? Exceptions/
?   ??? NotFoundException.cs
?   ??? BadRequestException.cs
??? Configuration/
    ??? JwtSettings.cs
    ??? CorsSettings.cs
```

**MyBasisWebApi_DataAccess Structure:**
```
MyBasisWebApi_DataAccess/
??? DbContext/
?   ??? MyDbContext.cs
??? Configurations/                   # EF Core entity configurations
?   ??? Users/
?   ?   ??? ApiUserConfiguration.cs
?   ??? Roles/
?       ??? RoleConfiguration.cs
??? Repositories/                     # Optional: Aggregate-specific repositories
?   ??? Users/
?       ??? IUserRepository.cs
?       ??? UserRepository.cs
??? Migrations/
??? Interfaces/
```

**MyBasisWebApi_Presentation Structure:**
```
MyBasisWebApi_Presentation/
??? Program.cs
??? Controllers/
?   ??? AccountController.cs
?   ??? RolesController.cs
??? Middleware/
?   ??? ExceptionMiddleware.cs
??? appsettings.json
??? appsettings.Development.json
??? appsettings.Production.json
```

### Phase 6: Update Namespaces
Update all namespaces to match new structure:
- `BLL.*` ? `MyBasisWebApi.Logic.*`
- `DAL.*` ? `MyBasisWebApi.DataAccess.*`
- `MyBasisWebApi.*` ? `MyBasisWebApi.Presentation.*`
- `Domain.*` ? `MyBasisWebApi.Logic.Entities.*`

### Phase 7: Fix Security Issues

#### Update CORS Configuration
In `appsettings.json`:
```json
{
  "AllowedOrigins": [
    "https://localhost:5001",
    "https://yourdomain.com"
  ]
}
```

In `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>())
              .AllowAnyMethod()
              .AllowAnyHeader());
});
```

### Phase 8: Add Comprehensive Documentation
- Add XML documentation to ALL public types and members
- Add inline comments explaining WHY, not WHAT
- Follow the documentation examples in copilot-instructions.md

### Phase 9: Apply Code Standards
1. Make all classes `sealed` by default (unless designed for inheritance)
2. Use `required` keyword for required properties
3. Use records for DTOs
4. Validate all constructor parameters with `ArgumentNullException.ThrowIfNull`
5. Use `IReadOnlyList<T>` for collection return types
6. All async methods must accept `CancellationToken`

### Phase 10: Testing
1. Build the solution: `dotnet build`
2. Run all tests: `dotnet test`
3. Fix any compilation errors
4. Test the API endpoints
5. Verify database migrations still work

---

## Quick Reference: Key Changes

### Remove These Files:
- ? `Domain\Class1.cs` (DELETED)
- ? `BLL\Repos\GenericRepository.cs`
- ? `BLL\Interfaces\IGenericRepository.cs`

### Update These Files:
- ? `Program.cs` - Fix CORS, improve documentation
- ? `BaseEntity.cs` - Add sealed, improve docs
- ? `ApiUser.cs` - Move to Logic layer, improve docs
- ? All handlers - Ensure comprehensive docs and inline comments
- ? All DTOs - Convert to sealed records where appropriate

### Create These Files:
- ? `.github/copilot-instructions.md` (the standards document)
- ? `README.md` with setup instructions
- ? Aggregate-specific repositories (if needed)

---

## Validation Checklist

After refactoring, verify:
- [ ] All projects follow naming convention: `ProjectName_Layer`
- [ ] All folders match namespace structure
- [ ] No generic repositories exist
- [ ] All public types have comprehensive XML documentation
- [ ] All non-trivial logic has inline WHY comments
- [ ] All classes are sealed unless designed for inheritance
- [ ] CORS uses explicit allowed origins
- [ ] All async methods accept CancellationToken
- [ ] All constructor parameters are validated
- [ ] Solution builds without errors
- [ ] All tests pass
- [ ] Git commit with clear message

---

## Need Help?
Refer to `.github/copilot-instructions.md` for detailed standards and examples.
