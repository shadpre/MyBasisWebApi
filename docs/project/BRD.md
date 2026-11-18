# MyBasisWebApi — Business Requirements Document

| Field | Value |
| --- | --- |
| Document revision | 0.1.0.0 |
| Updated | 2026-11-18, Europe/Copenhagen |
| Status | DRAFT: project direction accepted; detailed requirements proposed |
| Owner | Klavs Tranborg Eriksen |
| Repository | https://github.com/shadpre/MyBasisWebApi |
| Inspected source | `master`, commit `261eaf40396cf344673f66581d18e24edb71e7e5` |
| Audience | Developers, maintainers and implementation agents |

## 1. Purpose and accepted direction

Modernize the existing repository into a common, reusable Web API template and create a dedicated private project plugin. The owner selected this direction on 2026-11-18. The template should give a new application a small, understandable foundation with secure authentication, explicit authorization, database persistence, diagnostics and reproducible setup.

The project plugin preserves this project's requirements, decisions, verified state and next task. It does not make an API deployment, provide a running MCP server, or independently grant source or infrastructure permissions.

This document owns business rationale and goals. [PRD.md](PRD.md) owns product features and journeys; [SRS.md](SRS.md) owns exact behavior and acceptance; [TDD.md](TDD.md) owns implementation design. [DECISIONS.md](DECISIONS.md) is the single project decision register, and [BACKLOG.md](BACKLOG.md) owns execution order.

## 2. Current situation and evidence

The owner already has a .NET API foundation containing Identity, JWT authentication, SQL Server persistence and documentation. Reusing it without modernization would also copy its inconsistent tests, outdated setup and unprotected role-management operations into subsequent applications.

The initial source inspection established the following baseline. These observations are not runtime or deployment verification.

| Baseline observation | Evidence at inspected commit | Business consequence |
| --- | --- | --- |
| Active projects target `net9.0` | Presentation, BLL and DAL project files | Runtime and direct dependencies require a compatible update before reuse |
| Role authorization exists only as a TODO | `MyBasisWebApi/Controllers/RolesController.cs` | A copied application can inherit an unprotected privileged operation |
| Middleware assigns public cache headers broadly | `MyBasisWebApi/Program.cs` | Security-sensitive responses lack a deliberately scoped cache contract |
| One test is empty; the other test project references a missing project filename | `MyBasisWebApi.Tests/UnitTest1.cs`, `TESTS/TESTS.csproj` | Existing test files do not establish readiness of authentication or authorization |
| Test projects are absent from the solution | `MyBasisWebApi.sln` | A solution-level build or test result can omit intended verification |
| No versioned Actions workflows or Dependabot configuration were found | Complete source tree | Reuse lacks a verified automated build and dependency-maintenance baseline |
| README describes the code as production-ready | `README.md` | Documentation overstates the evidence available to a downstream maintainer |

Actual consumers, deployed instances, operational load, current maintenance effort and reuse frequency are UNKNOWN. No production incident or exploitation has been established. Preserve compatibility until affected consumers and breaking changes have been assessed.

## 3. Users and responsibilities

| Stakeholder | Need and responsibility |
| --- | --- |
| Owner / primary developer | Chooses project direction, material scope and policy exceptions; uses the template for future APIs |
| Implementing developer | Creates a new application from verified sources and completes one real end-to-end flow |
| Template maintainer | Maintains dependencies, contracts, release history, instructions and known limitations |
| Consumer application developer | Needs stable, documented HTTP authentication and error behavior |
| Future application operator | Needs explicit environment requirements, safe configuration and useful diagnostics |

The owner is the confirmed initial audience. Broader adoption, external customers and a commercial product are not established requirements.

## 4. Business goals and measurement

| ID | Goal | Baseline | Desired evidence | Horizon |
| --- | --- | --- | --- | --- |
| BRD-G01 | Reduce repeated setup when starting an API | Reuse effort is unmeasured; existing files retain project-specific names and assumptions | A developer creates one distinctly named application using the documented process, builds it and completes the real database-backed flow; record steps, failures and elapsed effort | First template candidate |
| BRD-G02 | Prevent common security defects from propagating | Missing role access control and broad public-cache headers are observed | Anonymous and insufficiently privileged callers cannot modify roles; authentication responses meet the approved cache contract; secrets are externalized | Before any reusable release claim |
| BRD-G03 | Keep the foundation understandable and maintainable | Existing .NET 9 stack has duplicated concerns and unused or inconsistently connected projects | A documented compatible runtime/dependency set, justified component boundaries and no speculative business features | Modernization MVP |
| BRD-G04 | Make building, diagnosing and maintaining copies repeatable | Build, test and runtime outcomes have not been established in this work | Versioned setup, meaningful checks, persistent safe logging and observable source/build/version identity are demonstrated | Modernization MVP and release preparation |
| BRD-G05 | Preserve accurate project knowledge across sessions | Historical completion documents coexist with unverified behavior | Repository documents and private plugin distinguish proposed, implemented, verified and released states, with exact source references | Every meaningful checkpoint |
| BRD-G06 | Support later integrations without burdening the core | No concrete AI, ERP or hosted integration use case is approved for this template | An extension boundary can be described without credentials, paid calls, provider resources or mandatory AI dependencies | Design now; implementation only for a real requirement |

No percentage saving, performance promise, adoption target or revenue projection is asserted. The first measured reuse exercise supplies the initial baseline. Evaluate its results before adding packaging or automation complexity.

## 5. Options and rationale

This is an internal engineering decision, not a product-market launch. The comparison below uses the existing source and project needs; it is not a current competitive or pricing survey.

| Option | Benefit | Cost or limitation | Disposition |
| --- | --- | --- | --- |
| Modernize the existing repository | Retains the selected C#/.NET, SQL Server and Identity foundation and existing owner knowledge | Requires a truthful audit, security corrections and dependency cleanup | Owner-selected direction |
| Start from a minimal SDK-generated API | Small initial surface and fewer inherited choices | Authentication, persistence and project-specific working conventions must be assembled again | Useful simplicity benchmark; no replacement selected |
| Adopt a third-party starter framework | Could supply packaged features | Introduces architecture, upgrade and license commitments before a concrete need exists | Not selected; reconsider only for a documented gap |
| Copy the current repository unchanged | Lowest immediate preparation effort | Repeats verified defects and misleading readiness claims | Does not meet BRD-G02 or BRD-G05 |

Use existing open standards and platform facilities where they meet the required behavior. A library's previous inclusion is not proof that its current license, cost or complexity suits a general template. The technical source assessment belongs in TDD and the decision register.

## 6. MVP scope

- Modernize to the owner's .NET 11 target after current release and dependency compatibility verification.
- Preserve SQL Server, EF Core, Identity and controller-based HTTP APIs as the initial supported foundation.
- Deliver one real persistent flow: register, log in, access an authorized endpoint, refresh authentication and manage roles under administrator authorization.
- Provide safe configuration, bounded validation, deliberate cache behavior, useful error contracts and structured diagnostics.
- Make the reuse and rename process explicit and verify it against a distinct application name.
- Repair existing build/test integration and add the smallest meaningful checks needed for the observed risks.
- Establish versioning, dependency maintenance, release notes and truthful startup/usage documentation.
- Publish the dedicated private project plugin with project-specific sources and verifiable status.

## 7. Boundaries

The MVP does not include ERP/CRM or payroll logic, a frontend, multi-tenant isolation, public SaaS operation, an identity provider service for third parties, automatic deployment to an existing server, or publishing packages to a public registry.

Windows and Linux portability is a design and verification goal. It does not select a hosting provider, distribution, reverse proxy, hostname, certificate, runtime host or database subscription. A future project's hosting decisions remain its own.

OpenAI use for analysis and development is separate from OpenAI inside the delivered API. An optional future adapter may be evaluated, but core startup and authentication must not depend on an AI account or provider availability. No API spend or automatic model invocation is approved by this document.

## 8. Constraints, cost drivers and dependencies

| Constraint or dependency | Treatment |
| --- | --- |
| Existing source and unknown consumers | Record observed contracts and migration impact before breaking them; do not invent deployed consumers |
| Runtime and dependency compatibility | Verify current stable versions and licensing together; document material incompatibilities |
| Real SQL Server access | Required for the persistent flow; missing access blocks that validation, not independent document or source preparation |
| Identity and secret management | No checked-in usable secrets or default administrator credentials; define controlled administrator bootstrap |
| Maintainability | Prefer a small core and explicit extension seams over broad framework adoption |
| Work allocation | One active implementation issue per repository by default; keep separate backlog work distinct |
| Time and budget | Owner availability and delivery deadline are UNKNOWN; no external spend is authorized here |

Potential costs are developer time, a real local/test SQL Server environment, CI consumption, optional library licenses and future hosting. No price or license entitlement is assumed.

## 9. Risks and smallest useful validation

| Risk | Consequence | Bounded response |
| --- | --- | --- |
| Readiness claims exceed evidence | Unsafe or broken behavior gets copied | Tie the candidate to actual source, build, targeted security checks and one reuse run |
| Framework and package upgrades are treated independently | A nominal upgrade fails to restore or run | Verify one coherent version set and licensing before fixing project versions |
| Generalization becomes a framework rewrite | More maintenance than value | Keep the first acceptance centered on one API and one real authentication flow |
| Existing consumers rely on old behavior | Breaking changes surprise downstream users | Ask for or inspect known consumers at the breaking-change boundary; record UNKNOWN otherwise |
| The plugin becomes a second conflicting specification | Future work follows stale decisions | Link canonical repository paths and retain a dated snapshot with a verified source SHA |

## 10. Decisions and open questions

The accepted direction is modernization into a common API template with its own private project plugin. All other material decisions are recorded once in [DECISIONS.md](DECISIONS.md), including their source, status and implications.

| Open question | Impact | Owner and next resolution | Blocks |
| --- | --- | --- | --- |
| Which applications already consume this repository? | Determines compatibility and migration requirements | Owner; resolve before an incompatible public contract change | Only the dependent breaking change |
| Which distribution form is most useful? | Repository-copy, GitHub template or `dotnet new` packaging have different maintenance costs | Owner with maintainer; use one verified copy/rename flow first | Public packaging, not core modernization |
| What token/session defaults should the common template adopt? | Security and client behavior | Owner and technical design; retain the existing 10-minute access-token setting until a replacement is explicitly recorded | A change to these defaults |
| How is the first administrator provisioned safely? | Enables privileged acceptance without an open bootstrap endpoint | Technical design, reviewed with owner when operational access is needed | Administrator bootstrap and role acceptance |
| Which Windows/Linux environments will be used for acceptance? | Determines actual portability evidence | Implementer records available runtimes and real SQL Server access | Claims about untested targets |

## 11. Authority and completion

The owner confirmed the project direction and project-plugin deliverable on 2026-11-18. Codex authors and reconciles the project documents and agent instructions. IDE GitHub Copilot remains the default product-code implementer unless a current, explicit project-specific exception is recorded. The direction does not itself prove a separate grant for every repository mutation, merge, online review, provider operation or deployment.

The modernization is complete only when the agreed MVP behavior is implemented and verified at an identified source revision, the documented reuse exercise succeeds, release/version information is coherent and the private project plugin reflects the actual result. Documentation publication alone is a completed foundation, not a completed API modernization.

## 12. Source register

Source inspection date: 2026-11-18. All repository links below intentionally pin the inspected baseline; refresh them when recording new implementation evidence.

- [Repository baseline](https://github.com/shadpre/MyBasisWebApi/tree/261eaf40396cf344673f66581d18e24edb71e7e5)
- [Presentation project](https://github.com/shadpre/MyBasisWebApi/blob/261eaf40396cf344673f66581d18e24edb71e7e5/MyBasisWebApi/MyBasisWebApi_Presentation.csproj)
- [Role administration](https://github.com/shadpre/MyBasisWebApi/blob/261eaf40396cf344673f66581d18e24edb71e7e5/MyBasisWebApi/Controllers/RolesController.cs)
- [Application startup and cache middleware](https://github.com/shadpre/MyBasisWebApi/blob/261eaf40396cf344673f66581d18e24edb71e7e5/MyBasisWebApi/Program.cs)
- [Solution](https://github.com/shadpre/MyBasisWebApi/blob/261eaf40396cf344673f66581d18e24edb71e7e5/MyBasisWebApi.sln) and [test project reference](https://github.com/shadpre/MyBasisWebApi/blob/261eaf40396cf344673f66581d18e24edb71e7e5/TESTS/TESTS.csproj)
- Owner direction: the project conversation on 2026-11-18; no broader action authority is inferred.
