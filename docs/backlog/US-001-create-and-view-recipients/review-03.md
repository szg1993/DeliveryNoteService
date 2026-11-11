# US-001: Agent review 03

- Review type: agent review
- Review date and time: 2026-11-11 11:37 Europe/Budapest (UTC+01:00)
- Authoritative story: `docs/backlog/US-001-create-and-view-recipients.md` (revised 2026-11-11 in `d518e71`)
- Plan: `docs/backlog/US-001-create-and-view-recipients/README.md`
- Harness repository and commit: `agentic-engineering`, `d19a8d75f1a6e59c2c5953d32aa0741e4fc0195f`
- Reviewed branch: `feature/US-001-create-and-view-recipients`
- Reviewed head: `0ea32fd56da0a2a6991f0961b77ad84f32371d84`
- Comparison base: original PR #1 base, `17fa518f621fbe6a6c4fac138dbf3cfb76ce0c3d`
- Review mode: follow-up review, completing the initial-review work omitted by review-02
- Previous review: [review-02.md](review-02.md), FAIL, coverage incomplete
- Previous reviewed head: `9a0ea7ba8bd4eed320a87e6a9295ab36f99a2405` (ancestor of the reviewed head)
- Committed follow-up range: `9a0ea7ba8bd4eed320a87e6a9295ab36f99a2405..0ea32fd56da0a2a6991f0961b77ad84f32371d84`
- Included working-tree diff: None. The uncommitted `AGENTS.md` to `CLAUDE.md` renames at the repository root and in `service/test` are unrelated, do not overlap the story diff, and were excluded. Their content matches the committed files apart from the renamed cross-link.
- Previous comparison base: `17fa518f621fbe6a6c4fac138dbf3cfb76ce0c3d`, unchanged. Master advanced to `7720314` (US-002 merge), which the reviewed branch contains. US-002 commits were treated as context and inspected only for effects on recipient behavior.
- Explicitly identified requirement change: the developer revised AC11, AC14, AC16, AC17 and the agreed test boundary on 2026-11-11 (`d518e71`). This review evaluates against the revised story.
- Review coverage: complete
- Explicitly included uncommitted changes: None
- Final verdict: FAIL

## Verdict and merge readiness

FAIL. Correction attempt 1 (`fa3f912`) resolves every finding from review-02 that
still applies under the revised story. The recipient validation, duplicate-code,
ID and error behavior now meets the revised acceptance criteria. SQLite tests and
the approved PostgreSQL concurrency test pass.

This review also completed the work that review-02 left unverified. That work
found one new blocking finding, F007. The refactor in `fa1a3ba` replaced the
shared Swagger generator configuration with a bare Swashbuckle registration. As
a result, the generated OpenAPI contract for the recipient operations regressed
from the reviewed review-01 contract and no longer matches the implementation.
The recipient schema filter written for this problem is never registered.
Human acceptance cannot be recorded until F007 is corrected and reviewed.

Selected starting report: review-02 is the latest committed agent review. Its
coverage was incomplete, so its items were rechecked and its omitted work was
completed here. Review-01 (PASS, complete) covered `5f5b513` and remains the
accepted baseline for code that has not changed since.

Commits in the follow-up range:

- `5cfd4f7`..`4b7ce6a`, `7720314`: US-002 implementation, reviews and merge. These
  are context. Recipient-affecting changes are the `repository` rename in
  `CreateRecipient`, the typed-DTO arrangement in
  `CreateRecipient_IgnoresClientAssignedServerFields`, the shared
  `ConcurrentCreation` helper replacing `RecipientConcurrency`, the `AppDbContext`
  `SaveChangesAsync` override, the Note foreign key to `Recipients`, and the new
  `WebAppInit` Swagger registration. None of these regresses recipient runtime
  behavior. The Swagger registration keeps F007 in place.
- `63dc447`: empty reservation marker. Administrative.
- `d518e71`: developer-approved requirement revision of the story and plan. No
  implementation changes.
- `fa3f912`: correction attempt 1 for F001, F002, F004, F005 and F006.
- `0ea32fd`: correction history in the plan, T002 and T004. These are progress
  notes only, with no status, implementation or requirement changes.

| Item | Type | First reported in | Previous severity | Outcome | Evidence |
| --- | --- | --- | --- | --- | --- |
| F001: Blank values after normalization and trimmed-length behavior | Finding | review-02 | Blocking | Resolved (correction attempt 1 succeeded) | Under revised AC14/AC17, only code and country are normalized. `fa3f912` rejects a code that is empty after trimming in `CreateRecipientDto.cs:46` and maps name, ZIP, city and address unchanged in `MapTo`. Lengths apply to the supplied values. The `CodeWhitespaceOnly`, `WhitespaceOnlyTextIsAccepted` and `*LengthIncludesWhitespace` unit tests and the HTTP whitespace-code rejection pass. The trimmed-length part is no longer applicable because revised AC14 measures supplied values. |
| F002: Duplicate-code response contract | Finding | review-02 | Blocking | Resolved (correction attempt 1 succeeded) | Revised AC11 requires 400 `Recipient.AlreadyExists`. `CreateRecipient.cs:31` (pre-check) and `CreateRecipient.cs:41-44` (translation of `UniqueKeyConflictException` from `AppDbContextBase`) both raise it. The 409 metadata was removed, and the generated OpenAPI shows only 201/400. `CreateRecipient_RejectsExistingNormalizedCode` (SQLite) and `RecipientConcurrencyTests` (PostgreSQL, run with approval) pass. |
| F003: Nonpositive recipient IDs return 404 | Finding | review-02 | Blocking | No longer applicable | The developer revised AC16 on 2026-11-11 (`d518e71`): zero and negative IDs return 404. This matches `GetRecipient`, `NonPositiveRecipientId_ReturnsNotFound` and the endpoint description. No correction was needed. |
| F004: Use-case-level validation and no-save HTTP regressions | Finding | review-02 | Blocking | Resolved (correction attempt 1 succeeded); matrix portion no longer applicable | The revised test boundary makes `Validate()` unit tests the validation seam and requires at least one HTTP rejected creation that leaves the list unchanged. `ListRecipients_RejectedCreationDoesNotChangeList` (`RecipientListingTests.cs:32`) passes. Whitespace-only DTO cases were added. Restoring the HTTP validation matrix is no longer required. |
| F005: `AlreadyExists` error-code constant | Finding | review-02 | Blocking | Resolved (correction attempt 1 succeeded) | `ErrorCodes.cs:12` is now `_prefix + "AlreadyExists"`. The unused `InvalidId` was removed. |
| F006: Formatting verification fails | Finding | review-02 | Blocking | Resolved (correction attempt 1 succeeded) | Formatter verification over 71 US-001-scoped files, including every file `fa3f912` changed, exits 0. Remaining failures are only in US-002 files; see Verification performed. |
| R001: Default-parallel suite stall | Risk | review-01 | Non-blocking risk | Unresolved (carried) | Only sequential collection execution was run. |
| R002: PostgreSQL timestamp precision | Risk | review-01 | Non-blocking risk | Unresolved inherent limit (carried) | The PostgreSQL test still uses the fixed clock and passed. |
| R003: Duplicate shared trees | Scope note | review-01 | Not a finding | No longer applicable (as recorded in review-02) | Unchanged in this range. `service/src/Common.*` is not referenced by the solution projects. |
| R004: PR/merge awaiting a human | Lifecycle context | review-01 | Not a finding | Resolved (as recorded in review-02) | PR #1 was merged at `025ec1c`. |
| Unverified: wider refactor in `fa1a3ba`/`99ae0fd` as it affects recipients | Coverage gap | review-02 | Required for PASS | Completed; produced F007 | See Architecture assessment. |
| Unverified: generated OpenAPI for the three recipient operations | Coverage gap | review-02 | Required for PASS | Completed; produced F007 | Generated with the pinned Swashbuckle CLI from the built application. |
| Unverified: runtime model versus migration snapshot | Coverage gap | review-02 | Required for PASS | Completed | The `Recipients` table matches the snapshot. One unrelated drift was found (`JobExecutions`); see N001. |

## Blocking findings

### F007: Generated OpenAPI contract for the recipient operations regressed and does not match the implementation

- First reported in: review-03 (completion of the OpenAPI and wider-refactor checks that review-02 left open)
- Introduced by: `fa1a3ba` (`Program.cs` changed `.AddMedusaSwaggerGen(moduleName)` to a bare `.AddSwaggerGen()`). The setting persists through `5cfd4f7` into `service/src/WebApp/Configuration/WebAppInit.cs:9`.
- Locations: `service/src/WebApp/Configuration/WebAppInit.cs:9` and `service/src/WebApp/RestApi/CreateRecipientDtoSchema.cs:7`
- Violated rules: technology policy `web-api/openapi.md`, which requires each documented operation to stay consistent with the endpoint implementation and request schemas to be described where inference is incomplete. Technology policy `dotnet/aspnet-core.md`, which requires response and request metadata consistent with the handler. It is also a regression from the contract accepted in review-01 (`service/openapi.json` at `5f5b513`). Clean code: unregistered dead code.

Evidence: running `dotnet swagger tofile` against the built application at the
reviewed head shows the following.

- `CreateRecipientDto` has no `required` list. All six properties are
  `"nullable": true`, with no `minLength` and no `description`. Review-01 documented
  all six as required, non-nullable, `minLength: 1`, with field descriptions. The
  implementation rejects null and empty values with 400. The `[Description]`
  texts that `fa3f912` updated in `CreateRecipientDto.cs` never reach the
  document, because `DtoPropertyDescriptionFilter` is no longer registered.
- `RecipientsSortBy` and `SortDirection` are documented as integer enums
  `[0, 1]`. Review-01 documented them as `["Id","Name"]` and `["Asc","Desc"]`, and
  the operation description says `SortBy accepts Id or Name`. The page and limit
  parameter descriptions are also lost.
- `CreateRecipientDtoSchema`, which marks every property required, non-nullable
  and `minLength: 1`, is never registered. A repository search finds only its
  declaration. `WebAppInit` registers only `NoteContractSchema`.
- Document `info` has the title
  `DeliveryNoteService.WebApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null`
  and version `1.0`. The shared configuration set the module name and the local
  version.

The operations themselves remain present with correct operation IDs, summaries,
descriptions, status codes (201/400, 200/400, 200/400/404), envelope schemas
and the `Location` header. AC9's operation presence is satisfied, but the request
and query contract shown to consumers is wrong.

Required correction: restore recipient schema metadata in the generated
document. Then confirm by regenerating the document. Either:

- register the shared configuration, `Common.AspNetCore.Swagger.SwaggerInit.AddSwaggerGen(moduleName, ...)`, keeping `NoteContractSchema`, and check that the Note schemas are unchanged in substance; or
- register `CreateRecipientDtoSchema` together with the description and enum filters needed for the recipient contract.

Otherwise, delete the dead filter and have the developer explicitly approve and
record the reduced contract. Keep any new test proportionate to the repository
testing rules.

## Non-blocking findings

### N001: Migration snapshot retains `JobExecutions`, which the runtime model no longer maps

- First reported in: review-03 (review-02's snapshot-consistency check)
- Location: `service/src/Persistence/Migrations/AppDbContextModelSnapshot.cs:26`

An EF Core 8.0.10 `IMigrationsModelDiffer` comparison of the snapshot model with
the runtime design-time model (Npgsql provider) reports exactly one difference:
`DropTable JobExecutions`. The `Recipients` table, its columns, lengths and
unique `Code` index, and the Note foreign key have no differences. The
`JobExecution` entity came from the scheduling infrastructure removed in
`fa1a3ba`. The `RemoveDemoModel` and `AddDeliveryNotes` migrations deliberately
retained it in their snapshots. The next `dotnet ef migrations add` will
therefore also emit a `DropTable JobExecutions`. This has no runtime effect on
recipients and the table is unused. The developer should decide whether to drop
it intentionally in a dedicated migration or to keep it, and review the next
generated migration accordingly. This is outside US-001's recipient behavior and
does not affect the verdict.

## Requirement traceability

Statuses marked "inherited" rely on review-01 or review-02 evidence for
unchanged code. That evidence was not rerun as a historical run. Current SQLite
and PostgreSQL test results below are from this review.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
| --- | --- | --- | --- | --- |
| AC1: 201, envelope, Location | `RecipientEndpoints.cs` POST with `Results.Created` | `CreateRecipient_ReturnsCreatedWithLocation`, `CreatedRecipient_CanBeRetrievedThroughLocation` passed (this review) | Met | None |
| AC2: Read back normalized code/country, supplied text, ID, UTC time, foreign status | `CreateRecipientDto.MapTo`, `RecipientDetailsDto.FromModel` | `CreateRecipient_NormalizesIdentifiersPreservesTextAndDerivesForeignStatus` (HU/US) passed | Met | None |
| AC3: Immediately listed | No cache; `GetRecipients` reads the repository | `ListRecipients_ImmediatelyIncludesCreatedDetails` passed | Met | None |
| AC4: Invalid creation rejected, nothing created | `dto.Validate()` before persistence | `ListRecipients_RejectedCreationDoesNotChangeList` and DTO unit tests passed | Met | None (F001/F004 resolved) |
| AC5: Deterministic list order | ID default and name/ID tie-breaker in `GetRecipients` | Listing tests passed; review-02 status inherited for unchanged code | Met | None |
| AC6: Empty list succeeds | Paged repository query | `ListRecipients_EmptyDatabaseReturnsEmptyPage` passed | Met | None |
| AC7: Existing detail returns 200 | `GetRecipient` | Detail tests passed | Met | None |
| AC8: Unknown ID returns 404 | `GetRecipient` raises `NotFoundException` | `UnknownRecipientId_ReturnsNotFound` passed | Met | None |
| AC9: All three operations in generated OpenAPI | Endpoint metadata in `RecipientEndpoints.cs` | Generated document retrieved through the Swagger CLI: all three operations, status codes and Location header are present | Met for operation presence | F007 (schema contract regression) |
| AC10: Duplicate names allowed | Unique index on `Code` only | `CreateRecipient_AllowsDuplicateNamesWithDifferentCodes` passed | Met | None |
| AC11: Normalized-code uniqueness; 400 `Recipient.AlreadyExists` sequentially and concurrently | Pre-check plus `UniqueKeyConflictException` translation in `CreateRecipient`; unique index | SQLite `CreateRecipient_RejectsExistingNormalizedCode` (3 cases) and PostgreSQL `RecipientConcurrencyTests` passed | Met | None (F002 resolved) |
| AC12: Paging defaults and ID/name sorting | `GetRecipients`, `ValidateListQuery` | Listing tests passed; inherited from review-02 for unchanged code | Met | None |
| AC13: Recognized country, uppercased, HU domestic | `RecipientCountries`, `FromModel` | HU/US HTTP cases and `CountryUnrecognized` unit cases passed | Met | None |
| AC14: Required fields; only code and country normalized; supplied-value lengths; empty trimmed code rejected; ZIP as text | `CreateRecipientDto.Validate`/`MapTo`; DB lengths match | `*Missing`, `*TooLong`, `*MaximumLength`, `*LengthIncludesWhitespace`, `CodeWhitespaceOnly`, `WhitespaceOnlyTextIsAccepted` unit tests and the HTTP preservation test passed | Met | None |
| AC15: Demo paging format, validation, limit=0 | `PageRequestQueryParam`, `ValidateListQuery`, `PagedEnvelope` | Listing tests passed; inherited from review-02 for unchanged code | Met | None |
| AC16: 400 for invalid fields, malformed IDs and paging; 404 for any absent well-formed ID including nonpositive ones; stable codes | Error middleware, `GetRecipient` | `MalformedRecipientId_ReturnsBadRequest`, `NonPositiveRecipientId_ReturnsNotFound`, malformed body and listing validation tests passed | Met | None (F003 no longer applicable) |
| AC17: Normalized code non-empty ASCII letters/digits/hyphen/underscore | `CreateRecipientDto.cs:46` | `CodeInvalidCharacters`, `CodeWhitespaceOnly` unit tests passed | Met | None |
| AC18: No postal or address verification | Length and requiredness only | `PostalFormatAndAddressAreNotVerified` passed | Met | None |
| Revised agreed test boundary (HTTP workflows plus `Validate()` unit tests, one rejected-create no-save HTTP check) | Test suites as described | Both suites run in this review | Met | None |
| Repository conventions (logging, DTO mapping, error-code pattern, formatting) | `CreateRecipient` logger pattern; `ToModel`/`MapTo`; `ErrorCodes.Recipient` | Formatter verification exit 0 on scoped files | Met | None |
| Technology OpenAPI/ASP.NET Core metadata consistency | `WebAppInit` Swagger registration | Generated document inspected | Not met | F007 |
| Historical TDD execution | Task progress records; `fa3f912` records red/green in T002/T004 | Inherited; not recreated | Met (inherited) | None |

## Verification performed

All commands ran from the repository root unless noted. No source, test, story,
plan or task file was changed.

```text
dotnet test service/DeliveryNoteService.sln --filter "Database!=PostgreSQL" -v minimal --blame-hang --blame-hang-timeout 120s -- xUnit.ParallelizeTestCollections=false
```

Result: exit 0. ApiTests passed 74/74 and UnitTests passed 182/182, with no
failures or skips.

```text
dotnet test service/test/ApiTests/ApiTests.csproj --no-build --filter "FullyQualifiedName~ApiTests.RecipientConcurrencyTests&Database=PostgreSQL" -v minimal
```

Result: passed 1/1. The developer approved this single Testcontainers run on
2026-11-11. Docker engine 29.6.1 was available. Afterwards no PostgreSQL test
container remained; only the Testcontainers reaper was briefly present. No other
Testcontainers tests were run. The test accepts either the pre-check or the
database-race path. The race translation path was verified by inspection:
`AppDbContextBase.SaveChangesAsync` converts PostgreSQL 23505 and SQLite unique
violations to `UniqueKeyConflictException`, and `CreateRecipient` catches it. The
US-002 `AppDbContext` override filters only Note-number check violations, so it
does not intercept that exception.

```text
dotnet format service/DeliveryNoteService.sln --no-restore --include-generated --verify-no-changes --include <71 US-001-scoped C# files>
```

Result: exit 0. The scope covers every C# file added or modified by `fa3f912`
(including the five files named in F006), all recipient source and test files,
`service/test/ApiTests/TestSuite/*.cs`, `ErrorCodes.cs`, and the Persistence
project files and migrations, excluding the US-002 migration.

A wider verification over all 88 C# files added or modified in the follow-up
range exited 2. It reported only US-002 files:
`service/src/Persistence/Migrations/20261110131638_AddDeliveryNotes.Designer.cs`
(final newline, import order) and
`service/test/UnitTests/DtoValidation/CreateNoteDtoTests.cs` (final newline).
These belong to US-002, which has its own accepted reviews, so they are not a
US-001 finding. They are recorded under remaining risks.

```text
cd service
dotnet tool restore
cd src/WebApp
ASPNETCORE_ENVIRONMENT=Development dotnet swagger tofile --output <temporary-directory>/openapi.json bin/Debug/net8.0/DeliveryNoteService.WebApp.dll v1
```

Result: the document was written using the pinned Swashbuckle CLI 6.9.0. The
three recipient operations were inspected and compared with `service/openapi.json`
at `5f5b513` (review-01 baseline) using `git show`. Findings are in F007. No
local database file was left behind; `app.db` is gitignored and was not created.

A model and snapshot consistency check ran in a temporary console project
outside the repository, referencing the built Persistence, Core and Common
assemblies with EF Core 8.0.10. It compared
`IMigrationsAssembly.ModelSnapshot` against `IDesignTimeModel` through
`IMigrationsModelDiffer` with the Npgsql provider, without connecting to a
database. Result: seven migrations were recognized and one difference was found,
`DropTable JobExecutions` (N001). There were no recipient differences.

## Code-guideline assessment

Applied central guidance: .NET C# style, ASP.NET Core, dependency injection
(inspection only), .NET testing (component-testing skill for test-level choice),
and Web API OpenAPI and version-endpoint guidance. Repository instructions in
`CLAUDE.md` and `service/test/CLAUDE.md` take precedence. The revised testing
rules make DTO `Validate()` unit tests the validation seam and keep HTTP tests
minimal. These supersede the central component-testing preference for HTTP-only
validation coverage, and the conflict is resolved in favor of the repository.
`EnforceCodeStyleInBuild` remains set in project props, and the build is clean
enough for the tests to run.

Correction `fa3f912` is small and readable:

- `CreateRecipient` keeps the validation, lookup, mapping, persistence and
  response sequence, with the required logger wrapper.
- The duplicate translation is a named private factory. The `CLAUDE.md` rule
  allows it because AC11 explicitly requires `Recipient.AlreadyExists` for the
  database conflict.
- The DTO follows the repository rules to preserve supplied strings and apply
  lengths to supplied values.
- `Guard.ThrowIfMissing` semantics (null or empty) are used correctly. The empty
  trimmed code is a straightforward extra condition.
- The error-code constant follows the prefix rule.

Clean-code assessment of the recipient code: 8/10. The remaining issue is F007's
dead `CreateRecipientDtoSchema` and inaccurate generated contract. Code
normalization (`Trim().ToUpperInvariant()`) appears in `Validate`, `MapTo` and
the duplicate pre-check. This was accepted in review-01 and is proportionate
here, so it is not raised as a finding.

## Architecture assessment

This assessment completes the wider-refactor inspection omitted by review-02, as
it affects recipients.

- **Layering.** Recipient model, DTOs, use cases and error codes live in
  `service/src/Core`. Persistence references Core and owns EF mapping
  (`ModelBuilderExtensions.ConfigureRecipients`). WebApp composes the layers and
  hosts thin Minimal API route modules exposed through `MapRecipientRoutes()`.
  Binding sources are explicit. Use cases are auto-registered. This conforms to
  the ASP.NET Core and dependency-injection guidance at this project's scale.
- **Shared libraries.** The active dependencies are the repository-root
  `src/Common.*` projects. The error middleware maps `DataValidationException`
  to 400, `NotFoundException` to 404 and binding failures to 400 with
  `InvalidHttpRequest`, which is consistent with AC16.
  `RequiredHeaderValidationMiddleware` affects only endpoints that declare
  required headers, so the recipient routes are unaffected. Endpoints remain
  anonymous, as agreed.
- **Demo removal and namespace consolidation.** These leave no dangling
  recipient dependency. The removed scheduling infrastructure leaves N001 in the
  snapshot.
- **Persistence.** The runtime recipient model matches the migration snapshot.
  The unique `Code` index remains the race backstop, and both duplicate paths now
  share one contract. US-002 adds a restricting foreign key from Notes to
  Recipients. This has no effect because recipients cannot be updated or deleted
  in scope.
- **Test infrastructure.** The shared `ConcurrentCreation` helper serves both
  the recipient and note concurrency tests. It has a single responsibility, a
  bounded deadline and response disposal on failure. The PostgreSQL base class
  keeps instance-scoped container ownership, and the run left no container
  behind.
- **API documentation (F007).** The shared Swagger configuration was replaced
  by a bare registration. This is a concrete operational and contract defect in
  documentation consumed outside the component. It is blocking as recorded above.

No unnecessary parallel design was found beyond the unregistered schema filter.

## Recorded deviations

- Developer-approved requirement revisions on 2026-11-11 (`d518e71`):
  - AC11: duplicate codes return 400 `Recipient.AlreadyExists` on both paths.
    This was previously 409.
  - AC14/AC17: only code and country are normalized, other text is preserved,
    and a code that is empty after trimming is rejected. Previously all fields
    were trimmed and blanks rejected.
  - AC16: nonpositive IDs return 404. Previously 400.
  - Test boundary: DTO `Validate()` unit tests are added and the HTTP validation
    matrix is reduced.

  These are acknowledged and approved. Record them in any external tracker.
- Core replaces the planned Domain/Application split. Demo routes, messaging and
  scheduling setup, related tests and the `DemoModels` table were removed
  (`RemoveDemoModel` migration). `DemoEndpoints.cs`, named in the story's
  technical constraints, no longer exists. `service/openapi.json` and
  `OpenApiSpecTest` were removed, contrary to the plan's "regenerate and keep
  passing" choice; the repository testing rules now discourage generated-OpenAPI
  tests without concrete need. These changes go beyond the plan's stated scope.
  They were first recorded in review-02. The developer should reconcile the plan
  when completing the story.
- Entity-specific error codes were renamed under the `ErrorCodes.Recipient`
  convention (`Recipient.InvalidCode`, `Recipient.InvalidCountry`,
  `Recipient.NotFound`, `Recipient.AlreadyExists`). This is approved by
  repository instructions.
- Deviations recorded in review-01 remain recorded there and were not
  re-examined.

## Unverified items and remaining risks

- R001: the cause of the default-parallel stall remains unestablished. Only
  sequential collection execution was run.
- R002: PostgreSQL timestamp precision is an inherent provider limit. The
  concurrency test uses a fixed clock.
- The PostgreSQL concurrency test cannot show which duplicate path (pre-check or
  database race) a given run exercised. The race translation is verified by
  inspection and by the shared unique-violation mapping.
- Formatter failures in the US-002 files
  `service/src/Persistence/Migrations/20261110131638_AddDeliveryNotes.Designer.cs`
  and `service/test/UnitTests/DtoValidation/CreateNoteDtoTests.cs` are outside
  US-001 scope and were not assessed as US-001 findings.
- N001 (`JobExecutions` snapshot drift) needs a deliberate decision before the
  next migration is generated.
- No other PostgreSQL or Testcontainers tests were run, in line with repository
  policy.
