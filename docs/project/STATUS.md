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

The repository and private plugin have been published and read back as recorded below. These are documentation/plugin outcomes; the product implementation and runtime acceptance remain outstanding.

Documentation checks: all local links in the twelve active Markdown documents were resolved; the backlog has exactly twenty distinct tasks; all nine PRD features map into SRS; original README/instruction archive bytes match the baseline Git objects. A separate document-coherence pass checked the complete foundation and led to corrected task ordering, optional packaging conditions, distribution scope and instruction paths. These checks are document evidence, not executed product tests or an online Copilot review.

## Verified repository publication

- Foundation branch: [docs/api-template-foundation](https://github.com/shadpre/MyBasisWebApi/tree/docs/api-template-foundation).
- Published foundation commit: [`65aa5fb8895e2c9a435107db2c2c03771773fda6`](https://github.com/shadpre/MyBasisWebApi/commit/65aa5fb8895e2c9a435107db2c2c03771773fda6).
- Verified tree: `255444a64d00eccd08877075da9be22a8d63dd70`; parent is the recorded product baseline.
- Scope: nineteen documentation, instruction, archive and project-plugin source/packaging files. No product C#, project, solution or runtime configuration change.
- Evidence: remote Git commit/tree readback and a successful fetch of the actual remote branch; the remote tree equals the locally reviewed tree. The default branch remains `master`; no merge or pull request was performed.

This receipt is a subsequent documentation change. The plugin's bundled references deliberately remain pinned to the verified foundation commit above. Refresh the current branch/full SHA for later work rather than treating that snapshot as the latest status.

## Verified private plugin publication

- Plugin: [MyBasisWebApi Projekt](https://chatgpt.com/plugins/plugins_6afde06cb7288191a9ebb0393da52919).
- Backend ID: `plugins_6afde06cb7288191a9ebb0393da52919`.
- Actual published version: `0.1.0`; scope: `USER`; discoverability: `PRIVATE`.
- Current/latest release at readback: `pluginrel_6afde06e5724819183662455a5bdd203`.
- Documentation source: `65aa5fb8895e2c9a435107db2c2c03771773fda6`; document revision: `0.1.0.0`.
- Submitted archive SHA-256: `1bec423ae1dec93e645e9420ce84872dd5266410475a0ee9da67141dfa0b639d`.
- Package validation: exactly twelve input files, valid JSON/YAML, resolved skill references, nine of nine original-source hashes and nine of nine packaged-reference hashes matched. ZIP integrity and safe paths passed.
- Actual published-file readback: thirteen files. All eleven authored skill/reference files match the submitted package byte-for-byte. Plugin Creator normalized publisher identity in the primary manifest and added `.codex-plugin/plugin.json`; functional manifest fields, skill discovery and version match. No publisher contact details are copied into this repository receipt.
- Source-guided scenario inspection covered a fresh TASK-01, authentication changes, a stale snapshot and online-review requests without a full SHA. This is inspection of packaged instructions, not native skill execution or a product build.

This is an actually created private project plugin. Installation/activation in a particular client session was not verified. It contains project knowledge and workflow guidance, with no standalone MCP server, background job or running API. No public directory submission or additional audience sharing was performed.

## Next bounded task

`TASK-01`: establish the .NET 11 build/solution baseline. Read [tasks/TASK-01.md](tasks/TASK-01.md). Reuse the current work track after refreshing its full remote SHA and existing PR state. Do not create a parallel implementation branch or execute later backlog tasks implicitly.

Database-sensitive acceptance remains blocked until an actual authorized SQL Server target is supplied. The build task can proceed independently. Resolve specific decisions in [DECISIONS.md](DECISIONS.md) when their dependent task starts.

## Trigger preflight for the documentation track

Observed on 2026-11-18: the complete baseline tree contains no Actions workflows; the live list of repository rulesets including inherited entries is empty; the live open-PR list is empty. The owner is a personal GitHub user. The documentation push creates/updates a branch with no PR and does not add workflows, so the documented pull-request automatic-review triggers do not apply to that push. Personal review settings were not inspected or changed; this is not a claim that they are disabled. Re-evaluate the actual surfaces before creating a PR, moving one to Ready, or pushing once a matching PR exists.
