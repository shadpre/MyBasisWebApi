# MyBasisWebApi - Refactoring Plan to ScanitechDanmark Standards

## ?? Executive Summary
This document outlines the comprehensive refactoring plan to bring MyBasisWebApi into full compliance with ScanitechDanmark coding standards.

## ?? Current State Analysis

### Current Structure:
```
MyBasisWebApi/
??? MyBasisWebApi/          # Presentation Layer
??? BLL/                    # Business Logic Layer  
??? DAL/                    # Data Access Layer
??? Domain/                 # Domain Models
??? Tests/                  # Test Projects
```

### Target Structure (ScanitechDanmark Standard):
```
MyBasisWebApi/
??? .github/
?   ??? copilot-instructions.md    # ? Already exists
??? MyBasisWebApi_Presentation/    # Renamed from MyBasisWebApi
?   ??? Program.cs
?   ??? Controllers/
?   ??? appsettings.json
?   ??? Properties/
??? MyBasisWebApi_Logic/            # Renamed from BLL
?   ??? Services/
?   ??? Handlers/                   # NEW - MediatR handlers
?   ?   ??? Commands/
?   ?   ??? Queries/
?   ?   ??? Notifications/
?   ??? Validation/                 # NEW - FluentValidation
?   ??? Models/
?   ?   ??? Requests/
?   ?   ??? Responses/
?   ?   ??? Dtos/
?   ??? Mapping/
?   ??? Interfaces/
?   ??? Exceptions/
??? MyBasisWebApi_DataAccess/      # Renamed from DAL
?   ??? DbContext/
?   ??? Entities/
?   ??? Configurations/
?   ??? Migrations/
?   ??? Repositories/              # NEW - Specific repositories only
?   ??? Interfaces/
??? MyBasisWebApi.Tests/            # Test project
```

## ?? Refactoring Tasks

### Phase 1: Project Restructuring
- [ ] Rename `MyBasisWebApi` ? `MyBasisWebApi_Presentation`
- [ ] Rename `BLL` ? `MyBasisWebApi_Logic`
- [ ] Rename `DAL` ? `MyBasisWebApi_DataAccess`
- [ ] Update all project references
- [ ] Update solution file
- [ ] Remove `Domain` project (merge into DataAccess/Entities)

### Phase 2: Remove Anti-Patterns
- [ ] **Delete `GenericRepository<T>`** - Anti-pattern per standards
- [ ] Remove `IGenericRepository<T>` interface
- [ ] Create specific repositories only where needed (e.g., `IUserRepository`)
- [ ] Update all usages to use DbContext directly or specific repos

### Phase 3: Add MediatR (CQRS Pattern)
- [ ] Install `MediatR` NuGet package
- [ ] Install `MediatR.Extensions.Microsoft.DependencyInjection`
- [ ] Create folder structure for Commands/Queries/Notifications
- [ ] Migrate controller logic to MediatR handlers:
  - [ ] RegisterCommand + RegisterCommandHandler
  - [ ] LoginQuery + LoginQueryHandler  
  - [ ] RefreshTokenCommand + RefreshTokenCommandHandler
- [ ] Update controllers to use IMediator

### Phase 4: Add FluentValidation
- [ ] Install `FluentValidation.AspNetCore` NuGet package
- [ ] Create validators:
  - [ ] ApiUserDtoValidator
  - [ ] LoginDtoValidator
  - [ ] RefreshTokenDtoValidator
- [ ] Add MediatR pipeline behavior for validation
- [ ] Remove manual ModelState validation from controllers

### Phase 5: Security Improvements
- [ ] Change CORS from `AllowAll` to specific origins
- [ ] Add strongly-typed JwtSettings with IOptions
- [ ] Review and update JWT configuration
- [ ] Implement password protection service (DPAPI or BCrypt based on use case)

### Phase 6: Code Quality Improvements
- [ ] Add comprehensive XML documentation to ALL public members
- [ ] Add inline comments explaining WHY (business rules, design decisions)
- [ ] Convert DTOs to `sealed record` types
- [ ] Mark all classes as `sealed` by default
- [ ] Change return types from `List<T>` to `IReadOnlyList<T>`
- [ ] Add `CancellationToken` parameters to all async methods
- [ ] Enable nullable reference types if not already enabled
- [ ] Use `ArgumentNullException.ThrowIfNull` for parameter validation

### Phase 7: Logging Improvements
- [ ] Review Serilog configuration
- [ ] Add structured logging with proper context
- [ ] Replace string interpolation with structured log properties
- [ ] Add correlation IDs using `BeginScope`

### Phase 8: Configuration Improvements
- [ ] Create strongly-typed configuration classes:
  - [ ] DatabaseSettings
  - [ ] JwtSettings  
  - [ ] CorsSettings
- [ ] Use IOptions pattern
- [ ] Move secrets to user secrets / environment variables

### Phase 9: Program.cs Refactoring
- [ ] Reorganize middleware pipeline order
- [ ] Add comprehensive comments explaining WHY for each configuration
- [ ] Group registrations logically
- [ ] Add startup validation
- [ ] Remove development-only code from production

### Phase 10: Testing
- [ ] Ensure all unit tests pass
- [ ] Add integration tests using WebApplicationFactory
- [ ] Test MediatR handlers independently
- [ ] Test validation rules

## ?? Critical Standards Compliance Checklist

### Architecture
- [x] Three-layer architecture (Presentation/Logic/DataAccess)
- [ ] One class per file
- [ ] Async-first design (all I/O operations async)
- [ ] Comprehensive XML documentation on public types/members
- [ ] Inline comments explaining WHY

### Technology Stack
- [x] Entity Framework Core (self-developed DB)
- [ ] MediatR for CQRS
- [ ] FluentValidation for validation
- [x] Serilog for logging
- [x] AutoMapper for mapping
- [ ] No Generic Repositories

### Code Quality
- [ ] Sealed classes by default
- [ ] Records for DTOs
- [ ] IReadOnlyList<T> for collections
- [ ] CancellationToken on all async methods
- [ ] Nullable reference types enabled
- [ ] Meaningful names (no abbreviations)

### Security
- [ ] No AllowAnyOrigin in CORS
- [ ] JWT properly configured
- [ ] Secrets in environment variables
- [ ] Password protection (DPAPI/BCrypt)

### Best Practices
- [ ] No business logic in controllers
- [ ] No DbContext usage outside repositories/services
- [ ] Global exception handling middleware
- [ ] Structured logging
- [ ] Strongly-typed configuration

## ?? Execution Strategy

### Approach: **Incremental Refactoring**
We'll refactor incrementally to maintain working code at each step:

1. **Setup Phase** - Install packages, create folder structures
2. **Migration Phase** - Move code to new structure while keeping old code
3. **Implementation Phase** - Implement new patterns (MediatR, FluentValidation)
4. **Cleanup Phase** - Remove old code, anti-patterns
5. **Polish Phase** - Documentation, comments, final touches
6. **Testing Phase** - Comprehensive testing

### Order of Operations:
1. Install all required NuGet packages
2. Create new folder structures
3. Rename projects (breaking change - do early)
4. Add MediatR infrastructure
5. Migrate one controller at a time to MediatR
6. Add FluentValidation
7. Remove GenericRepository
8. Add comprehensive documentation
9. Security improvements
10. Final cleanup and testing

## ?? Breaking Changes
- Project names will change (requires update of references)
- Generic repository will be removed (requires refactoring all usages)
- Controller signatures will change (MediatR integration)
- Some DTOs will become records (could affect serialization)

## ?? Benefits After Refactoring
- ? Full compliance with ScanitechDanmark standards
- ? Better separation of concerns (CQRS with MediatR)
- ? Improved testability
- ? Better validation (FluentValidation)
- ? Enhanced security
- ? Better maintainability
- ? Comprehensive documentation
- ? Modern C# patterns and features

## ?? Success Criteria
- [ ] All code passes build without errors
- [ ] All tests pass
- [ ] No compiler warnings
- [ ] All public APIs have XML documentation
- [ ] All non-trivial logic has inline comments
- [ ] CORS configured for specific origins
- [ ] No generic repositories in codebase
- [ ] MediatR handling all controller operations
- [ ] FluentValidation for all input validation
- [ ] Structured logging throughout

---

**Document Version:** 1.0  
**Created:** 2025-02-06  
**Author:** GitHub Copilot  
**Status:** Ready for Implementation
