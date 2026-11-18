# MyBasisWebApi — Product Requirements Document

| Field | Value |
| --- | --- |
| Document revision | 0.1.0.0 |
| Updated | 2026-11-18, Europe/Copenhagen |
| Status | DRAFT: project direction accepted; feature acceptance proposed |
| Repository baseline | `shadpre/MyBasisWebApi`, `master`, `261eaf40396cf344673f66581d18e24edb71e7e5` |
| Audience | Developers, maintainers and implementation agents |

## 1. Product definition

MyBasisWebApi is a reusable starting point for controller-based .NET APIs. Its core offers real SQL Server persistence, user authentication, explicit authorization, predictable HTTP contracts and diagnostics. A developer should be able to create a distinctly named API, configure it without editing secrets into source, and verify one real end-to-end flow before adding business features.

The dedicated private project plugin supports this repository's maintenance and development work. It supplies project context and navigation to canonical documents; it is not an API host or a separate implementation agent.

[BRD.md](BRD.md) owns the rationale and BRD-G goals. [SRS.md](SRS.md) defines exact requirements, negative cases and verification; [TDD.md](TDD.md) defines the implementation. [DECISIONS.md](DECISIONS.md) is authoritative for accepted/proposed choices, and [BACKLOG.md](BACKLOG.md) owns task order.

## 2. Audiences and product needs

| Audience | Concrete situation | Desired outcome |
| --- | --- | --- |
| Primary developer | Starts a new application that needs authenticated API endpoints | A small working foundation with a clear place to add the first business operation |
| Template maintainer | Updates dependencies or corrects a security problem | A bounded change with reliable verification, migration notes and version identity |
| API consumer developer | Implements registration, login and authorized requests | Accurate HTTP examples, error behavior and session lifecycle documentation |
| Future operator | Runs a derived API on a selected host | Explicit prerequisites, externalized configuration and diagnostics that identify failures safely |

No visual admin UI, customer portal, multi-tenant experience or public marketplace onboarding is required for the MVP.

## 3. Feature scope and traceability

The priorities below describe product importance. They are not an independent instruction to start multiple implementation issues. All features remain proposed or baseline-only until evidence is recorded.

| ID | Feature and value | BRD goals | Scope / importance | Dependencies | Current state |
| --- | --- | --- | --- | --- | --- |
| PRD-F01 | Repeatable creation of a distinctly named API without inherited project identifiers, secrets or generated output | BRD-G01, BRD-G05 | MVP / essential | Buildable foundation, documented rename scope | Proposed; existing repository is the source |
| PRD-F02 | Registration, login, protected access and refresh backed by real persistent user/session state | BRD-G02, BRD-G03 | MVP / essential | SQL Server access, configuration, approved session contract | Existing code; behavior unverified |
| PRD-F03 | Administrator-only role management with a controlled first-administrator path | BRD-G02 | MVP / essential | PRD-F02, authorization rules | Access-control defect observed |
| PRD-F04 | Supported runtime/dependencies and clear Windows/Linux configuration boundaries | BRD-G03, BRD-G04 | MVP / essential | Current source verification, licensing assessment | .NET 9 baseline; .NET 11 target |
| PRD-F05 | Predictable HTTP contracts, deliberate caching and useful structured diagnostics | BRD-G02, BRD-G04 | MVP / essential | Implemented flow and safe configuration | Existing facilities; broad public-cache defect observed |
| PRD-F06 | Repeatable build, dependency maintenance and versioned release information | BRD-G03, BRD-G04 | MVP / essential | Approved repository workflow, coherent solution | No versioned Actions/Dependabot configuration observed |
| PRD-F07 | Accurate developer documents, Danish user guidance and a private project plugin | BRD-G01, BRD-G05 | MVP / essential | Canonical source and verified state | Initial documentation foundation in preparation |
| PRD-F08 | A small core with explicit boundaries for later integrations | BRD-G03, BRD-G06 | Core boundary in MVP; adapters later | A concrete integration use case before implementation | Proposed; no provider selected |
| PRD-F09 | Proportionate evidence covering the real flow and observed security risks | BRD-G02, BRD-G04, BRD-G05 | MVP / essential | Working flow, repaired test references | Empty test and invalid test reference observed |

## 4. First complete journey

The first acceptance journey is a developer starting the API with an actual SQL Server database, registering a user, logging in, calling a protected endpoint, refreshing the session and exercising role management under the approved administrator policy. It includes a deliberately rejected anonymous request and a rejected ordinary-user role change.

Relevant persistent logging belongs in this first real flow. A simulated success response, runtime fake database or token stub is not completion. If a required real dependency is unavailable, record the exact missing access and continue independent preparation without claiming integration success.

## 5. Use cases

### PRD-UC01 — Create a derived API

- **Actor / goal:** Developer creates a new API from an identified template revision.
- **Trigger:** A new application needs the common API foundation.
- **Preconditions:** Candidate source is identified; supported SDK and real database prerequisites are known; destination name/path are chosen.
- **Main flow:** Read the setup guide; copy or instantiate the documented source; apply the supported rename process; supply environment configuration; restore/build; apply the documented database setup; start the API and run the published smoke flow.
- **Alternative flow:** If the destination already contains files, stop before overwriting and describe the collision. If an identifier is invalid, report the input and accepted naming rules. Missing SDK/database access produces a named prerequisite failure.
- **Outcome:** The derived API builds and runs under its chosen identity, with no accidental dependency on the source checkout or unrelated application configuration.
- **Features:** PRD-F01, PRD-F04, PRD-F05, PRD-F07, PRD-F09.

### PRD-UC02 — Authenticate and access a protected resource

- **Actor / goal:** API client registers an account and performs an authorized request.
- **Trigger:** The client needs access to a protected endpoint.
- **Preconditions:** The API and real database are available; registration policy and input requirements are documented.
- **Main flow:** Submit valid registration data; create the persistent identity without privileged roles; log in; receive the documented credentials; send the bearer token in the Authorization header; receive the permitted response.
- **Alternative flow:** Invalid input, duplicate registration, invalid credentials, missing token or insufficient permissions produce the SRS-defined safe response. A failed request does not grant extra roles or expose credential material.
- **Outcome:** Only the intended user receives the allowed access, and relevant diagnostics are available without logging passwords or tokens.
- **Features:** PRD-F02, PRD-F03, PRD-F05, PRD-F09.

### PRD-UC03 — Refresh an authenticated session

- **Actor / goal:** An authenticated client obtains renewed access using the approved refresh contract.
- **Trigger:** Access is expiring or has expired under the documented client flow.
- **Preconditions:** A valid refresh credential and corresponding persistent session state exist.
- **Main flow:** Submit the documented refresh request; validate the session and credential; issue the approved replacement credentials; persist the resulting state; return a non-cacheable response.
- **Alternative flow:** Invalid, expired, revoked or replayed refresh credentials are rejected according to SRS; concurrent refresh behavior is explicit. Transport failure does not justify an undocumented unsafe retry.
- **Outcome:** The documented session lifecycle is enforced without accepting arbitrary or reused credentials.
- **Features:** PRD-F02, PRD-F05, PRD-F09.

### PRD-UC04 — Manage user roles

- **Actor / goal:** An authorized administrator assigns or removes a permitted role.
- **Trigger:** A user's access responsibilities change.
- **Preconditions:** The actor has a valid authenticated administrator identity established by the approved controlled bootstrap or existing administration process.
- **Main flow:** Submit the user and role request; authorize the actor before changing state; validate the target and role; persist the approved change; return the documented result and record a safe audit event.
- **Alternative flow:** Anonymous and non-administrator callers are rejected without state changes. Missing users/roles and protection of the last administrative access path follow SRS. The handling of previously issued tokens after role changes is documented rather than assumed immediate.
- **Outcome:** Privileged state changes are authorized, observable and consistent with the session contract.
- **Features:** PRD-F03, PRD-F05, PRD-F09.

### PRD-UC05 — Maintain and release the template

- **Actor / goal:** Maintainer publishes a candidate that downstream developers can assess and reuse.
- **Trigger:** A security fix, dependency update or accepted template change is complete.
- **Preconditions:** Scope, implementer, repository track and required checks are established; release/publication actions have their actual authority verified.
- **Main flow:** Inspect affected contracts; update code and relevant docs within scope; run proportionate checks; verify authorized source publication; record exact revision, product version, change history, known limitations and migration impact; update the private project plugin's source pointers and dated status.
- **Alternative flow:** Failed checks or an unavailable runtime remain explicit. A successful local build does not establish remote publication, a release or a deployed runtime.
- **Outcome:** The candidate's contents and evidence can be identified and compared with the previous version.
- **Features:** PRD-F04, PRD-F06, PRD-F07, PRD-F09.

## 6. Product acceptance

These outcomes must map to concrete SRS requirements and verification methods before an implementation is accepted. They are not results already achieved.

| ID | Observable acceptance | Related features / use cases |
| --- | --- | --- |
| PRD-AC01 | A fresh derived API with a different valid name restores, builds and completes the documented real-database journey; destination collisions do not overwrite unrelated files | PRD-F01, PRD-UC01 |
| PRD-AC02 | Registration/login/protected access/refresh work against persistent SQL Server state, and restart behavior follows the documented session design | PRD-F02, PRD-UC02–03 |
| PRD-AC03 | Anonymous and ordinary-user callers cannot change roles; the administrator path is explicit and contains no shared default credentials | PRD-F03, PRD-UC04 |
| PRD-AC04 | Authentication and other designated sensitive responses meet the explicit non-cacheable policy; any public caching is deliberately limited to approved endpoints | PRD-F05, PRD-UC02–04 |
| PRD-AC05 | The coherent runtime and package set is documented with compatibility and licensing evidence; actual supported OS verification is distinguished from design intent | PRD-F04, PRD-UC01, PRD-UC05 |
| PRD-AC06 | Requests and failures can be correlated through persistent structured diagnostics without credential leakage; sink/configuration failures are visible | PRD-F05, PRD-UC01–04 |
| PRD-AC07 | The solution references the intended projects correctly; selected existing and targeted checks verify behavior rather than an empty success placeholder | PRD-F09, PRD-UC05 |
| PRD-AC08 | Approved build/dependency workflows exist and their actual execution state is recorded; product version and release notes identify the delivered source | PRD-F06, PRD-UC05 |
| PRD-AC09 | Four coherent project documents, English agent instructions, Danish user guidance and the private plugin reflect the same verified candidate and known limitations | PRD-F07, PRD-UC01, PRD-UC05 |
| PRD-AC10 | Core startup, authentication and the smoke journey require no AI credentials, model calls or optional external integration accounts | PRD-F08, PRD-UC01–03 |

## 7. Reuse and naming contract

The initial product need is one verified reuse process. Whether its implementation uses a documented copy/rename procedure, GitHub's template facility or a `dotnet new` package is an explicit decision, not an assumed product feature.

The process must define names for projects, namespaces, assemblies, solution, configuration identifiers and documentation examples. Distinguish source identifiers that must be replaced from contractual names that must remain stable. A blind repository-wide text replacement is not acceptance evidence.

Never overwrite an existing destination silently. Exclude usable secrets, logs, build outputs, local database contents and developer-specific state from the reusable output. Verify the resulting application in a separate destination, not merely by building the original source.

## 8. Boundaries and later options

- Keep business entities and domain behavior outside the common core until a specific project needs them.
- Retain controller-based HTTP APIs and the selected SQL Server/Identity foundation; alternatives need an actual requirement and decision.
- Treat multitenancy, external identity federation, a visual administrator UI and hosted operation as separate future scopes.
- Assess optional OpenAI integrations only for a concrete use case. An adapter may be considered later; accounts, data transfer, scheduled work and usage charges remain separate decisions.
- Do not add speculative extension frameworks merely to demonstrate extensibility.
- Public package publication, default-branch renaming and deployment to any host are distinct actions, not implicit results of preparing the template.

## 9. Validation and delivery approach

Use the existing source as the starting point. Establish a buildable candidate and the real persistent journey before broad new test infrastructure. Preserve meaningful existing checks, repair broken references and add focused verification for the observed authorization, cache and session risks.

The first implementation handoff requires the four project documents and applicable agent instructions to be available at a verified source location. IDE GitHub Copilot is the default implementer. Codex owns final document coherence; any product-code executor exception must come from an actual project-specific owner decision.

No fixed test count or coverage percentage is required. A test that does not exercise the application is not evidence of the product journey. Optional external reviews are distinct from required build/security acceptance and retain their separate authorization rules.

## 10. Open product decisions

| Question | Current treatment | Impact |
| --- | --- | --- |
| Existing derived applications and compatibility promises | UNKNOWN; record confirmed consumers before incompatible changes | Contract migration and release version |
| Token lifetime, refresh lifetime and session/revocation defaults | Existing access-token configuration is 10 minutes; no replacement is accepted here | Client behavior and security acceptance |
| Administrator bootstrap mechanism | Must be controlled and documented; no open registration-to-admin path | PRD-UC04 acceptance |
| Reuse distribution mechanism | One verified rename flow first; packaging form remains a recorded decision | PRD-UC01 delivery |
| Confirmed Windows and Linux verification environments | Record available actual targets and evidence in TDD/status | Portability claims |
| Optional AI or other adapter | Deferred until a concrete requirement, permitted data flow and budget exist | No block to core MVP |

These questions block only the dependent decision or acceptance claim. They do not require pausing independent documentation, source inspection or preparation.

## 11. Evidence and change control

The baseline source and limitations are recorded in [BRD.md](BRD.md#2-current-situation-and-evidence) and its pinned source register. The owner-confirmed direction is dated 2026-11-18. Feature definitions, priorities and acceptance in this revision are a proposed implementation contract; no item is marked implemented merely because it appears here.

When a feature changes, update its stable PRD-F/UC/AC references, the affected SRS/TDD sections and backlog entry in one coherent documentation change. Preserve source history, actual review status and current action authority. The private plugin must refer to canonical repository documents once they are installed and verified.
