# US-003: Review 02

- Review type: agent review
- Review date and time: 2026-11-11 14:58 (+01:00)
- Authoritative story: `docs/backlog/US-003-update-recipients.md`
- Plan: `docs/backlog/US-003-update-recipients/README.md`
- `AgenticEngineering` commit: `d19a8d75f1a6e59c2c5953d32aa0741e4fc0195f`
- Reviewed head: `feature/US-003-update-recipients` at `43c22bedca5e192b2faaffd946de2a799fccde6a`
- Comparison base: `master` at `2c80d390b49c26e26bf018425f949f469cc50ea5` (repository default branch; no pull request exists; merge base equals the base commit)
- Review mode: follow-up review
- Previous review report: `docs/backlog/US-003-update-recipients/review-01.md` (agent review, PASS, complete coverage, committed in `4c64684`)
- Previous reviewed head: `8ac80ffd85ac8d374058b7ba2a5a059ecace6f55` (ancestor of the current head: confirmed)
- Committed follow-up change range: `8ac80ff..43c22be` (`4c64684`, `8ab79e9`, `43c22be`)
- Included working-tree diff: none (working tree clean according to `git status`)
- Previous comparison-base commit: `2c80d390b49c26e26bf018425f949f469cc50ea5`; the base has not moved, so base movement has no effect
- Policy or evidence change: none identified
- Review coverage: complete
- Explicitly included uncommitted changes: none
- Final verdict: **FAIL**

## 1. Verdict and merge readiness

**FAIL.** Five blocking findings remain. This review treats the developer's
simplifications in `8ab79e9` and `43c22be` as intentional maintainability feedback, as
`CLAUDE.md` requires. It does not ask for the removed code to be restored by default.
It does find that the branch no longer matches what was agreed:

- The agreed US-003 test boundary is no longer covered (F003). The production
  behavior for AC6 to AC9 is still correct. The reviewer reran the removed tests
  against the current production code in a scratch copy.
- The agreed US-001 contract changed without a record: a concurrent duplicate code
  (F004) and paging and sorting validation (F006).
- The PostgreSQL concurrency test now expects an error code that a duplicate cannot
  produce. It rejects the response the service actually returns when its pre-check
  catches the duplicate (F005).
- Formatter verification fails on three test files (F007).

Each of these can be closed either by reconciling the requirement (the developer
records the accepted change in the story) or by a small code or test change. The
required correction for each finding names both routes where both apply.

Commits in the follow-up range:

| Commit | Subject | Content | Classification |
|---|---|---|---|
| `4c64684` | US-003: Record review 01 | Adds only `docs/backlog/US-003-update-recipients/review-01.md` | Administrative; no implementation or requirement change |
| `8ab79e9` | Removed page reques testing | Removes `ValidateListQuery` from `RecipientEndpoints` (paging and sort validation for `GET /api/recipients`), reformats the route chains, and removes two listing test theories (8 cases) | Implementation change, reviewed |
| `43c22be` | Human review | Removes the `UniqueKeyConflictException` translation from `CreateRecipient`; moves the recipient DTOs to `Recipients.Data`; makes `CreateRecipientDto` derive from `UpdateRecipientDto`; renames `NoteEndpoints` to `DeliveryNoteEndpoints`; removes unused usings from `VersionEndpoints`; removes 6 of 7 `RecipientUpdateTests`, 6 listing malformed-query cases, and 3 `CreateRecipientDtoTests` cases; changes `RecipientConcurrencyTests` | Implementation change, reviewed |

Previous-item outcomes:

| Item | Type | First reported in | Previous severity | Outcome | Evidence |
|---|---|---|---|---|---|
| F001: PUT not-found tests omit a negative ID | Finding | `review-01.md` | Non-blocking | Unresolved | No correction was attempted. `43c22be` removed `UpdateRecipient_UnknownId_ReturnsNotFound` entirely, so PUT now has no not-found test at all. The reviewer's scratch run confirms that `-1`, `0`, and `123456` return 404 `Recipient.NotFound`. The coverage gap is now part of F003. |
| F002: Code-supplied test does not also assert the creation time | Finding | `review-01.md` | Non-blocking | Unresolved | No correction was attempted. `43c22be` removed `UpdateRecipient_IgnoresSuppliedCode`, so a supplied `Code` is no longer tested at all. The gap is now part of F003. |

Correction-attempt history: neither previous item received a correction attempt.

## 2. Blocking findings

### F003: The agreed US-003 HTTP test boundary is no longer covered

- Location: `service/test/ApiTests/RecipientUpdateTests.cs` (only `UpdateRecipient_ReplacesDetailsAndKeepsCodeAndCreationTime` remains).
- Rule: the story's "Agreed test boundary" lists these HTTP tests: "a rejected update
  leaving the recipient unchanged, 404 for an unknown ID, code and creation time
  preserved (including when a `Code` is supplied), and one test showing a delivery
  note with the updated recipient details (AC9)". `CLAUDE.md` says to keep explicitly
  agreed requirements until the developer authorizes a change, and to reconcile any
  requirement that a simplification changes. A missing required test is a blocking
  finding under the review severity rules.
- Evidence: `43c22be` removed `UpdateRecipient_IsShownOnExistingDeliveryNote` (AC9),
  `UpdateRecipient_IgnoresSuppliedCode` (AC6), and
  `UpdateRecipient_RejectsInvalidDetailsWithoutChangingRecipient` (AC7). It also removed
  `UpdateRecipient_UnknownId_ReturnsNotFound` and
  `UpdateRecipient_MalformedId_ReturnsBadRequest` (AC8). No other test in the
  repository exercises PUT. After this change:
  - Nothing tests AC9 (live recipient details on a delivery note after an update).
  - Nothing tests whether a rejected update leaves the recipient unchanged.
  - Nothing tests PUT 404 or a supplied `Code`.

  The production behavior is still correct. The reviewer restored the eight removed
  test cases from `8ac80ff` (adding a `-1` ID) into a scratch copy of the reviewed head
  and ran them against the unchanged production code. All passed (section 5).
  `service/test/CLAUDE.md` asks for a minimal API test set. The story's boundary was
  agreed under that same instruction, though, and no revision is recorded.
- Required correction (choose one):
  - Restore a minimal set: one AC9 delivery-note test, one rejected-update-unchanged
    test, and one 404 test. Optionally fold the supplied `Code` into an existing
    request.
  - Have the developer revise the story's "Agreed test boundary" to record the
    reduced set and accept the uncovered criteria.

### F004: The duplicate-code race now returns 409 instead of the agreed 400 `Recipient.AlreadyExists` (US-001 contract change)

- Location: `service/src/Core/UseCases/Recipients/CreateRecipient.cs`, lines 31-38 (`43c22be`).
- Rule: US-001 AC11 (`docs/backlog/US-001-create-and-view-recipients.md`) says the
  response is "HTTP 400 ... `Recipient.AlreadyExists`, for both the sequential check
  and a concurrent database uniqueness conflict. (Changed from 409 on 2026-11-11.)"
  `CLAUDE.md` says to "Add translation only for a concrete agreed contract that
  requires it". AC11 is that kind of contract. Changing the public API contract of an
  earlier story without approval is a blocking contract deviation.
- Evidence: the `try`/`catch (UniqueKeyConflictException)` translation was removed.
  In the reviewer's scratch probe on SQLite, the pre-check was bypassed only for a
  marker code to simulate losing the race. That path returned
  `409 {"errorCode":"error:DNC::UniqueKeyConflict"}`, because
  `src/Common.AspNetCore/ErrorHandling/ErrorHandlingMiddleware.cs` maps
  `UniqueKeyConflictException` to 409. The sequential path still returns
  `400 {"errorCode":"Recipient.AlreadyExists"}`. One rule, a duplicate code, now has
  two outcomes, and which one a client sees depends on timing. US-001 and US-003 do
  not record this change.
- Required correction (choose one):
  - Restore the translation (a single `catch` around `SaveChangesAsync`).
  - Have the developer revise US-001 AC11 and its agreed decisions to accept 409
    `UniqueKeyConflict` for the concurrent case.

  Either way, align `RecipientConcurrencyTests` (F005).

### F005: `RecipientConcurrencyTests` accepts `Recipient.InvalidCode`, which a duplicate cannot produce

- Location: `service/test/ApiTests/RecipientConcurrencyTests.cs`, lines 24-28.
- Rule: tests must cover the changed behavior correctly. US-001 AC11 requires
  concurrent duplicate rejection to be tested.
- Evidence: `"Recipient.InvalidCode"` is a real error code (`ErrorCodes.Recipient.InvalidCode`).
  Only `CreateRecipientDto.Validate()` raises it, and only for a code that is empty after
  trimming or that contains characters outside ASCII letters, digits, `-` and `_`. The
  test sends `" acme-01 "` and `"ACME-01"`, both valid, so neither request can produce
  it. When the second request is rejected, there are two possible outcomes:
  - The pre-check catches the duplicate: (400, `Recipient.AlreadyExists`). The test
    does not accept this outcome.
  - The database conflict fires: (409, `error:DNC::UniqueKeyConflict`). The test
    accepts this outcome.

  The test therefore fails whenever the first insert commits before the second
  request's pre-check, which makes it timing-dependent. The value looks like a mistake
  for `Recipient.AlreadyExists`. It is also a string literal rather than the
  `ErrorCodes` constant used elsewhere. The test is tagged `Database=PostgreSQL` and was
  not run, as the verification rules require. This conclusion comes from code analysis
  plus the SQLite probes of both paths.
- Required correction: if F004 keeps 409, accept `(BadRequest, Recipient.AlreadyExists)`
  in place of `(BadRequest, "Recipient.InvalidCode")`. If F004 restores the
  translation, assert the single 400 `Recipient.AlreadyExists` outcome again.

### F006: List paging and sorting validation was narrowed without a recorded US-001 revision

- Location: `service/src/WebApp/RestApi/RecipientEndpoints.cs` (`8ab79e9` removed `ValidateListQuery`); `service/test/ApiTests/RecipientListingTests.cs` (`8ab79e9` and `43c22be` removed 14 cases).
- Rule: US-001 AC16 says "invalid paging or sorting return HTTP 400", and AC15 says
  "Use the demo's paging validation". Agreed requirements stay in force until the
  developer authorizes a change (`CLAUDE.md`). Changing the public API contract of an
  earlier story without approval is a blocking contract deviation.
- Evidence (reviewer's SQLite probes at the reviewed head):
  - `?Page=0` and `?Limit=-1` return 200 on an empty collection. With at least one
    recipient they still return 400, because the shared `PagedResultHelpers` validates
    the page request.
  - `?SortBy=99` and `?SortDir=99` return 200 whether or not the collection is empty.
    For `SortBy=99`, `SortMap.Get` falls back to a no-op ordering, so page contents are
    not deterministically ordered.
  - Malformed values (`?Page=abc`, `?SortBy=Country`) still return 400
    `InvalidHttpRequest` through binding.

  The commit subject and the repository rule "Never validate `PageRequest` or `Sorting`
  in use case classes for GET endpoints" show that the developer intends this
  simplification. However, US-001 still states the stricter contract.
- Required correction: the review does not require the removed validation to be
  restored. Have the developer record the accepted narrowing in US-001 (AC16): undefined
  numeric sort values and invalid paging on an empty collection are no longer rejected.
  Alternatively, restore the endpoint validation.

### F007: Formatter verification fails on three changed test files

- Location: `service/test/ApiTests/DtoFactories/DtoFactory.cs` (31), `service/test/ApiTests/RecipientUpdateTests.cs` (44), `service/test/UnitTests/DtoValidation/UpdateRecipientDtoTests.cs` (57).
- Rule: `AGENTS.md` requires formatter verification to pass. `service/.editorconfig` sets
  `insert_final_newline = false`.
- Evidence: in a clean export of the reviewed head, `dotnet format ... --verify-no-changes`
  reports `FINALNEWLINE: Fix final newline` for these three files, and `43c22be` added
  each of those trailing newlines. In the local checkout, the same command also reports
  `ENDOFLINE` for `RecipientUpdateTests.cs` and `UpdateRecipientDtoTests.cs`. Those files
  have LF line endings only in the working tree. The committed content does not have
  this problem, and a fresh checkout (`core.autocrlf=true`) does not reproduce it.
- Required correction: run `dotnet format` on the three files, or remove the trailing
  newlines.

## 3. Non-blocking findings

### F001: PUT not-found tests omit a negative ID (carried from `review-01.md`)

Unresolved. PUT now has no not-found test at all (see F003). The behavior was verified
with `-1` in the reviewer's scratch run. This finding closes with F003 if the restored
or revised boundary covers the not-found case.

### F002: Code-supplied test does not also assert the creation time (carried from `review-01.md`)

Unresolved. The supplied-`Code` test was removed (see F003). This finding closes with
F003.

### F008: `CreateRecipientDto.MapTo` duplicates the base mapping

- Location: `service/src/Core/UseCases/Recipients/Data/CreateRecipientDto.cs`, lines 31-42.
- Evidence: `CreateRecipientDto` now derives from `UpdateRecipientDto` and reuses the
  base `Validate()`. This removes the duplicated `Guard` calls noted in review 01, and it
  is a sound simplification. However, `MapTo(model, timeService)` still repeats the five
  assignments that `UpdateRecipientDto.MapTo(model)` already performs.

  The inheritance has two minor side effects:
  - Code errors are now reported before missing or too-long errors on other fields.
    Error codes are unchanged.
  - A `CreateRecipientDto` can be passed wherever an `UpdateRecipientDto` is expected.

  The generated OpenAPI schemas are unaffected: `CreateRecipientDto` still has six
  properties and `UpdateRecipientDto` has five.
- Suggested correction (optional): call `MapTo(model)` from the create mapping, then
  set `Code` and `CreatedAt`.

### F009: The whitespace-only code rejection is no longer unit-tested

- Location: `service/test/UnitTests/DtoValidation/CreateRecipientDtoTests.cs`.
- Evidence: `43c22be` removed `CodeWhitespaceOnly` and `WhitespaceOnlyTextIsAccepted`.
  Whitespace-only text acceptance is still covered for creation, through the shared base
  validation tested by `UpdateRecipientDtoTests.WhitespaceOnlyTextAndUnnormalizedCountryAreAccepted`.
  US-001 AC14 and AC17 say a code that is empty after trimming is rejected. That rule is
  no longer tested, although its code did not change. `service/test/CLAUDE.md` allows
  representative cases.
- Suggested correction (optional): add `" "` as an `InlineData` to `CodeInvalidCharacters`,
  or accept the gap.

## 4. Requirement traceability

Statuses for unchanged production paths are inherited from `review-01.md` where noted. The
reviewer reran evidence only where this table says so.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
|---|---|---|---|---|
| AC1: Valid update returns 200 with `Envelope<RecipientDetailsDto>` | `UpdateRecipient.cs` unchanged except the `using` for `Recipients.Data`; the `MapPut` handler was only reformatted | `RecipientUpdateTests.UpdateRecipient_ReplacesDetailsAndKeepsCodeAndCreationTime`; rerun passed | Met | None |
| AC2: GET-by-ID and listing return new details immediately | Unchanged save path; no caching | Same test asserts GET-by-ID and list; rerun passed | Met | None |
| AC3: Full replacement of the five required fields | `UpdateRecipientDto` unchanged except namespace and `virtual Validate()` | `UpdateRecipientDtoTests` (`NameMissing`, `MissingProperties`) and the API happy path; rerun passed | Met | None |
| AC4: US-001 field rules unchanged | `UpdateRecipientDto.Validate()` unchanged; `CreateRecipientDto` now reuses it through inheritance, so creation and update share one rule set | `UpdateRecipientDtoTests` (`AddressTooLong`, whitespace and unnormalized country, `CountryUnrecognized`); `CreateRecipientDtoTests`; API happy path; rerun passed | Met | F008, F009 |
| AC5: Foreign status re-derived; cannot be supplied | Unchanged | API happy path (`HU` to `US`, `IsForeign = true`); rerun passed | Met | None |
| AC6: Code, ID, creation time unchanged; supplied `Code` ignored | `UpdateRecipientDto` has no `Code`, `Id`, or `CreatedAt`; recipient DTOs do not disallow unmapped members | Repository: happy path asserts code, ID, and creation time without a supplied `Code`. Supplied `Code` verified only by the reviewer's scratch rerun of the removed `UpdateRecipient_IgnoresSuppliedCode` (passed) | Met | F003, F002 |
| AC7: Invalid details rejected with 400 and stable codes; recipient unchanged | `dto.Validate()` before lookup and save (unchanged) | Repository: DTO unit tests only. The unchanged-on-reject API check was verified only by the reviewer's scratch rerun of the removed `UpdateRecipient_RejectsInvalidDetailsWithoutChangingRecipient` (passed) | Met | F003 |
| AC8: Malformed ID returns 400; unknown ID (including 0 and negative) returns 404 `Recipient.NotFound` | `[FromRoute] int id`; `NotFoundException` on missing lookup (unchanged) | Repository: none for PUT. Reviewer's scratch rerun: not-found for `0`, `-1`, `123456` and the malformed ID all passed | Met | F003, F001 |
| AC9: A delivery note shows updated recipient details | No production change (US-002 live relationship) | Repository: none. Reviewer's scratch rerun of the removed `UpdateRecipient_IsShownOnExistingDeliveryNote` passed | Met | F003 |
| AC10: OpenAPI describes the endpoint and its status codes under the reduced contract | Endpoint metadata unchanged | Regenerated document inspected: `UpdateRecipient`, responses 200, 400, 404, required body `UpdateRecipientDto` (5 properties); `CreateRecipientDto` still 6 properties | Met | None |
| Scope: API, `UpdateRecipient` use case, persistence, `CreateRecipient` patterns and logging | Unchanged (inherited from `review-01.md`) | Inherited; not rerun beyond the tests above | Met | None |
| Agreed decisions: no body `Id`, last write wins, no PATCH | Unchanged (inherited) | Inherited code inspection | Met | None |
| Agreed test boundary | See AC6 to AC9 | Only the successful-update test remains | Not met | F003 |
| Regression: US-001 AC11 duplicate code returns 400 `Recipient.AlreadyExists` for sequential and concurrent cases | Translation removed in `CreateRecipient.cs` | Scratch probe: concurrent path returns 409 `error:DNC::UniqueKeyConflict`; PostgreSQL test changed and incorrect | Not met | F004, F005 |
| Regression: US-001 AC16 invalid paging or sorting returns 400 | `ValidateListQuery` removed | Scratch probes: `Page=0` and `Limit=-1` return 200 when empty; `SortBy=99` and `SortDir=99` return 200 | Not met | F006 |

## 5. Verification performed

The repository commands below ran from `service/` against the reviewed head with a clean
working tree. Scratch checks ran in a `git archive` export of the reviewed head under
`<temporary-directory>`. Their extra files and edits are described below and were never
added to the repository.

| Command | Result |
|---|---|
| `dotnet build DeliveryNoteService.sln` | Build succeeded, 0 warnings, 0 errors |
| `dotnet format DeliveryNoteService.sln --verify-no-changes --include <the 24 C# files changed in master..HEAD>` | Exit code 2. In the clean export: `FINALNEWLINE` in `test/ApiTests/DtoFactories/DtoFactory.cs(31,2)`, `test/ApiTests/RecipientUpdateTests.cs(44,2)`, `test/UnitTests/DtoValidation/UpdateRecipientDtoTests.cs(57,2)`. The local checkout also reports `ENDOFLINE` for the latter two (LF in the working tree only; see F007). |
| `dotnet test DeliveryNoteService.sln --no-build --filter "Database!=PostgreSQL"` | ApiTests (`ApiTests.dll`): 61 passed, 0 failed (81 at review 01; 20 cases removed). UnitTests (`Medusa.Enap.Common.UnitTests.dll`, the solution's only unit-test project, which also holds the Common unit tests): 187 passed, 0 failed (190 at review 01; 3 removed). The solution has no separate Common unit-test project; `Common.Testing` is a helper library without tests. |
| Scratch: removed `RecipientUpdateTests` from `8ac80ff`, namespace adjusted and `-1` added to the not-found theory, run with `dotnet test test/ApiTests/ApiTests.csproj --filter "FullyQualifiedName~ReviewRestoredUpdateTests"` | 8 of 8 passed (happy path, AC9 note, supplied `Code`, rejected update unchanged, not-found `0`, `-1`, `123456`, malformed ID) |
| Scratch: in `CreateRecipient`, pre-check bypassed only for codes starting with `RACE`; probes post a duplicate code twice (SQLite) | Database-conflict path: `409 {"errorCode":"error:DNC::UniqueKeyConflict"}`. Sequential pre-check path (unmodified code): `400 {"errorCode":"Recipient.AlreadyExists"}` |
| Scratch: listing probes, empty and then non-empty collection | `?Page=0`: 200, then 400. `?Limit=-1`: 200, then 400. `?SortBy=99`: 200, 200. `?SortDir=99`: 200, 200. `?Page=abc` and `?SortBy=Country`: 400 `error:DNC::InvalidHttpRequest` |
| `dotnet tool restore`, then with `ASPNETCORE_ENVIRONMENT=Development`: `dotnet swagger tofile --output <temporary-directory>/openapi-us003-r2.json src/WebApp/bin/Debug/net8.0/DeliveryNoteService.WebApp.dll v1` | Document written. PUT `UpdateRecipient` has responses 200, 400, 404; `UpdateRecipientDto` has 5 properties; `CreateRecipientDto` has 6 (flattened, no `allOf`); POST has 201, 400 |

Not run: PostgreSQL API tests (`Database=PostgreSQL`, Testcontainers), including
`RecipientConcurrencyTests`. The task's verification rules and `service/test/CLAUDE.md` do
not allow them without explicit approval. F005 rests on code analysis and the SQLite
probes of both rejection paths.

## 6. Code-guideline assessment

Applied guidance:

- Repository: `CLAUDE.md` (proportionality, use case, DTO, and error-code rules) and
  `service/test/CLAUDE.md`.
- .NET policy: C# style, .NET testing, ASP.NET Core.
- Web API policy: OpenAPI endpoint metadata, version endpoint.
- `clean-code`.

The repository's proportionality rules take precedence over generic advice. The
developer's simplifications were assessed as maintainability feedback.

- Simplifications in the follow-up range:
  - Moving the recipient DTOs into `Recipients.Data` is a reasonable organization
    choice, applied consistently to every consumer.
  - Deriving `CreateRecipientDto` from `UpdateRecipientDto` removes the duplicated field
    validation that review 01 accepted as a tradeoff. The `Guard` field names are
    preserved because the base class uses the same property names. A minor residual
    duplication remains in `MapTo` (F008).
  - `CreateRecipient` is now a cleaner validation, lookup, mapping, and persistence
    sequence. Dropping the translation conflicts with an agreed contract, though (F004).
  - Removing unused usings from `VersionEndpoints` is safe: the Web SDK supplies them as
    implicit usings, and the build passes.
  - Renaming `NoteEndpoints` to `DeliveryNoteEndpoints` matches the domain term. Its
    extension method is still `MapNoteRoutes`. This is a minor naming inconsistency, not
    raised as a finding.
- Use case and DTO conventions: `UpdateRecipient` still follows the `CreateRecipient`
  logging and step structure. Both DTOs keep explicit `init` properties, preserve
  supplied strings, and use block-bodied `MapTo` methods that return the same instance.
  `CreateRecipient` now throws `DataValidationException(ErrorCodes.Recipient.AlreadyExists)`
  without a custom message. The message is not serialized in the error response, so this
  has no contract effect.
- Tests: the remaining tests follow `service/test/CLAUDE.md` (DTO instances, `with`
  expressions, Arrange-Act-Assert). The reductions follow the repository's "smallest
  useful set" preference. They remove coverage that the story's agreed boundary
  explicitly requires, however (F003). The concurrency test change is incorrect (F005).
  No new test infrastructure was introduced, so the extracted-infrastructure check does
  not apply.
- Formatting: the build enforces code style and passes, but formatter verification fails
  (F007).
- ASP.NET Core and OpenAPI: route metadata is unchanged apart from formatting. The
  endpoints still declare 400 for the list operation, which is still reachable through
  binding and the shared page validation.

## 7. Architecture assessment

- Layering is unchanged: the endpoints are in `WebApp/RestApi`, the use cases in
  `Core/UseCases/Recipients`, and the DTOs in the new `Core/UseCases/Recipients/Data`
  namespace. Notes DTOs remain in `Core/UseCases/Notes`. That asymmetry is acceptable;
  no repository rule names a DTO namespace.
- Data integrity: the database unique index on `Recipient.Code` still prevents duplicates
  under concurrency. Only the reported status and error code changed (F004).
- External contracts:
  - `PUT /api/recipients/{id}` and its schema are unchanged since review 01.
  - The `POST /api/recipients` concurrent-duplicate response (F004) and the
    `GET /api/recipients` paging and sorting validation (F006) changed. Both are service
    API contracts agreed in US-001. Neither change is acknowledged in any story or plan
    record, so both are unapproved external-contract deviations until the developer
    records them.
- No unnecessary parallel design was introduced.

## 8. Recorded deviations

Story deviations from US-003: none in production behavior. The deviation from the
agreed test boundary is reported as F003.

Deviations from US-001 agreed contracts introduced on this branch, not acknowledged:

- A concurrent duplicate code returns 409 `error:DNC::UniqueKeyConflict` instead of 400
  `Recipient.AlreadyExists` (F004).
- Invalid paging on an empty collection and undefined numeric sort values return 200
  instead of 400 (F006).

Plan and task-level differences carried from review 01 remain as recorded there. In
addition, after `43c22be` the T001 and T002 verification evidence (removed tests) no
longer exists in the repository. The task files still describe it.

Repository-required additions: none beyond the story.

## 9. Unverified items and remaining risks

- PostgreSQL API tests were not run, as the verification rules require. With the current
  assertion, `RecipientConcurrencyTests` is expected to fail whenever the duplicate is
  caught by the pre-check (F005).
- Manual verification through Swagger UI or a running service was not performed. It is
  part of human acceptance.
