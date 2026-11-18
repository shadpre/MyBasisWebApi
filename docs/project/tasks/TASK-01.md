# TASK-01 — Reproducible .NET 11 solution baseline

Status: PREPARED for IDE GitHub Copilot; not executed

GitHub issue: [#1](https://github.com/shadpre/MyBasisWebApi/issues/1). Reuse it; do not create a duplicate.

## Goal

Make `shadpre/MyBasisWebApi` restore and build coherently on the verified stable .NET 11 generation and repair the solution/test project references. Produce honest evidence for this build milestone without claiming the authentication/security/template milestones are complete.

Read `../STATUS.md`, `../DECISIONS.md`, `../SRS.md` REQ-01/REQ-21, `../TDD.md` and the root `.github/copilot-instructions.md` before implementation. The product baseline is `261eaf40396cf344673f66581d18e24edb71e7e5`; the actual documentation candidate is the refreshed remote HEAD of `docs/api-template-foundation`, not a guessed commit.

## Scope

- Verify actual repo identity, clean/user-owned work, current branch/full SHA, issue and PR state. Continue this work track; one active task/branch/PR.
- Inspect all actual csproj and solution references. Include the intended meaningful test project after fixing its reference to `MyBasisWebApi_Presentation.csproj`; reconcile duplicate/empty scaffolding with a documented reason. Do not add a fake endpoint to make an obsolete assertion pass.
- Add stable SDK selection and upgrade target frameworks/compatible Microsoft packages coherently to .NET11. Verify current official SDK/EF/Identity/JWT versions and the real NuGet graph. The dated verified starting selection is SDK 11.0.100 and runtime/EF 11.0.0.
- Add EF Design explicitly where tooling needs it. Identify SQL Server/SqlClient/migration consequences, but do not execute migrations without authorized real database access.
- Review other direct package compatibility and actual licensing. DEC-005 remains a proposed dependency simplification for TASK-07; this task does not silently remove architectural dependencies or accept commercial terms. If a package blocks this bounded upgrade, document the exact conflict and prepare the smallest necessary decision.
- Make only source adjustments required by the framework/package upgrade and project-reference repair. Preserve endpoint/data contracts. Record existing security blockers separately; do not claim they are solved.
- Keep secrets out of logs, code and output. No runtime mock/InMemory substitution, new provider, cloud resource, deployment, branch rename, public package, automatic online review or unrelated refactoring.

## Validation and completion

Record actual selected SDK, restore and Release build commands and results for the corrected solution. Run existing meaningful tests if their dependencies are genuinely available; distinguish a real failure from a test whose real database is unavailable. Do not weaken or delete assertions solely for a green result. An empty test is not product coverage.

Explain the resulting project graph, dependency changes and remaining risks. Update the Danish README and affected English documents at meaningful checkpoints and before commit/push. Preserve the build/runtime distinction and state that the secured real vertical flow remains outstanding.

Commit and push task-owned changes to the approved existing track with meaningful intermediate checkpoints for a long task. Recheck review/workflow effects before the triggering operation. Verify full remote SHA after push. Before an authorized PR mutation, IDE Copilot uses authenticated `gh`/`gh api`, discovers and reuses the matching PR, and verifies automatic-review triggers. Do not silently open/Ready a PR while those triggers are unverified. Merge, release and deployment are outside this task.

Report: actual issue/PR if present, full HEAD, changed files, executed checks, failures/blocked checks, README impact, remote verification and next task. Online review remains `Ikke bestilt` unless the owner authorizes one actual PR/full HEAD/single run. Stop after TASK-01; do not start TASK-02 automatically.
