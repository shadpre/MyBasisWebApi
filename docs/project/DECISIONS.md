# MyBasisWebApi decisions

Revision: 0.1.0.0 | Recorded: 2026-11-18, Europe/Copenhagen

## Authority and status

The owner selected modernization of `shadpre/MyBasisWebApi` as a reusable API template with its own project plugin on 2026-11-18. This is the project goal and the authority for preparing its foundation and privately publishing its project plugin. Design choices below are distinguished from express owner decisions. This file records authority; it cannot create additional authority.

The existing shared workflow assigns product implementation to IDE GitHub Copilot. No project-specific ChatGPT/Codex product-code exception has been established for this repository. ChatGPT/Codex authors and maintains project documents, repository instructions and the project plugin. The authorized documentation work uses `docs/api-template-foundation`, based on the verified `master` commit recorded in [STATUS.md](STATUS.md). No default-branch rename or direct-default-branch write is part of this work.

## Decision register

| ID | Status | Decision and consequence |
| --- | --- | --- |
| DEC-001 | OWNER CONFIRMED | Modernize this repository as the owner's reusable Web API template and give it a dedicated private project plugin. Keep concrete project state here, not in the shared plugins. |
| DEC-002 | OWNER BASELINE / SOURCE VERIFIED | Target the current stable .NET 11 generation. On the recorded source date, SDK 11.0.100 and runtime/ASP.NET Core 11.0.0 are available. This is a target, not an implemented upgrade. |
| DEC-003 | PRESERVED BASELINE | Retain controller-based ASP.NET Core, SQL Server, EF Core and Identity as the core implementation context. No database-provider replacement or mandatory cloud service is introduced. |
| DEC-004 | DESIGN PROPOSAL | Keep three meaningful runtime projects: host/Presentation, Logic and DataAccess. Move infrastructure implementations behind the actual Logic contracts. Retain a Domain project only if a concrete model requires it. No generic repository framework is required. |
| DEC-005 | DESIGN PROPOSAL | Prefer ordinary application services and explicit mapping for this small template. Remove mandatory MediatR/AutoMapper coupling in a dedicated task if this proposal is accepted; otherwise document the actual entitlement and compatibility of the selected versions. Do not infer customer licence eligibility. |
| DEC-006 | SECURITY REQUIREMENT | Close anonymous role changes and blanket public caching before any distribution or exposure of the modernized template. Add effective validation and a stateful, replay-resistant refresh flow before calling authentication complete. |
| DEC-007 | WORKFLOW APPLIED | One implementation issue/branch/PR at a time. Continue the prepared documentation branch for its first implementation task when handed over; do not open a competing branch merely to get another name. Backlog entries are not simultaneous implementation instructions. |
| DEC-008 | NOT AUTHORIZED | Online GitHub Copilot review has status `Ikke bestilt`. Every individual run needs explicit consent for the actual PR and full HEAD. A code, push, PR or merge request does not include an online review. |
| DEC-009 | NOT SELECTED | No deployment host, public API URL, cloud resources, identity tenant, client UI or public marketplace release is selected. Document portable requirements; do not create these resources. |
| DEC-010 | PROPOSED DEFAULT | Initially preserve the existing ten-minute access-token configuration while making it consistent and validated. The refresh/session lifetime, device policy and administrative revocation semantics need a recorded decision before refresh implementation. Do not borrow another project's values. |
| DEC-011 | DESIGN PROPOSAL | Deliver a source template first, then a local `dotnet new` package after generated-project checks pass. Public NuGet publication and the GitHub template-repository setting are separate publication choices. |
| DEC-012 | ASSESSED: LATER | Keep product AI integration optional and outside the authentication core. A later opt-in Responses/Structured Outputs adapter is a candidate. No paid call, agent, automation, SDK installation or account connection follows from this assessment. |
| DEC-013 | SHARED VERSION POLICY APPLIED | Product versions use MAJOR.MINOR.BUILD.REVISION, changelog, release notes and tags `vA.B.C.D`. Plugin manifests and NuGet use valid SemVer independently, with an explicit mapping when a package is produced. Documentation revision 0.1.0.0 and plugin 0.1.0 do not constitute a product release. |

## Open decisions

| ID | Question | Needed before | Default handling |
| --- | --- | --- | --- |
| OPEN-01 | Which real development SQL Server version/compatibility level and credentials may the implementer use? | Database integration/migration smoke tests | Build work may proceed. Database behavior remains BLOCKED until authorized access exists. Never manufacture a working database result. |
| OPEN-02 | Accept direct services/mapping, or retain licensed MediatR/AutoMapper? | TASK-07 | Preserve behavior during early safety work; do not silently upgrade or pin libraries to avoid the decision. |
| OPEN-03 | Refresh-token lifetime, simultaneous sessions and effect of account/role revocation? | Administrative revocation before TASK-02; refresh/session policy before TASK-05 | Specify these explicitly in the scoped implementation task; no guessed 30-day/remember-me contract. |
| OPEN-04 | Is the first administrator provisioned through a one-time owner operation or an external identity provider? | First full role-management smoke test | No hardcoded account, password, first-user-wins promotion or publicly callable bootstrap endpoint. |
| OPEN-05 | Which product release number and distribution audience? | TASK-20 | README's historical `1.0.0` is not evidence of a verified released build. No release/tag now. |
| OPEN-06 | Who owns the copyright/redistribution terms of the template? | Redistribution outside the owner's projects | Existing LICENSE.txt has unfilled copyright placeholders. Preserve it until attribution is established. |

Resolve only the decision that blocks the next bounded task. Do not use this list as a reason to stop independent document, build or source-inspection work.
