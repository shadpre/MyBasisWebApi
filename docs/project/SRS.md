# MyBasisWebApi — Software Requirements Specification

Revision: 0.1.0.0 | Date: 2026-11-18 | Status: DRAFT implementation contract

This document specifies the modernized target, not the behavior of the existing .NET 9 code. Read [PRD.md](PRD.md), [DECISIONS.md](DECISIONS.md) and [STATUS.md](STATUS.md). Requirement IDs remain stable when priorities change.

## Functional requirements

| ID | Product link | Required observable behavior | Acceptance / verification |
| --- | --- | --- | --- |
| REQ-01 | PRD-F04, PRD-F09 | The solution restores and builds with the selected stable .NET 11 SDK, without preview dependencies. Product and real test project references resolve; no empty test is presented as coverage. | AC-01: Record SDK, full source SHA, restore/build result and included projects. Build success does not establish database operation. |
| REQ-02 | PRD-F02 | Registration validates the actual command/DTO, creates a user with Identity password hashing and never permits the caller to select an administrative role. A failed registration leaves no partially privileged account. | AC-02: Real SQL-backed success and invalid/duplicate input paths have explicit outcomes and unchanged privilege assertions. |
| REQ-03 | PRD-F02 | Login authenticates credentials and returns the documented bearer-token result. Failed attempts do not reveal whether a named account exists and are subject to a bounded abuse policy. | AC-03: Check valid/invalid credentials, lockout/rate-limit behavior and absence of secrets in persistent logs. |
| REQ-04 | PRD-F02 | Protected endpoints require a valid bearer token in the Authorization header. Validate signature, allowed algorithm, issuer, audience and expiration. Query-string, form and cookie token fallbacks are not part of the core contract. | AC-04: Missing/invalid tokens give 401; insufficient privilege gives 403. Negative cases must not mutate data. |
| REQ-05 | PRD-F02 | Refresh uses a server-side, user/session-bound token record with explicit expiry and one-time consumption. A successful rotation invalidates the predecessor atomically, including under two concurrent refresh requests. | AC-05: Exactly one concurrent refresh succeeds; replay/expiry/revocation fail without issuing valid new credentials. Record transaction/concurrency evidence against real SQL Server. |
| REQ-06 | PRD-F02 | Never trust a user ID or JWT claim parsed without cryptographic validation. If an expired access JWT participates in refresh, validate all required properties except lifetime under a separate constrained validation path; otherwise remove that input deliberately and document the API change. | AC-06: Forged subject/algorithm/issuer/audience cannot choose the account or receive a token. |
| REQ-07 | PRD-F03 | Role mutations require an authenticated administrator; check current administrative authority using the selected revocation policy. Protect the last active administrator, including concurrent removals. | AC-07: Anonymous 401, ordinary user 403, permitted admin success, last-admin conflict with no role change. A comment or TODO is not enforcement. |
| REQ-08 | PRD-F03 | Invalid refresh requests do not let an unauthenticated caller revoke an unrelated user's sessions. Verified replay may revoke only the identified session family under the documented policy. | AC-08: Random or forged refresh input cannot change another account's security stamp or token state. |
| REQ-09 | PRD-F05 | Input validation executes for the actual request types that reach application services/handlers, supports asynchronous rules and propagates cancellation. | AC-09: An HTTP request violating a non-DataAnnotations FluentValidation rule produces a stable 400 error and no write. Merely registering DTO validators is insufficient. |
| REQ-10 | PRD-F05 | Return consistent ProblemDetails for expected input/auth/domain/infrastructure failures; include a correlation identifier, not stack traces, connection strings, secrets or raw provider errors. | AC-10: Inspect representative 400/401/403/404/409/429/500 outcomes and matching safe diagnostics. |
| REQ-11 | PRD-F05 | Authentication, token, role-management and user-specific responses are non-cacheable (`no-store`). Public caching is explicitly opted into only for documented public GET responses. | AC-11: Inspect success and error headers; no blanket `public` header survives. Do not rely solely on server response-cache middleware's exclusions. |
| REQ-12 | PRD-F05 | Validate required configuration at startup. Use one access-token lifetime key, explicit allowed origins, bounded limits and real database settings. Production cannot start using a documented sample JWT signing key. | AC-12: Missing/weak/sample secrets or invalid bounds produce a clear safe startup failure. Development defaults never imply production safety. |
| REQ-13 | PRD-F05 | Persist structured operational logs from the first real flow. Correlate request outcome, auth failures and administrative mutations without passwords, access/refresh tokens or complete sensitive payloads. Configure rotation/retention and make sink failure visible. | AC-13: Observe actual persistent success/failure entries and verify redaction and writable target behavior. |
| REQ-14 | PRD-F05 | Expose minimal health information appropriate to its audience. A readiness failure reflects database unavailability without leaking connection details; liveness and readiness mean different things. | AC-14: Observe reachable/unreachable real database results. No simulated green readiness. |
| REQ-15 | PRD-F01 | A generated project has its chosen project name/namespaces/configuration identity, no inherited secrets, no source project's private state and no unsafe overwrite of existing content. | AC-15: Generate two different names into empty directories; inspect collisions/exclusions and restore/build each generated project. |
| REQ-16 | PRD-F01 | Generated output excludes this project's plugin, historical records, local settings, logs, build output and governance snapshots unless explicitly useful to the generated project. | AC-16: Inspect the actual emitted tree and secret scan results. Template packaging does not copy the maintainer's entire repository. |
| REQ-17 | PRD-F06 | Provide useful build checks and dependency maintenance covering NuGet and GitHub Actions. Activate only intended triggers, with least-privilege credentials and no automatic online Copilot review or deployment. | AC-17: Inspect triggers/permissions; bind actual workflow results to the full candidate SHA. Missing Dependabot configuration is not treated as enabled. |
| REQ-18 | PRD-F06 | Product version, release notes, changelog and source/build identity agree. Preserve a known-good artifact and migration/recovery notes where an installation exists. | AC-18: Check actual version/readback and artifact digest for a release; a documentation revision or plugin publication is not a deployed product. |
| REQ-19 | PRD-F08 | Optional OpenAI functionality cannot be required for core startup/auth/database behavior. Any selected module uses explicit account/data/budget authority and returns a validated proposal, not permission to mutate the system. | AC-19: Core works without a package/key/account; no unsolicited model call, role tool or database effect occurs. Module runtime remains unimplemented in this foundation. |
| REQ-20 | PRD-F07 | Keep BRD/PRD/SRS/TDD, English repository instructions, Danish user guidance and the private project plugin aligned with a verified source revision. | AC-20: Check complete file links, requirement traceability, source hashes and actual plugin publication/readback; never equate plugin publication with product completion. |
| REQ-21 | PRD-F09 | Verification exercises actual product behavior and preserves useful regressions; database-sensitive assertions use the selected real SQL Server. | AC-21: Report exact commands, source SHA, environment and actual outcomes. Empty tests, excluded projects and unexecuted checks do not count as passes. |

## Non-functional requirements

| ID | Constraint | Verification |
| --- | --- | --- |
| NFR-01 | Keep dependencies and architecture proportional; no mandatory ERP, company, UI, tenant, cloud or agent product. | Review dependency graph and generated tree. |
| NFR-02 | Support a Windows or Linux host for the API; SQL Server may be external. Windows LocalDB is an optional Windows development route, not the portable baseline. | State tested OS/runtime/database combinations explicitly. Do not claim untested cross-platform execution. |
| NFR-03 | Use real SQL Server for provider-sensitive migrations, refresh concurrency and admin invariants. No InMemory provider or test double in runtime. | Inspect runtime dependencies and execute focused integration checks only with authorized access. |
| NFR-04 | Respect cancellation and bounded work; no broad retry of authentication/administrative writes with an unknown result. | Trace HTTP cancellation and datastore calls; inspect error recovery. |
| NFR-05 | Use server-controlled UTC timestamps and stable identifiers for token/session audit. | Verify clocks, expiry boundaries and persisted event correlation. |
| NFR-06 | Give AI implementers English requirements/instructions and users Danish setup/operation guidance. | Validate complete files by audience and actual available commands. |
| NFR-07 | Record performance measurements only with workload, environment and sample size. | No invented latency, throughput or efficiency targets in this foundation. |

## Data and migration rules

Keep Identity users, roles and memberships as existing persistent business state. Model refresh sessions with a unique token hash, user/session family, expiry, consumption/revocation metadata and concurrency protection. Never store reusable plaintext refresh tokens as the durable verification secret. Choose the exact schema in TASK-05 after OPEN-03 is resolved.

Registration name validation must accept legitimate Unicode names, including Danish letters; do not restore the old ASCII-only rule when connecting the currently ineffective validators. Every supported authentication entry point must share the same sign-in, issuance and refresh semantics. Inspect actual consumers before retiring the duplicate AuthManager/handler path.

Do not reset an existing database to repair migrations. Verify the SQL Server engine version and database compatibility level before adopting EF Core 11. Its SQL Server default compatibility level is 160; select an explicit supported level if the authorized target requires it. Record the reviewed migration and recovery path. Database permission is distinct from permission to edit a migration file.

## Verification phases

Early work: restore/build, source checks and narrow regressions for the concrete safety changes. First real vertical flow: register → login → authenticated access → refresh → administrator-only role change, using authorized SQL Server and persistent logs. Broader automated regression coverage follows a working flow; no universal test-count or coverage quota applies.

An unavailable database makes the dependent integration evidence BLOCKED. It does not justify a simulated runtime or a claim that an unexecuted acceptance criterion passed.

## Sources

See [SOURCES.md](SOURCES.md) for current vendor sources and [BASELINE.md](BASELINE.md) for source-level findings. [TDD.md](TDD.md) owns implementation design; [BACKLOG.md](BACKLOG.md) owns ordered work.
