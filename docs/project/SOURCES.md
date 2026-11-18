# Dated technical source register

Checked: 2026-11-18 | Audience: implementers

These primary sources support the proposed target. They are not evidence of a successful restore, runtime test, licence entitlement, account access or deployment. Recheck changing facts when implementation begins.

| ID | Source | Supported observation / use |
| --- | --- | --- |
| SRC-01 | https://dotnet.microsoft.com/en-us/download/dotnet/11.0 | .NET 11.0.0 released 2026-11-10; SDK 11.0.100; ASP.NET Core 11.0.0; C# 15; Visual Studio 2026 18.12 support. |
| SRC-02 | https://dotnet.microsoft.com/en-us/platform/support/policy | .NET 9 support ended 2026-11-10; do not call net9.0 the current supported template target. |
| SRC-03 | https://github.com/dotnet/core/blob/main/release-notes/releases-index.json | Machine-readable runtime/SDK/support evidence checked alongside the download page. |
| SRC-04 | https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes | Review EF10 as well as EF11 changes; SQL Server compatibility default becomes 160; SqlClient 7; EF Design dependency must be explicit for tooling. Do not apply Cosmos-only changes to SQL Server. |
| SRC-05 | https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/11.0.0 | Stable EF SQL Server package 11.0.0 exists. Whole-project compatibility still needs restore/build. |
| SRC-06 | https://www.nuget.org/packages/Microsoft.AspNetCore.Identity.EntityFrameworkCore/11.0.0 | Matching Identity EF generation exists. |
| SRC-07 | https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer/11.0.0 | Matching JWT Bearer generation exists. |
| SRC-08 | https://docs.mediatr.io/en/latest/Migration-Guide-14.x-to-15.0.html | MediatR 15 targets .NET10/11 and retains other target options; review migration and licence requirements. |
| SRC-09 | https://mediatr.io/ | Current MediatR licensing; actual organisation/customer entitlement is unverified. The repository already references v14, after the v13 licence-model transition. |
| SRC-10 | https://docs.automapper.io/en/latest/15.0-Upgrade-Guide.html | AutoMapper v15 introduced licence-key and API changes; do not treat upgrading from v13 as a cost/contract-neutral patch. |
| SRC-11 | https://docs.automapper.io/en/latest/17.0-Upgrade-Guide.html | Current v17 target/API changes; check actual graph if retained. |
| SRC-12 | https://www.nuget.org/packages/FluentValidation/12.2.0 | Stable FluentValidation core release supporting .NET8 and newer. |
| SRC-13 | https://docs.fluentvalidation.net/en/latest/aspnet.html | FluentValidation.AspNetCore automatic pipeline is no longer recommended/supported for new use; manual async validation or a correctly typed application pipeline is suitable. |
| SRC-14 | https://docs.github.com/en/copilot/how-tos/copilot-on-github/set-up-copilot/configure-code-review | Personal, repository and organisation automatic review settings are independent and apply to pull requests/new PR pushes. No open PR existed for the initial documentation-only branch push; PR creation still requires a fresh trigger check. |
| SRC-15 | https://github.com/openai/openai-dotnet/blob/main/README.md | Official .NET SDK and Responses support; thread-safe client guidance. No OpenAI dependency was present in this repository when inspected. |
| SRC-16 | https://developers.openai.com/api/docs/guides/structured-outputs | Candidate later text-to-DTO adapter; schema conformance does not prove domain correctness. Handle refusal/incomplete/provider failures. |
| SRC-17 | https://developers.openai.com/api/docs/guides/agents | Direct Responses, managed Agents and SDK-based orchestration are distinct choices. No managed agent is required by the core template. |
| SRC-18 | https://developers.openai.com/api/docs/deprecations | Agent Builder/Evals workflows/reusable prompt-object deprecations make them unsuitable new template dependencies; check current dates before future architecture choices. |

## OpenAI opportunity assessment

Use now: the dedicated private project plugin for source-aware development support, decisions and handover. This does not create a running agent, external MCP server or automation.

Later: an explicitly enabled Responses/Structured Outputs adapter for a concrete text-to-DTO use case. Core startup, login, role checks and database operation must work without it. Account/model access, data rules, budget and real integration evidence remain future project-specific prerequisites. No paid model call or provider setup was performed.

Do not select a C# Agents SDK on the basis of Python/TypeScript examples; a suitable official C# SDK for that separate orchestration choice was not verified in this assessment.
