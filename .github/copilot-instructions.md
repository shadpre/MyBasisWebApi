# MyBasisWebApi repository instructions

## Project and source

This repository is being modernized into a reusable, controller-based .NET API template. Read `docs/project/STATUS.md`, `docs/project/DECISIONS.md` and the current bounded task first. Use BRD → PRD → SRS → TDD → BACKLOG for intent, behavior and design. These are English AI-facing contracts; the root README and user/operations guidance are Danish.

The verified default branch is currently `master`. Do not rename it or assume `main` exists. The initial documentation/implementation track is `docs/api-template-foundation`; verify the real branches/PRs before reusing it. No project-specific direct-default-branch or ChatGPT/Codex product-code exception is recorded. IDE GitHub Copilot is the default product-code implementer. A later explicit owner decision for this exact repository may change those facts; record it rather than inferring authority from this file.

Historical root refactoring reports and `docs/archive/**` are not active instructions or current acceptance evidence. Do not load the archived company-wide instruction file as a second normative policy.

## Implementation boundaries

- Implement one selected issue/task at a time; do not execute the twenty-item backlog on a continuation request for one task.
- Preserve unrelated working-tree changes and useful tests. Do not weaken assertions, delete failing tests or add fake behavior just to obtain green checks. Explain necessary test corrections against the real contract.
- Use KISS, DRY and SOLID with real responsibilities. Keep controllers thin and SQL/EF/Identity infrastructure in DataAccess behind the required contracts. No generic repository, speculative module framework or interface for every class.
- Target the newest verified mutually compatible stable .NET11 generation under the current baseline; check SDK, packages, licensing and breaking changes. No preview dependencies or silent old-version pinning. Document concrete incompatibilities.
- Keep SQL Server and controller-based ASP.NET Core. A provider, cloud, UI, tenant or material architecture change needs its actual decision. DEC-005 is a proposal, not a licence entitlement.
- Use one primary top-level C# type per file, clear names, async I/O and supported cancellation. Add useful XML docs for affected public contracts and comments explaining non-obvious intent. Avoid ceremonial XML comments on ordinary statements.

## Security and real behavior

- Enforce auth/authorization in executable code. Anonymous callers and ordinary users cannot change roles. Never seed a default admin password or expose an unauthenticated bootstrap bypass.
- Access JWTs use the Authorization bearer header only. Do not trust merely decoded claims. Refresh must meet SRS identity-binding, authoritative-state, single-use and concurrency requirements.
- Sensitive responses use no-store; no blanket public-cache middleware. Validate the actual request types before side effects.
- Keep secrets out of Git, logs, prompts and generated projects. Reject missing/weak/known sample keys as specified. Do not print credentials during diagnosis.
- No fake provider, mock database, stub issuer or simulation in runtime/product code. Integration evidence uses authorized real SQL Server. Report missing access as BLOCKED and continue independent work.
- Persist relevant structured, correlated and redacted logs from the first real flow. Logger injection/console output alone does not prove persistence.

## Verification, documentation and delivery

- Build and exercise the real vertical flow before broad new test infrastructure. Add focused regressions for concrete risks; no universal coverage/test-count target.
- Check README and affected docs at meaningful checkpoints and before commit/push. Keep setup, behavior, versions, commands and limitations accurate; never mark a plan implemented.
- Record commands, environment, full SHA and observed outcomes. Distinguish written, committed, pushed, built, tested, packaged, released and deployed.
- Authorized repo-changing work must be committed, pushed to the approved track and verified against full remote SHA before completion. Push coherent checkpoints during long work. A blocked push means incomplete source delivery.
- Before push/PR/Ready/workflow operations, inspect actual triggers, matching PR, rulesets and applicable personal automatic-review settings. A branch with no PR/workflow differs from opening a PR. Never change protections/settings to bypass a gate.
- Every online GitHub Copilot review needs explicit owner consent for the actual PR, full HEAD and one run. Default: `Ikke bestilt`. Code/push/merge permission does not authorize it; do not repeatedly propose a declined optional review.
- IDE Copilot uses authenticated `gh pr` / `gh api` for authorized PR mutations and searches relevant PR states before creating one. Missing MCP does not imply missing CLI. ChatGPT/Codex may use a supported authenticated connector for independently authorized operations.
- Merge, public release, deployment, provider changes and machine/service shutdown retain their actual scope/authority. Green CI is not blanket permission. Stop an unverified triggering effect while continuing safe preparation.
- Product versions use MAJOR.MINOR.BUILD.REVISION, CHANGELOG, Danish release notes and matching vA.B.C.D tags. Plugin/NuGet manifests use supported SemVer with an explicit mapping. Plugin publication is not API deployment.

On stop/handover, stop new implementation and long runs. Preserve decisions, verified source state, remaining risks and next action. Return reusable prompts in a fenced text block.
