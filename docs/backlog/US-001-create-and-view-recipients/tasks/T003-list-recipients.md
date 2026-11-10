# T003: List recipients with paging and sorting

- Parent: [US-001](../README.md)
- Status: done
- Depends on: T002
- Increment: Extension

## Outcome

A caller can discover created recipients through a deterministic, paged list.

## Scope

- Add the list use case and GET /api/recipients in the existing recipient route
  module. Reuse `PageRequestQueryParam`, sorting abstractions, repository paging,
  and `PagedEnvelope<T>` from the actual referenced shared projects.
- Return the agreed read fields, default to page 1/limit 10 and ID ascending, allow
  ID/name ordering, and append ID as a deterministic name-sort tie-breaker. Follow
  existing direction and paging query conventions rather than adding a new API.
- Keep the demo's limit=0 behavior, paging validation, and stable 400 errors for
  invalid paging/sorting. No search or cache is introduced.
- Extend recipient HTTP helpers as necessary. Arrange test recipients through
  POST; assert only HTTP-visible results.
- Add complete endpoint metadata and refresh `service/openapi.json` in this task.

## Acceptance criteria

- [x] An empty database returns HTTP 200 with an empty paged collection.
- [x] A successful POST is immediately visible in the list without a refresh flag.
- [x] List items contain normalized details and server-assigned fields consistent
      with creation/detail responses.
- [x] Default page/limit/order, explicit paging, supported sort directions, name
      ties, and limit=0 return the expected results and demo pagination metadata.
- [x] Invalid query values return 400 in the demo's error format with stable codes.
- [x] Invalid creation has no effect on a subsequent list; no direct database
      assertion is used to prove absence.
- [x] The generated OpenAPI document accurately describes all three Recipient
      operations, query parameters, schemas, successful statuses, and errors.

## Verification

Use one HTTP red/green cycle at a time. Add `RecipientListingTests` with literal
expected examples covering empty lists, multiple pages, repeated names, sorting,
unpaged results, and invalid queries. Run:

```powershell
dotnet test service/test/ApiTests/ApiTests.csproj --filter 'FullyQualifiedName~Recipient|FullyQualifiedName~OpenApiSpecTest'
```

Then run the complete existing ApiTests suite once for shared paging/host
regressions. Do not require T004's PostgreSQL harness to pass this task's tests.
Format changed files and verify formatting. Record actual test commands and
red/green evidence.

## Progress

2026-11-08: Started the listing slice at the agreed HTTP boundary, reusing the
existing Recipient request builders, endpoint wrapper, host, and datastore fixtures.

### Implemented and verified

Added `GetRecipients` and anonymous GET /api/recipients with the demo's
`PageRequestQueryParam`, typed sorting, `SortMap`, repository paging, and
`PagedEnvelope<RecipientDetails>`. Default ordering is ID ascending; name sorting
uses ID ascending for ties in either name direction. No cache or search was added.
All test records are arranged through POST and effects observed through HTTP.

TDD checkpoints, each observed before its implementation:

- Empty-page scenario failed with no list operation, then passed after the route
  and use case were added.
- ID-descending order failed (FIRST, SECOND instead of SECOND, FIRST), then all
  six initial listing scenarios passed with typed sorting.
- Both name-direction examples failed, then eight scenarios passed after the
  stable name/ID ordering was added.
- Four invalid paging examples on empty/unpaged requests failed, then twelve
  scenarios passed with validation before repository paging.
- Four undefined numeric sorting examples failed, then sixteen scenarios passed
  after explicit enum validation.
- Large positive page regression failed because int offset multiplication wrapped
  and returned the first recipient. A local 64-bit offset check now returns an
  empty page with its actual total count when the offset exceeds integer range;
  no new public page limit or shared-helper behavior was introduced.

Additional coverage verifies immediate normalized details, default and explicit
pages, unpaged results, total counts, malformed/overflowing integer query values,
invalid POST absence, beyond-last pages, and direction without a sort field.
There are 26 listing scenarios and 74 Recipient scenarios in total.

The shared paging helper skips validation for empty datasets and limit=0, so this
use case validates Page >= 1 and Limit >= 0 before using it. The existing demo
`QS().Unpaged()` helper sends page=0; Recipient requests use page=1/limit=0 or
limit=0 alone. Shared/demo helpers remain unchanged. Invalid typed values use
the existing HTTP binding error code; undefined enum values use InvalidSortField;
invalid page/limit values use InvalidPageRequest.

### Refactoring and test infrastructure assessment

Scoped clean-code assessment: 10/10 for the changed slice after extracting request
validation and the named name sorter, and separating effective default sorting
from repository orchestration. No wider shared paging rewrite was justified.
Reviewed the existing creation/detail tests, Demo paging tests, TestSuite request
builder and endpoint wrappers for reuse. They already remove serialization and
HTTP mechanics while retaining scenario inputs in each test; no new helper that
merely renames a one-line API call was needed. The existing application fixture
owns per-test host/database/clock/messaging setup and disposal; configuration
remains before startup. Tests share reusable code, not mutable state or lifecycle.

### Final verification

From the repository root, after the last source edit:

```powershell
dotnet format service/DeliveryNoteService.sln --no-restore --include service/src/Application/Recipients/GetRecipients.cs service/src/WebApp/RestApi/RecipientEndpoints.cs service/test/ApiTests/RecipientListingTests.cs
dotnet format service/DeliveryNoteService.sln --no-restore --verify-no-changes --include service/src/Application/Recipients/GetRecipients.cs service/src/WebApp/RestApi/RecipientEndpoints.cs service/test/ApiTests/RecipientListingTests.cs
dotnet test service/test/ApiTests/ApiTests.csproj --filter 'FullyQualifiedName~Recipient|FullyQualifiedName~OpenApiSpecTest' --no-restore -v quiet
dotnet test service/test/ApiTests/ApiTests.csproj --no-restore -v minimal --blame-hang --blame-hang-timeout 60s -- xUnit.ParallelizeTestCollections=false
```

Formatting and verification both exited 0 without diagnostics. The focused
Recipient/OpenAPI run passed 75/75, with no failures or skips. The complete API
suite passed 114/114, with no failures or skips, in 7 seconds using the explicit
sequential xUnit option. The blame collector reported all tests finished and no
sequence file was needed. The supported runner option was checked against the
[official xUnit RunSettings documentation](https://xunit.net/docs/config-runsettings).

The first default-parallel full-suite attempt stalled during host/scheduler
activity and was cancelled through its owned test session. Its outcome is not
claimed as passing, nor is its root cause established. The focused 75-test suite
passed with default parallelization. No test-runner defaults or unrelated host
code were changed to obtain the complete sequential verification.

The parent agent regenerated `service/openapi.json` using the pinned Swagger CLI
6.9.0 after the listing metadata build; the final OpenAPI freshness test passed.
The operation documents parameters, paging defaults, deterministic sorting,
success/error shapes, and anonymous access. No Markdown/JSON formatter is
configured; generated JSON formatting was retained and tracking links/whitespace
were checked separately.

Skills used: `tdd` for observed HTTP red/green cycles; `component-testing` for the
real application boundary and existing infrastructure substitutes; `clean-code`
for the post-green scoped production/test assessment. No loaded skill was unused.
T003 is complete; T004's PostgreSQL concurrency verification is next.
