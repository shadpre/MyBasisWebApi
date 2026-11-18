---
name: mybasiswebapi-project
description: "Use for shadpre/MyBasisWebApi and its modernization into a reusable .NET API template: project status, decisions, requirements, authentication risks, dependency choices, Copilot tasks and release handover. Keep this project's facts out of shared general plugins."
---

# MyBasisWebApi project

Respond to the owner in Danish. Write AI-facing documents and reusable implementation prompts in English; put the actual prompt in a fenced text block. Use Udviklingshjælper for shared engineering methods and Projektværkstedet for product/document coherence. This project source owns its concrete requirements, decisions and status.

## Establish current context

1. Read [SOURCE.json](references/SOURCE.json) for the exact packaged documentation commit, product baseline, hashes and source paths. This is a dated snapshot, not current GitHub/runtime evidence.
2. Use an actually available authorized GitHub integration or checkout to verify repository `shadpre/MyBasisWebApi`, its actual default branch, current work branch/full SHA, open issue/PR state and `docs/project/STATUS.md`. The initial default is `master`; initial work track is `docs/api-template-foundation`. Refresh them instead of assuming they remain unchanged.
3. Read current `docs/project/DECISIONS.md` and the requested bounded task. Match the current owner request to the actual actor/actions/resource. If live access is unavailable, say which facts are snapshot-only and continue useful document analysis; do not claim current source, tests or deployment.
4. Reuse existing project/plugin identity when maintaining this plugin. Never create another similarly named project plugin merely because a session is new.

## Source map

| Need | Bundled starting reference | Canonical repository source |
| --- | --- | --- |
| Purpose and business goals | [BRD](references/BRD.md) | docs/project/BRD.md |
| Features and journeys | [PRD](references/PRD.md) | docs/project/PRD.md |
| Precise security/behavior acceptance | [SRS](references/SRS.md) | docs/project/SRS.md |
| Design and integration choices | [TDD](references/TDD.md) | docs/project/TDD.md |
| Owner decisions, proposals and authority | [Decisions](references/DECISIONS.md) | docs/project/DECISIONS.md |
| Twenty ordered work items | [Backlog](references/BACKLOG.md) | docs/project/BACKLOG.md |
| Existing source defects and evidence limits | [Baseline](references/BASELINE.md) | docs/project/BASELINE.md |
| Dated vendor facts and OpenAI assessment | [Sources](references/SOURCES.md) | docs/project/SOURCES.md |
| First bounded implementation task | [TASK-01](references/TASK-01.md) | docs/project/tasks/TASK-01.md |

Read only the references needed for the user's request. For implementation handover, verify the actual four repository documents and active `.github/copilot-instructions.md` at the full remote SHA; bundled snapshots alone do not establish that the IDE has current files.

## Project boundaries

The owner confirmed a common reusable API template and a dedicated private project plugin on 2026-11-18. The source starts at .NET9 with controllers, SQL Server, EF and Identity. .NET11 is the verified modernization target, not an already completed upgrade. Do not label the product production-ready from old README/refactoring reports.

The first real product journey is register → login → authenticated access → refresh → authorized role administration, with actual SQL Server and persistent safe logging. Address the documented unprotected roles, blanket public cache, ineffective validation and refresh integrity risks. Do not replace absent real access with a runtime fake or invent successful tests.

IDE GitHub Copilot remains the product-code implementer under the current recorded workflow. ChatGPT/Codex owns document/instruction/plugin preparation; no project-specific product-code exception was recorded at publication. Honor a later verified owner exception if one exists; this skill cannot grant one. Recognize existing valid action authority without repeatedly requesting it.

Keep one active implementation issue/branch/PR. A request to continue TASK-01 does not authorize all twenty tasks. Current default-branch renaming, public package publication, hosting, cloud resources, external identity, secrets and deployment are not selected by the project goal. Every online Copilot review requires separate actual PR/full HEAD/single-run consent; initial status is `Ikke bestilt`.

Keep OpenAI product integration optional and later. The core must run without an AI SDK/key/account. A documented future Responses/Structured Outputs adapter is not an installed integration, paid-call authorization, managed agent or automatic role/database tool.

## Maintain and report

Update canonical repository documents first within the actual authorized work track, preserving IDs, accepted decisions, user changes and source evidence. Keep detailed design proposals distinct from explicit owner decisions. Update the Danish README when actual setup/behavior changes. Rebuild this package from verified committed sources using the repository packaging workflow; do not hand-edit a second normative copy in the plugin.

Use Plugin Creator for publication, preserve identity/audience, version the plugin using supported SemVer and read back the actual release/files. Product four-part versions and plugin publication are separate. Record a publication receipt separately from source/runtime acceptance to avoid claiming deployment from a plugin update.

Report actual outcomes: prepared/written/pushed/built/tested/packaged/published/deployed. Include the full relevant source SHA, real issue/PR only if present, review status and the next bounded action. If the owner asks for stop/handover, stop dependent work and preserve an honest checkpoint. Do not start a scheduled job or another round automatically.
