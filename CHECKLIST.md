# ?? Refactoring Checklist - Quick Reference

Use this checklist to track your progress through the refactoring process.

## ?? CRITICAL - Do These First

### Security Fixes
- [ ] **CORS Configuration**
  - [ ] Add `AllowedOrigins` array to `appsettings.json`
  - [ ] Update `Program.cs` to use `WithOrigins()` instead of `AllowAnyOrigin()`
  - [ ] Test with Swagger UI
  - [ ] Commit changes

- [ ] **JWT Secret Key**
  - [ ] Generate secure random key (32+ characters)
  - [ ] Update `appsettings.Development.json` (don't commit!)
  - [ ] Document prod deployment process (env vars/Key Vault)
  - [ ] Add sensitive files to `.gitignore`

---

## ??? HIGH PRIORITY - Remove Anti-Patterns

### Delete Generic Repository
- [ ] Find all usages of `IGenericRepository<T>`
- [ ] Document where it's used (controllers, services, etc.)
- [ ] Replace with DbContext direct access or specific repositories
- [ ] Update DI registration in `Program.cs` (remove generic repository line)
- [ ] Delete `BLL\Repos\GenericRepository.cs`
- [ ] Delete `BLL\Interfaces\IGenericRepository.cs`
- [ ] Build solution - fix any errors
- [ ] Run tests
- [ ] Commit changes

---

## ?? MEDIUM PRIORITY - Project Structure

### Rename Projects (Visual Studio)
- [ ] Rename `MyBasisWebApi` ? `MyBasisWebApi_Presentation`
- [ ] Rename `BLL` ? `MyBasisWebApi_Logic`
- [ ] Rename `DAL` ? `MyBasisWebApi_DataAccess`
- [ ] Delete or integrate `Domain` project

### Rename Folders (File Explorer)
- [ ] Rename `MyBasisWebApi` folder ? `MyBasisWebApi_Presentation`
- [ ] Rename `BLL` folder ? `MyBasisWebApi_Logic`
- [ ] Rename `DAL` folder ? `MyBasisWebApi_DataAccess`
- [ ] Delete `Domain` folder (after moving entities)

### Update Namespaces (Find & Replace All)
- [ ] `namespace BLL` ? `namespace MyBasisWebApi.Logic`
- [ ] `namespace DAL` ? `namespace MyBasisWebApi.DataAccess`
- [ ] `namespace Domain` ? `namespace MyBasisWebApi.Logic.Entities`
- [ ] `using BLL` ? `using MyBasisWebApi.Logic`
- [ ] `using DAL` ? `using MyBasisWebApi.DataAccess`
- [ ] `using Domain` ? `using MyBasisWebApi.Logic.Entities`

### Move Entity Files
- [ ] Move `Domain\Common\BaseEntity.cs` ? `MyBasisWebApi_Logic\Entities\Common\BaseEntity.cs`
- [ ] Move `Domain\Common\IEntity.cs` ? `MyBasisWebApi_Logic\Entities\Common\IEntity.cs`
- [ ] Move `DAL\ApiUser.cs` ? `MyBasisWebApi_Logic\Entities\Users\ApiUser.cs`
- [ ] Update namespaces in moved files
- [ ] Update using statements in files that reference them

### Move Other Files
- [ ] Move `BLL\Middleware\ExceptionMiddleware.cs` ? `MyBasisWebApi_Presentation\Middleware\ExceptionMiddleware.cs`
- [ ] Update namespace and references

### Reorganize Folders (Within Projects)
- [ ] Create `MyBasisWebApi_Logic\Entities\` folder structure
- [ ] Create `MyBasisWebApi_Logic\Services\` folder structure
- [ ] Create `MyBasisWebApi_DataAccess\DbContext\` folder
- [ ] Create `MyBasisWebApi_DataAccess\Configurations\` folder structure
- [ ] Move files to match standard structure (see REFACTORING_GUIDE.md)

---

## ?? ONGOING - Code Quality

### Documentation
- [ ] Add XML docs to `BLL\Handlers\Queries\Login\LoginQueryHandler.cs`
- [ ] Add XML docs to `BLL\Handlers\Commands\RefreshToken\RefreshTokenCommandHandler.cs`
- [ ] Add XML docs to `BLL\Repos\AuthManager.cs`
- [ ] Add XML docs to all DTO files in `BLL\DTO\`
- [ ] Add XML docs to all validator files in `BLL\Validation\`
- [ ] Add XML docs to exception files in `BLL\Exceptions\`
- [ ] Add XML docs to `DAL\MyDbContext.cs`
- [ ] Add XML docs to all controllers in `MyBasisWebApi\Controllers\`
- [ ] Add inline WHY comments to complex business logic

### Code Standards
- [ ] Make all classes `sealed` unless designed for inheritance
- [ ] Use `required` keyword for non-nullable properties
- [ ] Convert appropriate DTOs to `sealed record`
- [ ] Add `ArgumentNullException.ThrowIfNull` to all constructors
- [ ] Use `IReadOnlyList<T>` for collection returns
- [ ] Ensure all async methods accept `CancellationToken`
- [ ] Use private setters for entity properties

---

## ?? VALIDATION - Test Everything

### Build & Compile
- [ ] `dotnet clean`
- [ ] `dotnet build`
- [ ] No warnings or errors
- [ ] All projects reference correct namespaces

### Database
- [ ] Migrations still work: `dotnet ef database update`
- [ ] Can create new migration: `dotnet ef migrations add TestMigration`
- [ ] Remove test migration: `dotnet ef migrations remove`

### Runtime Testing
- [ ] Application starts without errors
- [ ] Swagger UI loads at `/swagger`
- [ ] Can register new user
- [ ] Can login with user credentials
- [ ] JWT token is generated correctly
- [ ] Protected endpoints require authentication
- [ ] Roles authorization works
- [ ] CORS works with configured origins
- [ ] Rate limiting is functioning

### Unit Tests (if they exist)
- [ ] `dotnet test`
- [ ] All tests pass
- [ ] No test failures due to refactoring

---

## ?? DOCUMENTATION - Keep It Current

### Create/Update Files
- [ ] Create `.github\copilot-instructions.md` (the full standards document)
- [ ] Update `README.md` with current project structure
- [ ] Update API documentation in Swagger/OpenAPI annotations
- [ ] Add `CHANGELOG.md` to track refactoring changes
- [ ] Create `DEPLOYMENT.md` with production deployment steps

---

## ?? GIT - Commit Strategy

### Recommended Commit Flow
```bash
# After security fixes
git add appsettings.json MyBasisWebApi/Program.cs
git commit -m "fix: update CORS to use explicit allowed origins"

# After removing generic repository
git add .
git commit -m "refactor: remove generic repository anti-pattern

- Delete GenericRepository and IGenericRepository
- Update services to use DbContext directly
- Follow ScanitechDanmark coding standards"

# After renaming projects
git add .
git commit -m "refactor: rename projects to follow standard naming convention

- MyBasisWebApi -> MyBasisWebApi_Presentation
- BLL -> MyBasisWebApi_Logic
- DAL -> MyBasisWebApi_DataAccess
- Update all namespaces and references"

# After reorganizing files
git add .
git commit -m "refactor: reorganize folder structure for better SRP

- Move entities to Logic/Entities/
- Move middleware to Presentation/Middleware/
- Group files by feature and layer"

# After adding documentation
git add .
git commit -m "docs: add comprehensive XML documentation

- Add XML docs to all public APIs
- Add inline WHY comments to complex logic
- Follow ScanitechDanmark documentation standards"
```

---

## ? Final Validation

Before considering refactoring complete:

- [ ] Solution builds without errors or warnings
- [ ] All tests pass
- [ ] Application runs and all features work
- [ ] No generic repositories exist
- [ ] CORS uses explicit allowed origins
- [ ] All projects follow naming convention: `ProjectName_Layer`
- [ ] All folders match namespace structure
- [ ] All public types have comprehensive XML documentation
- [ ] All classes are sealed unless designed for inheritance
- [ ] All async methods accept CancellationToken
- [ ] All constructor parameters are validated
- [ ] Code follows ScanitechDanmark standards
- [ ] Git history is clean with clear commit messages
- [ ] Documentation is up to date

---

## ?? Progress Tracking

**Quick Stats:**
- ? Completed: 6 items
- ?? Critical: 2 items remaining
- ?? High: 1 item remaining  
- ?? Medium: 6 sections remaining
- ?? Ongoing: 2 sections remaining

**Current Phase:** Phase 1 - Security Fixes

**Estimated Time Remaining:** 4-8 hours (depending on project size and test coverage)

---

## ?? Celebration Milestones

- ?? **Milestone 1:** Build is green (? DONE!)
- ?? **Milestone 2:** Security fixes complete
- ??? **Milestone 3:** Generic repository removed
- ?? **Milestone 4:** Projects renamed and organized
- ?? **Milestone 5:** All documentation complete
- ? **Milestone 6:** All validation checks pass
- ?? **Milestone 7:** Production ready!

---

**Last Updated:** [Current Date/Time]  
**Next Action:** Fix CORS configuration (estimated 10-15 minutes)

**Remember:** Commit frequently, test after each phase, and don't hesitate to refer back to REFACTORING_GUIDE.md for detailed instructions!
