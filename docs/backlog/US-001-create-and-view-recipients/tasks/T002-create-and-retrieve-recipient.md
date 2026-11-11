# T002: Create a recipient and retrieve its details

- Parent: [US-001](../README.md)
- Status: done
- Depends on: T001
- Increment: Skeleton

## Outcome

A caller can create a valid recipient and follow its Location header to retrieve
the persisted, normalized details without login.

## Scope

- Add the recipient model, request/read DTOs, EF mapping, normalized-code unique
  index, and additive PostgreSQL migration under `service/src`. Keep migration and
  model changes compatible with the existing SQLite test fixture.
- Add create and detail use cases through the established repository/unit-of-work
  and use-case registration patterns. Follow existing ID and UTC timestamp
  conventions; derive foreign status from normalized country instead of accepting
  it from the caller. Do not accept client-assigned ID or creation time as inputs.
- Implement all source-story input invariants in this first affected slice:
  required fields, trimming, length limits, allowed code characters, recognized
  uppercase two-letter countries, and normalized code uniqueness. Duplicate names
  remain valid. Preserve ZIP as text and add no postal or address verification.
- Add `RecipientEndpoints` following `DemoEndpoints`, with explicit parameter
  bindings and cancellation tokens. Register POST /api/recipients and
  GET /api/recipients/{id} from the existing composition root.
- Return the demo's envelopes, HTTP 201 and Location on creation, HTTP 200 on detail,
  HTTP 400 on invalid input, HTTP 404 on unknown positive ID, and HTTP 409 on code
  conflict. Use stable error codes and the existing error envelope. Update the
  actually referenced root `src/Common.AspNetCore` middleware's uniqueness mapping
  as needed, preserving existing error behavior for other exception types.
- Reuse `ApiTests` and its SQLite/messaging fixtures. Add only small recipient HTTP
  client/setup helpers, not a new test runner, repository mock, or persistence fake.
- Add complete OpenAPI metadata and refresh `service/openapi.json` in this task.

## Acceptance criteria

- [x] POST followed by GET of Location returns normalized recipient details, a
      server-generated ID, UTC creation time, and correct domestic/foreign status.
- [x] Blank, missing, overlength, invalid-country, and invalid-code inputs return
      400. Boundary-length inputs and textual ZIPs with leading zeros work.
- [x] Names and addresses retain internal text while surrounding whitespace is
      removed; a nonempty ZIP is not rejected for country-specific format.
- [x] Sequential code variants such as `ACME`, `acme`, and ` ACME ` conflict after
      normalization; two recipients with the same name and different codes work.
- [x] A database unique constraint protects normalized codes from the first
      implementation; a pre-check alone is insufficient.
- [x] Invalid creation followed by valid creation of the same candidate code
      succeeds, showing rejection did not reserve that code or persist a recipient.
- [x] Malformed/nonpositive IDs return 400, and unknown positive IDs return 404 in
      the agreed error format.
- [x] Both endpoints are anonymous and documented accurately in generated OpenAPI.

## Verification

Use TDD at the confirmed HTTP boundary, one failing behavior test followed by
minimal passing implementation. Name the new suite `RecipientCreationTests` and
`RecipientDetailsTests` so verification can target them. Run from the repository:

```powershell
dotnet test service/test/ApiTests/ApiTests.csproj --filter 'FullyQualifiedName~RecipientCreationTests|FullyQualifiedName~RecipientDetailsTests|FullyQualifiedName~OpenApiSpecTest'
```

Run focused existing HTTP error regressions affected by the middleware change.
Observe effects through responses and follow-up HTTP calls, not database queries.
Do not require T003's listing endpoint for this task's verification. Run applicable
formatters on changed files and verify formatting before committing. Record exact
commands, red/green evidence, and any environmental blockers.

## Progress

### Review-02 correction attempt 1 (2026-11-11)

Corrected review-02 findings F001, F002, F004 (remainder), F005 and F006 against
the story revised on 2026-11-11 (AC11, AC14, AC16, AC17, agreed test boundary) in
commit `fa3f912`. Task status is unchanged; no unfinished gate is marked complete.
A complete-coverage follow-up review is still required.

- F001: `CreateRecipientDto.MapTo` stores name, ZIP, city and address exactly as
  supplied; `Guard.ThrowIfMissing` semantics (null/empty rejected, whitespace
  accepted) and length limits apply to the supplied values. Code and country stay
  trimmed and uppercased; a code empty after trimming is rejected with
  `Recipient.InvalidCode` (400). DTO and endpoint descriptions updated.
- F002: Both the duplicate pre-check and the database `UniqueKeyConflictException`
  race in `CreateRecipient` return 400 `Recipient.AlreadyExists`; the shared
  middleware is unchanged. The 409 `Produces` metadata was removed. The PostgreSQL
  concurrency test now expects exactly 400 + `Recipient.AlreadyExists`; no SQLite
  recipient concurrency variant exists, so the race translation is not exercised
  by routine SQLite tests.
- F003: No code change. `RecipientDetailsTests` and the endpoint description
  already express 404 for zero and negative IDs.
- F004: Added DTO `Validate()` unit tests for whitespace-only codes (rejected) and
  whitespace-only name/ZIP/city/address (accepted); existing supplied-length tests
  retained. HTTP tests now expect preserved text and `Recipient.AlreadyExists`, and
  `ListRecipients_RejectedCreationDoesNotChangeList` restores the rejected-create
  no-save check.
- F005: `ErrorCodes.Recipient.AlreadyExists` is `_prefix + "AlreadyExists"`;
  unused `InvalidId` removed.
- F006: `dotnet format` applied to the 42 files with diagnostics in review-02's
  include scope plus every modified file (50 files).

TDD evidence: the new unit tests failed first (`CodeWhitespaceOnly`, 2 failures,
38 passed); the updated HTTP tests failed first (6 failures, 30 passed: preserved
text, `Recipient.InvalidCode` instead of `Recipient.AlreadyExists`, whitespace
code accepted). After implementation all passed.

Verification:
`dotnet test service/DeliveryNoteService.sln --filter "Database!=PostgreSQL" -v
minimal -- xUnit.ParallelizeTestCollections=false`: ApiTests 74/74 and UnitTests
182/182 passed, after formatting. `dotnet format service/DeliveryNoteService.sln
--no-restore --include-generated --verify-no-changes` exited 0 for both the
formatted set and review-02's full include scope (393 existing files). PostgreSQL
tests were not run (Testcontainers require explicit user approval).

Refactoring assessment (`clean-code`): duplicate-code exception creation is
extracted into one private factory used by both paths; the use case keeps its
validation, lookup, mapping, persistence and response sequence. Tests reuse the
existing `DtoFactory`, `RecipientRequests` and HTTP assertion helpers; no new
helpers were justified.

### Completed 2026-11-08

Implemented anonymous POST /api/recipients and GET /api/recipients/{id} using
injected use cases, the existing repository/unit of work, response envelopes,
clock, and error pipeline. Creation validates and normalizes before tracking the
entity, assigns server fields, and relies on a database unique index rather than
a race-prone pre-check. The country set is a fixed 249-code ISO alpha-2 set.

HTTP TDD evidence, one behavior at a time:

| Behavior | Observed red | Green combined count |
| --- | --- | --- |
| Create | POST returned 404 instead of 201 | 1 |
| Retrieve Location | GET returned 404 instead of 200 | 2 |
| Normalize input and derive foreign status | 2 country examples retained raw code whitespace | 4 |
| Required fields | 6 blank-field examples returned 201 | 10 |
| Maximum lengths | 5 overlength examples returned 201 | 15 |
| Code characters | 4 unsupported-code examples returned 201 | 19 |
| Country codes | 4 unrecognized-country examples returned 201 | 23 |
| Normalized code uniqueness | 3 duplicate-code examples returned 201 | 26 |
| Nonpositive ID | 2 invalid-ID examples returned 404 instead of 400 | 28 |

Coverage then confirmed existing behavior for malformed/unknown IDs, null/missing
fields, trimmed length boundaries, duplicate names, leading-zero ZIPs, internal
whitespace, ignored client-assigned server fields, and unverified postal/address
details. All 48 Recipient scenarios passed. The final post-format run also covered
the existing OpenAPI freshness test and related Demo/Paging regression cases:

```powershell
dotnet test service/test/ApiTests/ApiTests.csproj --filter 'FullyQualifiedName~RecipientCreationTests|FullyQualifiedName~RecipientDetailsTests|FullyQualifiedName~OpenApiSpecTest|FullyQualifiedName~DemoApiTests|FullyQualifiedName~PagingTests' --no-restore -v minimal
```

Result: 59 passed, 0 failed, 0 skipped. The generated OpenAPI document includes
both Recipient operations, six required request fields, all response fields,
400/404/409 responses, and the creation Location header. The pinned Swagger CLI
6.9.0 generated `service/openapi.json`; the freshness test verifies it against the
real host.

Migration `20261108202717_AddRecipients` and its designer/snapshot were scaffolded
with EF 8.0.10. Review discovered that the imported runtime model omitted the
already-persisted DemoModel UniqueId index, causing scaffolding to propose dropping
it. Restored that existing mapping and retained it in the target model/snapshot.
The new migration only creates Recipients and its unique Code index; rollback only
drops Recipients. SQLite HTTP tests apply the actual migrations. PostgreSQL
provider/concurrency verification remains T004 and is not claimed here.

Model/snapshot consistency also passed: from `service/src/Persistence`,
`../../../artifacts/tools/dotnet-ef.exe migrations has-pending-model-changes
--project Persistence.csproj` built successfully and reported that no model changes
had been made since the last migration (exit 0).

Scoped clean-code assessment covered production and test changes plus the related
Demo API/Paging consumers. Shared `RecipientLimits` avoids validation/mapping
drift; stable error codes are named; typed DbSet accessors avoid nullable
initialization state. The existing application host owns startup, per-test SQLite
databases, clock, messaging substitute, and disposal. Recipient request builders
and endpoint wrappers in TestSuite are shared by creation/detail and are available
for listing; they do not own lifecycle or shared mutable state. No new test runner
or internal mock was introduced. Configuration remains before host startup through
the existing fixture hooks. No wider baseline refactoring was warranted.

Ran `dotnet format service/DeliveryNoteService.sln --no-restore --include-generated
--include` with the 19 task C# paths, then the same scoped command with
`--verify-no-changes`; both final commands exited 0 without diagnostics. Naming
verification initially identified the required private-field underscore prefix,
which was fixed before the successful run. No Markdown formatter is configured;
tracking links/whitespace and `git diff --check` were verified separately.

Skills used: `tdd` (HTTP red/green cycles), `component-testing` (real application
boundary with existing deterministic infrastructure substitutes), and `clean-code`
(post-green production/test refactoring and shared-test responsibility review).
No loaded skill was unused. No task-specific blocker remains.

### Historical checkpoints

2026-11-08: Started the create/detail HTTP slice using the agreed public boundary.

Initial red: `dotnet test service/test/ApiTests/ApiTests.csproj --filter
'FullyQualifiedName~RecipientCreationTests' --no-restore -v minimal` compiled and
failed the first POST scenario with actual HTTP 404 instead of expected HTTP 201.
The first creation slice has been written. Its first verification attempts
overlapped and failed in the existing SQLite `seeded.db` initialization (file lock
and migration-history collision), so they do not establish a product result.
After adding the required Recipients table migration, the first scenario passed:
1 passed, 0 failed, 0 skipped (213 ms). The SQLite fixture applies migrations; a
model-only change had produced HTTP 500 because the table did not exist.

The next scenario, `RecipientDetailsTests.CreatedRecipient_CanBeRetrievedThroughLocation`,
has been written but not executed: its child escalation waited 328 seconds and
was aborted before a session was returned. The parent agent took sole ownership
of privileged test execution. No child command or test process remains active.
The expected next red is GET of Location returning 404 because no production GET
endpoint has been added. GET, normalization/validation, code uniqueness, complete
migration snapshot/metadata, OpenAPI refresh, formatting, and final verification
remain unfinished. Existing task checkboxes remain unchecked.
