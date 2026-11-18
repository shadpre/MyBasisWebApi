# MyBasisWebApi

MyBasisWebApi moderniseres til en fælles, genbrugelig Web API-skabelon med C#, ASP.NET Core, SQL Server og Identity.

**Aktuel status:** Projektgrundlaget er under etablering. Den eksisterende produktkode er fortsat på .NET 9. Opgraderingen til .NET 11, sikkerhedsrettelserne og den færdige skabelon er planlagt og er endnu ikke implementeret eller kørselsverificeret.

## Projektets mål

Skabelonen skal give et enkelt udgangspunkt til nye API-projekter med registrering, login, bearer tokens, sikker fornyelse af sessioner, adgangskontrol, inputvalidering, tydelige fejl og vedvarende logging. Den skal kunne genbruges under et nyt projektnavn uden hemmeligheder, lokale indstillinger eller projektspecifik tilstand.

Udgangspunktet er controllerbaseret ASP.NET Core og en rigtig SQL Server-database. Windows og Linux er planlagte værtsmiljøer; faktisk afprøvede kombinationer registreres under implementeringen. Hostingudbyder, brugergrænseflade og offentlig adresse er ikke valgt.

## Status og dokumentation

| Dokument | Indhold |
| --- | --- |
| [Projektstatus](docs/project/STATUS.md) | Verificeret kildeversion, udført arbejde, begrænsninger og næste opgave |
| [Beslutninger](docs/project/DECISIONS.md) | Bekræftede valg, forslag og konkrete åbne spørgsmål |
| [BRD](docs/project/BRD.md) | Formål, brugere og værdi |
| [PRD](docs/project/PRD.md) | Funktioner, brugerforløb og produktaccept |
| [SRS](docs/project/SRS.md) | Præcise krav og observerbare acceptkriterier |
| [TDD](docs/project/TDD.md) | Teknisk design; TDD betyder her Technical Design Document |
| [De 20 moderniseringsopgaver](docs/project/BACKLOG.md) | Prioriteret arbejdsplan med afhængigheder |
| [Fund i eksisterende kode](docs/project/BASELINE.md) | Dokumenterede udgangspunkter og kildehenvisninger |
| [Tekniske kilder](docs/project/SOURCES.md) | Kontrollerede versions- og kompatibilitetskilder |

De tekniske kontrakter er på engelsk til udviklingsagenter. Denne introduktion og kommende bruger-/driftsvejledninger er på dansk.

## Teknisk udgangspunkt og mål

| Område | Eksisterende kode | Moderniseringsmål |
| --- | --- | --- |
| Runtime | .NET 9 | Stabil .NET 11; SDK 11.0.100 verificeret 18. november 2026 |
| API | Controllers og JWT | Bevares med håndhævet adgangskontrol og tydelige kontrakter |
| Data | SQL Server, EF Core og Identity | Bevares; faktisk SQL-version og compatibility level verificeres |
| Projektstruktur | Presentation → BLL → DAL; separat Domain-projekt | Enkle ansvar; ændringer udføres i en afgrænset opgave |
| Tests | Tom test og forældet projektreference | Kontroller af rigtig funktion og sikkerhedsregressioner |
| Automatisering | Ingen Actions-/Dependabot-konfiguration | Afgrænset build og dependency-vedligeholdelse |

Den gamle README kaldte API'et produktionsklart. Det er ikke verificeret for den nuværende kode. Fundene omfatter manglende administratorbeskyttelse på rolleændringer, for brede cache-headere og refresh-tokenrotation uden dokumenteret atomisk engangsbrug. Se fund og acceptkrav før brug i drift.

## Første implementeringsopgave

[TASK-01](docs/project/tasks/TASK-01.md) etablerer en reproducerbar .NET 11-løsning og korrekte projektreferencer. Resten af planen starter ikke automatisk.

Default-branchen er `master`. Grundlaget forberedes på `docs/api-template-foundation`. IDE GitHub Copilot er den nuværende standard til produktkode; ChatGPT/Codex vedligeholder dokumentation og projektplugin. En ændring af rollefordelingen skal komme fra ejeren for netop dette repository.

Der er endnu ingen kørselsverificeret .NET 11-startvejledning. Tidligere kommandoer er bevaret som historik. Databaseadgang og første administrator skal fastlægges før det fulde login-/rolleforløb kan accepteres.

## Projektplugin

Det private projektplugin hedder **MyBasisWebApi Projekt** og samler beslutninger, kildehenvisninger, status og næste afgrænsede opgave. Faktisk udgivelsesstatus og adgangslink registreres i [projektstatus](docs/project/STATUS.md).

Pluginet er dokumentations- og arbejdsstøtte. Det starter ikke en server eller automatisk baggrundsarbejde. OpenAI i selve API'et er vurderet som et senere valgfrit modul; kernen skal fungere uden AI-konto eller betalte kald.

## Versioner og historik

Produktversioner følger `MAJOR.MINOR.BUILD.REVISION`, eksempelvis `1.2.3.4`. Faktiske releases skal have changelog, release notes og tilsvarende Git-tag. Plugin- og NuGet-pakker bruger deres understøttede SemVer med en tydelig sammenhæng til produktversionen. Projektgrundlaget udgiver ikke en produktversion.

Den gamle README og Copilot-instruktionerne ligger uændret i [historikmappen](docs/archive/README.md). Gamle refaktoreringsrapporter i roden beskriver tidligere arbejde; [projektstatus](docs/project/STATUS.md) er indgangen til aktuelle oplysninger.

LICENSE.txt indeholder ufyldte ophavsretsfelter. Attribution og bibliotekernes licensvilkår afklares før videre distribution.
