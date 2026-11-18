# MyBasisWebApi project status

Updated: 2026-11-18 | Document revision: 0.1.0.0

## Source identity

- Repository: https://github.com/shadpre/MyBasisWebApi
- Verified default branch: `master`.
- Product baseline: `261eaf40396cf344673f66581d18e24edb71e7e5`.
- Documentation work track: `docs/api-template-foundation`.
- First scoped issue: [#1 — TASK-01](https://github.com/shadpre/MyBasisWebApi/issues/1), open and prepared; implementation not started.
- Pull request: none observed or created in the documentation phase.
- Work mode: project documentation/instructions and private project-plugin creation. Product implementation is assigned to IDE GitHub Copilot by the existing workflow; no repository-specific ChatGPT/Codex exception is recorded.
- Current product implementation status: NOT STARTED in this modernization work.
- Product build/database/runtime evidence: NOT EXECUTED in the documentation phase.
- Online GitHub Copilot review: `Ikke bestilt`.
- Product release/deployment: NOT PERFORMED.

## Prepared foundation

BRD/PRD/SRS/TDD, decisions, the twenty-item backlog, source baseline, current primary sources, task instructions and concise repository guidance are prepared together. Historical README and Copilot instructions are preserved byte-for-byte in docs/archive. The current README describes the real preparation state instead of asserting production readiness.

Repository publication and plugin publication receipts are appended only after their actual readback. A local file, commit or generated archive alone is not evidence of remote publication.

Documentation checks: all local links in the twelve active Markdown documents were resolved; the backlog has exactly twenty distinct tasks; all nine PRD features map into SRS; original README/instruction archive bytes match the baseline Git objects. A separate document-coherence pass checked the complete foundation and led to corrected task ordering, optional packaging conditions, distribution scope and instruction paths. These checks are document evidence, not executed product tests or an online Copilot review.

## Next bounded task

`TASK-01`: establish the .NET 11 build/solution baseline. Read [tasks/TASK-01.md](tasks/TASK-01.md). Reuse the current work track after refreshing its full remote SHA and existing PR state. Do not create a parallel implementation branch or execute later backlog tasks implicitly.

Database-sensitive acceptance remains blocked until an actual authorized SQL Server target is supplied. The build task can proceed independently. Resolve specific decisions in [DECISIONS.md](DECISIONS.md) when their dependent task starts.

## Trigger preflight for the documentation track

Observed on 2026-11-18: the complete baseline tree contains no Actions workflows; the live list of repository rulesets including inherited entries is empty; the live open-PR list is empty. The owner is a personal GitHub user. The documentation push creates/updates a branch with no PR and does not add workflows, so the documented pull-request automatic-review triggers do not apply to that push. Personal review settings were not inspected or changed; this is not a claim that they are disabled. Re-evaluate the actual surfaces before creating a PR, moving one to Ready, or pushing once a matching PR exists.
