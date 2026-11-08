# US-001: Agent review 01

- Review type: agent review
- Review date and time: 2026-11-08 22:31 Europe/Budapest (UTC+01:00)
- Authoritative story: `docs/backlog/US-001-create-and-view-recipients.md`
- Plan: `docs/backlog/US-001-create-and-view-recipients/README.md`
- Harness repository and commit: `agentic-engineering`, `d19a8d75f1a6e59c2c5953d32aa0741e4fc0195f`
- Reviewed branch: `feature/US-001-create-and-view-recipients`
- Reviewed implementation head: `5f5b513d23bf5a733b691dc8e1f7188a1f7c5226`
- Comparison base: `master`, `17fa518f621fbe6a6c4fac138dbf3cfb76ce0c3d`
- Review mode: initial review
- Review coverage: complete
- Explicitly included uncommitted changes: None
- Final verdict: PASS

## Verdict and merge readiness

All 18 acceptance criteria and the story's implementation/testing constraints are
met. No blocking or non-blocking finding was identified. Independent verification
passed all 115 API tests, including the real PostgreSQL concurrency scenario, with
no failures or skips. Scoped C# formatting verification also passed.

This is an agent technical review, not human acceptance or permission to merge.
The story remains In review and the plan remains active pending human review and
acceptance. No pull request or merge status was inferred.

The comparison with master includes the explicitly authorized import of 344
existing demo files in baseline commit
`96c28175a57702594123080c992b423b4f8fc2b3`. That existing source was intentionally
preserved rather than retrofitted. The review inspected the resulting service and
the complete Recipient implementation, using that baseline to distinguish existing
behavior from changes introduced by commits `63abcbf`, `d190935`, and `5f5b513`.
The separate reservation, planning, and tracking commits were checked as
administrative changes. The untracked duplicate `service/src/Common.*` directories
are excluded, do not overlap the referenced shared projects, and were not changed.

## Blocking findings

None.

## Non-blocking findings

None.

## Requirement traceability

Implementation paths below are relative to the repository root. Recipient use
cases are in `service/src/Application/Recipients`; route metadata and bindings are
in `service/src/WebApp/RestApi/RecipientEndpoints.cs`. Test names refer to
`service/test/ApiTests`. All listed test evidence was exercised by the independent
full API run unless explicitly described as implementation-history evidence.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
| --- | --- | --- | --- | --- |
| AC1: POST returns 201, created envelope, and detail Location | `CreateRecipient`, POST route and Location metadata | `RecipientCreationTests.CreateRecipient_ReturnsCreatedWithLocation`, `RecipientDetailsTests.CreatedRecipient_CanBeRetrievedThroughLocation` | Met | None |
| AC2: Read normalized details, server ID, UTC time, and foreign status | `GetRecipient`, `RecipientDetails.From`, injected `ITimeService` | Location read-back, normalization/foreign theory, client-server-field test | Met | None |
| AC3: Creation is immediately visible in the list without refresh | `GetRecipients` reads repository data directly; no cache added | `RecipientListingTests.ListRecipients_ImmediatelyIncludesCreatedDetails` | Met | None |
| AC4: Invalid creation returns 400 and creates no recipient | Validation precedes repository Add/Save | Blank/length/country rejection tests and `ListRecipients_InvalidCreationDoesNotChangeList` | Met | None |
| AC5: List identifiers/details with deterministic ordering | Explicit ID default and name sorter with ID tie-breaker | Default ordering, descending ID, and name-tie scenarios | Met | None |
| AC6: Empty storage returns a successful empty collection | Existing paged envelope with empty repository result | `ListRecipients_EmptyDatabaseReturnsEmptyPage` | Met | None |
| AC7: Existing recipient returns 200 and details | `GetRecipient` repository lookup and envelope | Created-recipient Location read-back | Met | None |
| AC8: Unknown valid ID returns 404 | `GetRecipient` throws coded `NotFoundException` | `RecipientDetailsTests.UnknownRecipientId_ReturnsNotFound` | Met | None |
| AC9: All three operations appear in generated OpenAPI | Route metadata and `service/openapi.json` | `OpenApiSpecTest`, independent operation/status/schema inspection | Met | None |
| AC10: Duplicate names with different codes succeed | No name uniqueness rule or index | `CreateRecipient_AllowsDuplicateNamesWithDifferentCodes` | Met | None |
| AC11: Trimmed uppercase code uniqueness, concurrent requests, and 409 | Normalization, unique `IX_Recipients_Code`, existing provider exception translation plus shared 409 mapping | Three sequential code variants and `RecipientConcurrencyTests.ConcurrentEquivalentCodes_CreateOneRecipientAndReturnOneConflict` on PostgreSQL | Met | None |
| AC12: Default page 1/limit 10, ID/name sorts, deterministic name ties | Demo paging/sorting types with local defaults and tie-breaker | Default, explicit page, ID directions, name directions/ties, and implicit-field direction tests | Met | None |
| AC13: Recognized uppercase two-letter country and derived foreign status | Fixed 249-entry country set; `Country != "HU"` read projection | HU/US normalization, invalid countries, AQ destination scenario | Met | None |
| AC14: Required trimmed fields, declared limits, textual ZIP | `CreateRecipient.Required`, shared `RecipientLimits`, matching EF mapping | Six blank/null fields, missing fields, five overlength/boundary fields, preserved leading-zero ZIP and internal text | Met | None |
| AC15: Demo paging format/validation and limit 0 | `PageRequestQueryParam`, `PagedEnvelope`, repository paging, validation before empty/unpaged paths | Requested page, unpaged list, empty list, invalid paging, large-offset regression | Met | None |
| AC16: 400/404 and stable error envelopes for invalid requests | Framework binding plus coded validation/not-found exceptions and existing middleware | Malformed/nonpositive/unknown IDs; invalid paging and numeric/text sorting; JSON body cases | Met | None |
| AC17: Normalized ASCII code letters, digits, hyphen, underscore | Explicit allowed-character predicate after uppercase normalization | Unsupported code theory and accepted hyphen/underscore creation scenarios | Met | None |
| AC18: No postal-format or address-existence checks | Only requiredness/length applied to ZIP/address | `CreateRecipient_DoesNotVerifyPostalFormatOrAddressExistence` | Met | None |
| Minimal APIs, DemoEndpoints pattern, application/persistence layers | Capability route extension, explicit binding sources, injected `IUseCase` implementations, repository/unit of work | Real-host HTTP suite and project-reference inspection | Met | None |
| TDD through the agreed HTTP seam | Task records document one-behavior red/green cycles; tests enter actual host and observe API responses | T002/T003 progress evidence reviewed; current 115-test run independently passed | Met | None |
| Fresh database, anonymous access, no legacy compatibility requirement | Additive fresh-database migration and explicit AllowAnonymous metadata; no legacy calls added | SQLite fixture and migrated disposable PostgreSQL fixture; requests carry no login | Met | None |
| Deferred update/delete/dashboard/search/authentication scope | Only agreed POST/list/detail routes added | Route and generated OpenAPI inspection | Met | None |
| Central version endpoint contract remains available | Existing `/api/v1/Versions/{connectedServices}` route and schema preserved | Existing `VersionsTests` and OpenAPI freshness in the 115-test run | Met | None |

## Verification performed

Working directory for the following commands: repository root.

```powershell
dotnet test service/test/ApiTests/ApiTests.csproj --no-restore -v minimal --blame-hang --blame-hang-timeout 90s -- xUnit.ParallelizeTestCollections=false
```

Result: **115 passed, 0 failed, 0 skipped**, test duration 8 seconds. Build
succeeded. The blame collector reported that all tests finished and did not need
a sequence file. This independently exercises the default SQLite-backed suite,
the isolated PostgreSQL concurrency test, OpenAPI freshness, and existing
logging, scheduling, messaging, paging, and version regressions.

```powershell
dotnet format service/DeliveryNoteService.sln --no-restore --include-generated --verify-no-changes --include service/src/Application/Errors/ErrorCodes.cs service/src/Application/Recipients/CreateRecipient.cs service/src/Application/Recipients/CreateRecipientRequest.cs service/src/Application/Recipients/GetRecipient.cs service/src/Application/Recipients/GetRecipients.cs service/src/Application/Recipients/RecipientCountries.cs service/src/Application/Recipients/RecipientDetails.cs service/src/Domain/Models/Recipient.cs service/src/Domain/Models/RecipientLimits.cs service/src/Persistence/AppDbContext.cs service/src/Persistence/Migrations/AppDbContextModelSnapshot.cs service/src/Persistence/Migrations/20261108202717_AddRecipients.cs service/src/Persistence/Migrations/20261108202717_AddRecipients.Designer.cs service/src/WebApp/Program.cs service/src/WebApp/RestApi/RecipientEndpoints.cs service/test/ApiTests/RecipientCreationTests.cs service/test/ApiTests/RecipientDetailsTests.cs service/test/ApiTests/RecipientListingTests.cs service/test/ApiTests/RecipientConcurrencyTests.cs service/test/ApiTests/TestSuite/DemoServiceApi.cs service/test/ApiTests/TestSuite/DemoServiceApiTestBase.cs service/test/ApiTests/TestSuite/PostgresDemoServiceApiTestBase.cs service/test/ApiTests/TestSuite/RecipientConcurrency.cs service/test/ApiTests/TestSuite/RecipientRequests.cs src/Common.AspNetCore/ErrorHandling/ErrorHandlingMiddleware.cs src/Common.Testing/ApiTestBase.cs
```

Result: exit 0, no diagnostics, no changes. All 26 C# files changed after the
imported baseline were included, including generated migration artifacts.

Additional read-only checks:

- `git diff 96c2817..HEAD --check`: passed.
- Confirmed frozen head and comparison base; reviewed commit history and changes.
- Inspected generated route status sets: list 200/400, create 201/400/409, detail
  200/400/404. All six input fields are required in the schema, and the creation
  operation documents Location. OpenAPI freshness passed through the API suite.
- Counted 249 unique country codes with HU present; inspected normalization and
  rejection paths. No runtime country lookup or network dependency was added.
- Inspected migration Up/Down, designer, snapshot, and runtime mapping. The new
  migration creates only Recipients and its unique code index; Down removes only
  Recipients. The existing DemoModel unique index is retained. PostgreSQL
  initialization in the independently executed test applied the actual migrations.
- Inspected project/solution references and build-property placement. Referenced
  shared libraries are the root `src/Common.*` projects, not the untracked copies.
- Checked task status consistency: all four tasks done, story In review, plan
  active. The review does not rewrite those records.

No reviewed implementation, tests, generated contract, or tracking file was
modified. Only this new report is committed by the reviewer. No Markdown formatter
is configured; report whitespace and repository-relative references were checked.

## Code-guideline assessment

Applicable central policies: C# style, dependency injection, ASP.NET Core, .NET
testing, Web API endpoint documentation, and version endpoint contract. Repository
configuration takes precedence where it defines formatting. Manual inspection
covered the brace and multiline-parenthesis rules that Roslyn does not fully
enforce, as well as naming, responsibilities, duplication, and error handling.
Generated migration code was assessed as generated code, not rewritten.

The `EnforceCodeStyleInBuild` property is enabled through the applicable root,
service-source, and service-test props files. Program remains a composition root;
Recipient bindings are explicit and endpoint handlers delegate to use cases.
Validation precedes tracking/saving. Shared limit constants keep validation and
mapping aligned, and errors use named stable codes and the existing error format.

Skills used: `clean-code` for production/test readability, error handling, and
scoped refactoring assessment; `component-testing` for test-boundary and fixture
architecture review. Clean-code assessment: **10/10 within the reviewed change
scope**; no required correction was identified. Historical TDD evidence was
reviewed without changing tests or attempting to recreate earlier failures.

## Architecture assessment

The change follows the existing DemoEndpoints/use-case/repository layering. The
Recipient read projection consistently derives foreign status; database uniqueness
protects normalized codes beyond an application pre-check. Creation and viewing
are coherent vertical flows with no legacy backend or frontend dependency.

Test infrastructure was assessed across creation, detail, listing, concurrency,
and existing Demo/Paging consumers. Shared request builders and endpoint wrappers
own only capability/protocol mechanics. The existing application host owns
configuration-before-startup, messaging substitutes, clock, and HTTP client
lifecycle. SQLite remains the injected default. The protected instance fixture
selection is used consistently for database creation, service configuration,
clearing, null sorting, and scheduling. The thin PostgreSQL base initializes its
own fixture before the host and disposes the host before its container in a
finally block; no process-wide provider switch is introduced. The bounded-start
helper runs independent HTTP request scopes and does not assert internal calls.
Assertions observe responses and the final HTTP list, not database tables.

The main SQLite-backed component suite and the narrow real-PostgreSQL test provide
distinct confidence rather than duplicating the entire suite across providers.
No unnecessary parallel application design or production external adapter was
introduced. The additive migration and existing-index preservation avoid changing
demo data ownership or schema outside the story.

## Recorded deviations

- The existing shared paging implementation does not validate every empty or
  unpaged request and can overflow an integer offset. Recipient-local validation
  and a 64-bit offset guard meet the confirmed public contract without changing
  unrelated demo behavior. These implementation decisions are recorded in T003.
- The legacy test Unpaged helper sends page 0, conflicting with the confirmed
  positive-page rule. Recipient tests explicitly request page 1/limit 0; the
  shared helper remains unchanged. This is recorded in T003/T004.
- The imported runtime DemoModel mapping omitted an index already present in its
  migration snapshot. T002 restores the mapping so the generated Recipient
  migration preserves the existing index; this is recorded in its task and plan.
- The PostgreSQL test fixes the existing clock to a representable UTC instant,
  retaining exact response/read assertions while accounting for database timestamp
  precision. No production timestamp precision contract was changed.
- The central version-endpoint policy is the approval source for the existing
  shared version contract. This review records its preservation rather than
  treating it as an unapproved new story capability.

None of these is an unacknowledged product requirement or external-contract
deviation. No further architecture decision is needed to assess this story.

## Unverified items and remaining risks

- The implementation's default-parallel full-suite attempt stalled and was
  cancelled. Its cause is unestablished. This review independently verified the
  complete bounded sequential suite and did not repeat the known stalled mode.
- PostgreSQL timestamp storage has lower fractional-second precision than a
  general .NET timestamp. The deterministic provider test does not claim exact
  arbitrary-clock round-tripping beyond that database precision.
- Legacy data migration, authentication, update/delete, dashboard queries, and
  searching are explicitly deferred and were not verified as implemented.
- The excluded duplicate shared directories remain untracked. They were not
  included in the reviewed implementation and do not resolve referenced projects.
- Human review, human acceptance, pull-request creation, and merge are not part
  of this agent verdict. No required technical criterion remains unverified.
