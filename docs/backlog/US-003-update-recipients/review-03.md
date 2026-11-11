# US-003: Review 03

- Review type: agent review
- Review date and time: 2026-11-11 15:16 (+01:00)
- Authoritative story: `docs/backlog/US-003-update-recipients.md`
- Plan: `docs/backlog/US-003-update-recipients/README.md`
- `AgenticEngineering` commit: `d19a8d75f1a6e59c2c5953d32aa0741e4fc0195f`
- Reviewed head: `feature/US-003-update-recipients` at `4cf7419ac1607a3ec5125ab1308a8b540e3cfa3c`
- Comparison base: `master` at `2c80d390b49c26e26bf018425f949f469cc50ea5` (repository default branch; no pull request exists; merge base equals the base commit)
- Review mode: follow-up review
- Previous review report: `docs/backlog/US-003-update-recipients/review-02.md` (agent review, FAIL, complete coverage, committed in `8c44f4e`). It is the latest committed agent review. Its findings carry the open items from `review-01.md` (F001, F002), so both reports' items are rechecked here.
- Previous reviewed head: `43c22bedca5e192b2faaffd946de2a799fccde6a` (ancestor of the current head: confirmed with `git merge-base --is-ancestor`)
- Committed follow-up change range: `43c22be..4cf7419` (`8c44f4e`, `4cf7419`)
- Included working-tree diff: none (working tree clean according to `git status`)
- Previous comparison-base commit: `2c80d390b49c26e26bf018425f949f469cc50ea5`; the base has not moved, so base movement has no effect
- Policy or evidence change: none identified
- Review coverage: complete
- Explicitly included uncommitted changes: none
- Final verdict: **PASS**

## 1. Verdict and merge readiness

**PASS.** Every blocking finding from review 02 (F003 to F007) is resolved. Three were
resolved in code or tests (F003, F004, F005), one by a recorded requirement revision
(F006), and one by a formatting correction (F007). The follow-up change introduces no
new finding. The non-blocking findings F008 and F009 remain open; they do not prevent
`PASS`. Human review and manual verification are still required before the story is
`Done`.

Commits in the follow-up range:

| Commit | Subject | Content | Classification |
|---|---|---|---|
| `8c44f4e` | US-003: Record review 02 | Adds only `docs/backlog/US-003-update-recipients/review-02.md` | Administrative; no implementation or requirement change |
| `4cf7419` | US-003: Fix review 02 findings | Restores the `UniqueKeyConflictException` translation in `CreateRecipient`; restores four `RecipientUpdateTests` (6 cases); changes the `RecipientConcurrencyTests` assertion; removes final newlines from three test files; adds a narrowing note to US-001 AC16 | Implementation, test, and requirement change, reviewed |

Previous-item outcomes:

| Item | Type | First reported in | Previous severity | Outcome | Evidence |
|---|---|---|---|---|---|
| F001: PUT not-found tests omit a negative ID | Finding | `review-01.md` | Non-blocking | Resolved | `RecipientUpdateTests.UpdateRecipient_UnknownId_ReturnsNotFound` covers `-1`, `0`, and `123456` (`service/test/ApiTests/RecipientUpdateTests.cs`, lines 88-94); all pass |
| F002: Code-supplied test does not also assert the creation time | Finding | `review-01.md` | Non-blocking | Resolved | `UpdateRecipient_IgnoresSuppliedCode` now compares the whole response with `original with { ... }`, so `Code`, `Id`, `CreatedAt`, and `IsForeign` are all asserted (line 72) |
| F003: The agreed US-003 HTTP test boundary is no longer covered | Finding | `review-02.md` | Blocking | Resolved | Correction attempted in `4cf7419`; succeeded. AC9 note test, supplied-`Code` test, rejected-update-unchanged test, and the 404 theory are restored and pass. The malformed-ID PUT test was not restored; the agreed test boundary does not list it (see section 8) |
| F004: Concurrent duplicate code returns 409 instead of 400 `Recipient.AlreadyExists` | Finding | `review-02.md` | Blocking | Resolved | Correction attempted in `4cf7419`; succeeded. `CreateRecipient.cs`, lines 38-46, translates `UniqueKeyConflictException` to `DataValidationException(ErrorCodes.Recipient.AlreadyExists)`. Reviewer's SQLite scratch probe of the database-conflict path returns 400 `Recipient.AlreadyExists` (section 5) |
| F005: `RecipientConcurrencyTests` accepts `Recipient.InvalidCode` | Finding | `review-02.md` | Blocking | Resolved | Correction attempted in `4cf7419`; succeeded by code inspection. The test now asserts the single outcome 400 `Recipient.AlreadyExists`, identical to the `8ac80ff` version apart from the DTO namespace. Not run (PostgreSQL) |
| F006: List paging and sorting validation narrowed without a recorded US-001 revision | Finding | `review-02.md` | Blocking | Resolved | Resolved by requirement reconciliation in `4cf7419`: US-001 AC16 (`docs/backlog/US-001-create-and-view-recipients.md`, lines 75-79) records the developer's narrowing. The recorded behavior matches review 02's probes. Code intentionally unchanged |
| F007: Formatter verification fails on three changed test files | Finding | `review-02.md` | Blocking | Resolved | Correction attempted in `4cf7419`; succeeded. `dotnet format --verify-no-changes` on the 24 story C# files exits 0 in the checkout and in a clean export |
| F008: `CreateRecipientDto.MapTo` duplicates the base mapping | Finding | `review-02.md` | Non-blocking | Unresolved | No correction attempted (intentionally left by the developer). `service/src/Core/UseCases/Recipients/Data/CreateRecipientDto.cs` unchanged in the range |
| F009: Whitespace-only code rejection no longer unit-tested | Finding | `review-02.md` | Non-blocking | Unresolved | No correction attempted (intentionally left by the developer). `service/test/UnitTests/DtoValidation/CreateRecipientDtoTests.cs` unchanged in the range |

Correction-attempt history: F003, F004, F005, F006, and F007 each received one correction
attempt in `4cf7419`, and each succeeded. F001 and F002 were closed by the F003
restoration. F008 and F009 received no correction attempt.

## 2. Blocking findings

None.

## 3. Non-blocking findings

### F008: `CreateRecipientDto.MapTo` duplicates the base mapping (carried from `review-02.md`)

Unresolved. Location: `service/src/Core/UseCases/Recipients/Data/CreateRecipientDto.cs`,
lines 31-42. `MapTo(model, timeService)` still repeats the five assignments that
`UpdateRecipientDto.MapTo(model)` performs. Suggested correction (optional): call the base
`MapTo(model)` from the create mapping, then set `Code` and `CreatedAt`.

### F009: The whitespace-only code rejection is no longer unit-tested (carried from `review-02.md`)

Unresolved. Location: `service/test/UnitTests/DtoValidation/CreateRecipientDtoTests.cs`.
US-001 AC14 and AC17 reject a code that is empty after trimming; no test exercises it.
Suggested correction (optional): add `" "` as an `InlineData` to `CodeInvalidCharacters`,
or accept the gap.

## 4. Requirement traceability

Production code for `UpdateRecipient`, `UpdateRecipientDto`, and the endpoints did not
change in the follow-up range. Statuses for those paths are inherited from `review-02.md`
where noted, and that inherited evidence was not rerun except through the full test run
in section 5.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
|---|---|---|---|---|
| AC1: Valid update returns 200 with `Envelope<RecipientDetailsDto>` | Unchanged since `review-02.md` (inherited) | `RecipientUpdateTests.UpdateRecipient_ReplacesDetailsAndKeepsCodeAndCreationTime`; rerun passed | Met | None |
| AC2: GET-by-ID and listing return new details immediately | Unchanged (inherited) | Same test asserts GET-by-ID and list; rerun passed | Met | None |
| AC3: Full replacement of the five required fields | Unchanged (inherited) | `UpdateRecipientDtoTests` and the API happy path; rerun passed | Met | None |
| AC4: US-001 field rules unchanged | Unchanged (inherited) | `UpdateRecipientDtoTests`, `CreateRecipientDtoTests`; rerun passed | Met | F008, F009 |
| AC5: Foreign status re-derived; cannot be supplied | Unchanged (inherited) | API happy path (`HU` to `US`, `IsForeign = true`); rerun passed | Met | None |
| AC6: Code, ID, creation time unchanged; supplied `Code` ignored | Unchanged (inherited) | Restored `UpdateRecipient_IgnoresSuppliedCode`. The request type `UpdateWithCodeDto` adds `Code`, and `ApiEndpoint.JsonContent` serializes the runtime type, so `Code` is actually sent. The whole response is compared with the original. Passed | Met | None |
| AC7: Invalid details rejected with 400 and stable codes; recipient unchanged | Unchanged (inherited) | Restored `UpdateRecipient_RejectsInvalidDetailsWithoutChangingRecipient` (`Country = "ZZ"`, 400 `Recipient.InvalidCountry`, recipient unchanged); passed | Met | None |
| AC8: Malformed ID returns 400; unknown ID (including 0 and negative) returns 404 `Recipient.NotFound` | Unchanged (inherited) | Restored `UpdateRecipient_UnknownId_ReturnsNotFound` (`-1`, `0`, `123456`); passed. Malformed PUT ID: no repository test; inherited from the `review-02.md` scratch run (passed), not rerun. `RecipientDetailsTests.MalformedRecipientId_ReturnsBadRequest` covers the same route binding for GET | Met | None |
| AC9: A delivery note shows updated recipient details | No production change (US-002 live relationship) | Restored `UpdateRecipient_IsShownOnExistingDeliveryNote`; passed | Met | None |
| AC10: OpenAPI describes the endpoint and its status codes | Endpoint metadata unchanged in the range | Inherited from `review-02.md` (generated document inspected); not rerun | Met | None |
| Scope: API, `UpdateRecipient` use case, persistence, `CreateRecipient` patterns and logging | Unchanged (inherited). `CreateRecipient` keeps the `logger.RunAsync` wrapper | Inherited | Met | None |
| Agreed decisions: no body `Id`, last write wins, no PATCH | Unchanged (inherited) | Inherited code inspection | Met | None |
| Agreed test boundary | Unit tests: `UpdateRecipientDtoTests`. HTTP tests: successful update with GET-by-ID, rejected update unchanged, 404 for an unknown ID, code and creation time preserved with a supplied `Code`, AC9 delivery-note test | All present in `service/test/ApiTests/RecipientUpdateTests.cs`; all passed | Met | None |
| Regression: US-001 AC11 duplicate code returns 400 `Recipient.AlreadyExists` for sequential and concurrent cases | `CreateRecipient.cs`, lines 31-46 | SQLite scratch probe of both paths: 400 `Recipient.AlreadyExists`. `RecipientConcurrencyTests` (PostgreSQL) asserts the same; not run | Met | None |
| Regression: US-001 AC16 paging and sorting validation | `ValidateListQuery` remains removed; behavior now matches the revised AC16 | Behavior from `review-02.md` probes, matching the recorded revision; not rerun | Met | None |

## 5. Verification performed

The repository commands ran from `service/` against the reviewed head with a clean working
tree. Scratch checks ran in a `git archive` export of the reviewed head under
`<temporary-directory>`; their edits were never added to the repository.

| Command | Result |
|---|---|
| `dotnet build DeliveryNoteService.sln` | Build succeeded, 0 warnings, 0 errors |
| `dotnet test DeliveryNoteService.sln --no-build --filter "Database!=PostgreSQL"` | ApiTests (`ApiTests.dll`): 67 passed, 0 failed, 0 skipped (61 at review 02; 6 restored cases). UnitTests (`Medusa.Enap.Common.UnitTests.dll`, the solution's only unit-test project): 187 passed, 0 failed, 0 skipped (unchanged) |
| `dotnet format DeliveryNoteService.sln --verify-no-changes --include <the 24 C# files changed in master..HEAD>` (checkout) | Exit code 0, no diagnostics. No `ENDOFLINE` noise appeared in this checkout |
| Same command in the clean `git archive` export of `4cf7419` | Exit code 0, no diagnostics. The committed content is clean |
| Scratch: in `CreateRecipient`, the pre-check bypassed only for codes starting with `RACE`; probe test posts the same code twice for `RACE-1` (database-conflict path) and `SEQ-1` (pre-check path) on SQLite, run with `dotnet test test/ApiTests/ApiTests.csproj --filter "FullyQualifiedName~ReviewRaceProbe"` | 2 of 2 passed: both paths return 400 `Recipient.AlreadyExists` (review 02 observed 409 `error:DNC::UniqueKeyConflict` on the conflict path) |
| Code inspection: `src/Common.Testing/ApiWrappers/ApiEndpoint.cs` (`JsonContent` uses `dto.GetType()`) | Confirms the supplied-`Code` test sends `Code` in the body |

Not run: PostgreSQL API tests (`Database=PostgreSQL`, Testcontainers), including
`RecipientConcurrencyTests`. The verification rules and `service/test/CLAUDE.md` do not allow
them without explicit approval. F005's resolution rests on code inspection and the SQLite
probe above.

## 6. Code-guideline assessment

Applied guidance: repository `CLAUDE.md` (proportionality, use case, DTO, error-code rules),
`service/test/CLAUDE.md`, the .NET and ASP.NET Core technology policies selected in earlier
reviews, and `clean-code`. Repository proportionality rules take precedence.

- `CreateRecipient`: the translation is a single `try`/`catch` around `SaveChangesAsync`,
  with a short comment explaining the race. This is the smallest form of the agreed
  contract and matches the `CLAUDE.md` rule "Add translation only for a concrete agreed
  contract that requires it" (US-001 AC11). It is simpler than the `8ac80ff` version,
  which used a private factory and a custom message; the message is not serialized, so
  dropping it has no contract effect. The logging wrapper and step order are preserved.
- Restored tests: they follow `service/test/CLAUDE.md`. Request data uses DTO instances and
  `with` expressions; the supplied-`Code` case uses a small test-local record deriving from
  `UpdateRecipientDto` rather than JSON manipulation. The set is minimal: one test per
  agreed boundary item plus a three-case 404 theory. Arrange-Act-Assert is clear. Error-code
  string literals match the existing test style.
- `RecipientConcurrencyTests`: the unused `Common.Core.Exceptions` using was removed along
  with the dual-outcome assertion.
- Formatting: build-enforced code style passes, and formatter verification passes (F007
  resolved).
- No new test infrastructure was introduced, so the extracted-infrastructure check does not
  apply.

## 7. Architecture assessment

- Layering is unchanged since review 02. The conflict translation stays in the use case, so
  the shared `ErrorHandlingMiddleware` mapping of `UniqueKeyConflictException` to 409
  remains the default for other entities.
- Data integrity: the unique index on `Recipient.Code` still prevents duplicates, and the
  concurrent case now reports the agreed error.
- External contracts: `PUT /api/recipients/{id}` is unchanged. The `POST /api/recipients`
  duplicate response again matches US-001 AC11. The `GET /api/recipients` paging and sorting
  behavior now matches the revised US-001 AC16, so it is an acknowledged contract change.
- No unnecessary parallel design was introduced.

## 8. Recorded deviations

Story deviations from US-003: none in production behavior.

Acknowledged deviations recorded for traceability:

- US-001 AC16 narrowing (acknowledged in `docs/backlog/US-001-create-and-view-recipients.md`,
  AC16, dated 2026-11-11): the recipient list has no endpoint-specific paging or sorting
  validation. Invalid page or limit values return 400 only when recipients exist, and 200 on
  an empty list. Unknown numeric `SortBy` or `SortDir` values return 200 with no ordering
  guarantee. US-001 AC15 ("Use the demo's paging validation") was not reworded; the shared
  page validation still applies when recipients exist. The developer may want to align its
  wording with AC16 when updating that story.
- Malformed-ID PUT test not restored: AC8's malformed-ID behavior has no PUT-specific
  repository test. The story's agreed test boundary does not list it, and
  `service/test/CLAUDE.md` asks for a minimal API test set. The behavior was verified in the
  `review-02.md` scratch run, and the same route binding is tested for GET.
- Plan and task-level differences from review 01 remain as recorded there. The T001 task
  verification evidence still lists the malformed-ID PUT test, which no longer exists.

Repository-required additions: none beyond the story.

## 9. Unverified items and remaining risks

- PostgreSQL API tests, including `RecipientConcurrencyTests`, were not run, as the
  verification rules require. The SQLite probe exercises the same translation path.
- OpenAPI generation (AC10) and the list-validation probes were not rerun; neither path
  changed in the follow-up range.
- Manual verification through Swagger UI or a running service was not performed. It is part
  of human acceptance.
