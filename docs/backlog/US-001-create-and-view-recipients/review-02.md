# US-001: Agent review 02

- Review type: agent review
- Review date and time: 2026-11-10 13:16 Europe/Budapest (UTC+01:00)
- Authoritative story: `docs/backlog/US-001-create-and-view-recipients.md`
- Plan: `docs/backlog/US-001-create-and-view-recipients/README.md`
- Harness repository and commit: `AgenticEngineering`, `d19a8d75f1a6e59c2c5953d32aa0741e4fc0195f`
- Reviewed branch: `feature/US-002-create-delivery-notes`
- Reviewed head: `9a0ea7ba8bd4eed320a87e6a9295ab36f99a2405`
- Merged US-001 implementation: `025ec1cc157ab38b5cd4e5b8a133eec5f55839f9`
- Comparison base: original PR base, `17fa518f621fbe6a6c4fac138dbf3cfb76ce0c3d`
- Review mode: follow-up review after merge
- Previous review: [review-01.md](review-01.md), complete coverage, PASS
- Previous reviewed head: `5f5b513d23bf5a733b691dc8e1f7188a1f7c5226`
- Committed follow-up range: `5f5b513d23bf5a733b691dc8e1f7188a1f7c5226..9a0ea7ba8bd4eed320a87e6a9295ab36f99a2405`
- Previous comparison base: `17fa518f621fbe6a6c4fac138dbf3cfb76ce0c3d`, unchanged for this comparison
- Review coverage: incomplete
- Explicitly included uncommitted changes: None
- Final verdict: FAIL

## Verdict and merge readiness

The merged recipient implementation has blocking validation and contract
regressions. Passing current tests does not establish the agreed story contract:
several tests were removed or changed to accept the new behavior. Formal
completion must remain pending corrections or explicit agreement of revised
requirements, followed by review.

This report establishes the findings below through targeted recipient inspection,
the current SQLite solution tests, executable DTO validation checks, and formatter
verification. It does not certify every change in the much broader shared-code
and demo-removal refactor. Review coverage is explicitly incomplete; remaining
work is listed below and must be completed before a later PASS. Do not use this
report to inherit unexamined portions of that refactor as approved.

The prior reviewed head is an ancestor of the current head. `fa1a3ba` and
`99ae0fd` introduce implementation/test changes; `347e49b`, `8212045`, the merge,
the US-002 reservation/planning commits, and `9a0ea7b` provide review/tracking or
merge context. The current branch contains US-002 planning, not Note code. The
user selected committing the staged tracking edits and reviewing US-001; those
edits were committed separately as `9a0ea7b` before freezing this review head.

| Item | Type | First reported in | Previous severity | Outcome | Evidence |
| --- | --- | --- | --- | --- | --- |
| Prior findings | Findings | review-01 | None | No correction attempt to reassess | Prior PASS had no blocking/non-blocking findings; F001-F006 below are new |
| R001: Default-parallel suite stall | Risk | review-01 | Non-blocking risk | Unresolved | Sequential tests pass; parallel mode was not repeated |
| R002: PostgreSQL timestamp precision | Risk | review-01 | Non-blocking risk | Unresolved inherent limit | Provider test clock remains fixed; PostgreSQL was not rerun |
| R003: Duplicate shared trees untracked | Scope note | review-01 | Not a finding | No longer applicable as stated | Duplicate trees are now tracked; 121 added/modified C# files there are outside the referenced solution projects |
| R004: PR/merge awaiting a human | Lifecycle context | review-01 | Not a finding | Resolved for merge | Developer confirmation and local merge commit 025ec1c establish PR #1 merged; this is not a new acceptance record |

## Blocking findings

### F001: Reject blank values after normalization and restore trimmed-length behavior

Location: `service/src/Core/UseCases/Recipients/CreateRecipientDto.cs:30` and
`src/Common.Core/UseCases/Guard.cs:27`.

The DTO calls Guard.ThrowIfMissing before trimming. That guard checks
IsNullOrEmpty, so whitespace-only Code, Name, Zip, City, and Address all pass
Validate. The code-character check also accepts an empty trimmed code because
All on an empty sequence returns true. MapTo subsequently stores empty strings;
the relational required constraint does not reject empty strings. This violates
AC4, AC14, and the required-code portion of AC17.

Executable checks against the built DTO confirmed that replacing each of those
five fields with three spaces independently succeeds in Validate. Country was
kept HU and all other fields were valid. This check did not create database rows
and is explicitly not an HTTP regression test.

Length checks likewise run on raw text. The removed HTTP test
CreateRecipient_AcceptsMaximumLengthAfterTrimming covered padded values whose
normalized length equals the maximum; new DTO tests instead expect rejection.
Normalize before validating requiredness/limits, retain the correct field error
data, and restore HTTP boundary coverage for blanks and padded boundary lengths.

### F002: Duplicate-code response contradicts the confirmed recipient contract

Location: `service/src/Core/UseCases/Recipients/CreateRecipient.cs:30` and
`service/test/ApiTests/RecipientConcurrencyTests.cs:26`.

The pre-check now throws DataValidationException/Recipient.InvalidCode, returning
400 for an existing normalized code. AC11 and the story's Agreed decisions require
409. RecipientCreationTests now expects 400, while the concurrent test permits
either 400 or 409 depending on the race. The route still advertises duplicate
codes as 409, making the implementation and documentation disagree as well.

Restore a consistent approved duplicate-code contract across the pre-check,
database conflict, HTTP tests, and OpenAPI, or obtain an explicit recipient-story
requirement change and record it. The conversation's 400-business-error decision
was made while refining US-002; its plan explicitly leaves recipient contracts
unchanged. A merge alone does not reconcile this specification mismatch.

### F003: Nonpositive recipient IDs changed from 400 to 404 without a story update

Location: `service/src/Core/UseCases/Recipients/GetRecipient.cs:21` and
`service/test/ApiTests/RecipientDetailsTests.cs:41`.

The positive-ID validation was removed. Zero and negative IDs now reach the
repository and become 404; the changed HTTP tests confirm that behavior. AC16
explicitly requires 400 for nonpositive IDs and 404 for an unknown positive ID.
Restore the approved behavior or explicitly reconcile the recipient requirement
and its tests/documentation before acceptance.

### F004: Restore use-case-level validation and no-save HTTP regressions

Location: `service/test/ApiTests/RecipientCreationTests.cs:47` and the deletion of
ListRecipients_InvalidCreationDoesNotChangeList from RecipientListingTests.

The follow-up removes HTTP cases for blank/null/missing fields, maximum lengths,
unsupported code characters, unrecognized countries, and observing an unchanged
list after rejection. DTO-level tests replace much of this coverage but do not
check routing, validation invocation, error status/shape, or persisted effects at
the agreed HTTP boundary. They also omit whitespace-only cases, allowing F001 to
pass the full suite. Restore proportionate HTTP rejection/error/no-save coverage
for AC4/14/16/17, retaining focused unit tests only where they add distinct value.

### F005: Fix the new entity error-code constant

Location: `service/src/Core/Errors/ErrorCodes.cs:13`.

AlreadyExists is declared as _prefix + "InvalidCountry". It therefore equals the
country-validation code and violates AGENTS.md's explicit rule that each suffix
match its constant name. Correct it to AlreadyExists or remove the unused
constant. It is currently unused, so this review does not claim a live duplicate
response emits that wrong value.

### F006: Required formatting verification fails

Locations include `service/src/Persistence/DesignTimeDbContextFactory.cs:5`,
`service/src/WebApp/Auth/AuthSettings.cs:6`,
`service/src/WebApp/Diag/DiagInit.cs:11`,
`service/test/ApiTests/DtoFactories/DtoFactory.cs:19`, and
`service/test/ApiTests/GlobalUsings.cs:1`.

Read-only dotnet format verification exits 1 with whitespace, final-newline,
and imports-order diagnostics in changed product/shared/test files and migration
artifacts. Apply the repository formatter to the affected files during a
separate correction, then verify again. This review did not format source files.

## Non-blocking findings

None beyond the carried risks and explicit verification limitations.

## Requirement traceability

Met below is scoped to the cited recipient behavior. It does not establish
complete review coverage of the wider refactor. Historical review-01 evidence
is inherited only where identified and was not rerun as a historical test run.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
| --- | --- | --- | --- | --- |
| AC1/2: 201, Location, normalized details, ID/UTC time/foreign flag | Current POST, DTO MapTo/FromModel | Current create/Location/readback tests passed | Met | None for normal valid input; F001 covers boundary validation |
| AC3/5/6: Immediate list visibility, ordering, empty list | Current GetRecipients and sorted repository paging | Current listing HTTP tests passed | Met | None |
| AC4: Invalid creation rejected without persistence | Current DTO/guard admits whitespace; rejection-list test removed | Built DTO check reproduces admission; current suite misses it | Not met | F001, F004 |
| AC7/8: Existing detail and unknown positive ID | Current GetRecipient and 404 error | Current Location and unknown-ID HTTP tests passed | Met | None |
| AC9: All operations described in generated OpenAPI | Endpoint metadata remains; OpenApiSpecTest/artifact removed; Swagger registration changed | Live generated document not independently retrieved in this review | Not verifiable | Remaining verification below |
| AC10: Duplicate names allowed | Only code has uniqueness constraint | Current HTTP duplicate-name test passed | Met | None |
| AC11: Normalized-code uniqueness and 409, including races | Unique index retained, pre-check changed to 400 | Current sequential tests pass expecting 400; PostgreSQL test relaxed and not run | Not met | F002 |
| AC12/15: Paging defaults, unpaged listing and stable sorts | Route validation, effective ID default, name/ID tie-breaker, shared 64-bit offset guard | Current SQLite paging/sorting HTTP tests passed | Met | None |
| AC13: Recognized normalized country/foreign flag | Country set and DTO projection | Current HU/US HTTP scenarios and country DTO unit cases passed | Met | Missing rejection-boundary coverage is F004 |
| AC14: Required/trimmed/limited fields and textual ZIP | Validation precedes normalization and admits blanks | Removed trimmed-boundary tests; current units expect raw length limits | Not met | F001, F004 |
| AC16: 400 invalid IDs/fields, 404 unknown positive ID, stable errors | Current GetRecipient and middleware | Current nonpositive-ID tests pass expecting 404 | Not met | F003, F004 |
| AC17: Required ASCII code after normalization | Predicate retained but empty normalized code passes | Executable whitespace-code check accepted | Not met | F001, F004 |
| AC18: No postal/address verification | No new postal/address format validation | Current DTO unit case passed; historical HTTP evidence in review-01 not rerun | Met | Boundary-test gap is F004 |
| Agreed HTTP test seam | Existing real-host test infrastructure retained, validation scenarios moved to DTO tests | HTTP and unit tests independently run | Not met | F004 |
| Required code/error conventions and formatter verification | Logging/DTO mapping follow current local patterns; bad constant and formatter failures remain | Read-only format verification failed | Not met | F005, F006 |
| Historical TDD execution | Task progress records retained | Inherited historical evidence from review-01; not recreated | Met | No claim that later edits followed a new red/green cycle |
| Wider refactor, generated API and schema compatibility | Core consolidation, demo removal, namespace/migration changes | Solution builds/tests pass, broader inspection remains incomplete | Not verifiable | Remaining scope below |

## Verification performed

Working directory: repository root. No source or test corrections were made.

```text
dotnet test service/DeliveryNoteService.sln --no-restore --filter "Database!=PostgreSQL" -v minimal --blame-hang --blame-hang-timeout 90s -- xUnit.ParallelizeTestCollections=false
```

Result: exit 0; 56 API tests and 171 unit tests passed, zero failures or skips in
the selected tests. PostgreSQL-tagged tests were excluded, as repository policy
requires for routine checks. The sandboxed attempt exited 1 without diagnostics;
the authorized outside-sandbox run produced these successful results.

Read-only reflection checks loaded the built Common.Core and DeliveryNoteService
Core assemblies, constructed otherwise valid CreateRecipientDto instances, and
called Validate separately with Code, Name, Zip, City, and Address set to three
spaces. All five were accepted. No production service/database was contacted.

```powershell
$files = @(git diff --name-only --diff-filter=AM 5f5b513..9a0ea7b -- '*.cs' | Where-Object { Test-Path -LiteralPath $_ })
dotnet format service/DeliveryNoteService.sln --no-restore --include-generated --verify-no-changes --include $files
```

Result: exit 1 with formatter diagnostics. The include list requested 394
added/modified C# files; solution-loaded projects determine what the formatter
actually checks. Of that list, 121 files are in unused duplicate shared trees;
their formatting is not established by verification of the active solution.
The sandbox initially denied the build-host pipe connection; the authorized
outside-sandbox run reached the formatter and reported F006.

dotnet ef --version was also checked: the CLI is not on PATH. No EF tool was
installed and no migration/database mutation was attempted for review.

## Code-guideline assessment

Applied central C# style, dependency injection, ASP.NET Core, .NET testing,
OpenAPI, and version-contract guidance, with repository instructions taking
precedence. EnforceCodeStyleInBuild remains true in applicable project props.
Repository logging and DTO init/mapping patterns are present in current recipient
use cases. F005/F006 prevent code-guideline approval.

Skills used: clean-code for scoped readability/error handling and component-testing
for boundary/fixture assessment. Clean-code assessment for inspected recipient
changes: 7/10; restore normalized validation, meaningful public-boundary tests,
consistent errors, and clean formatting to reach the intended standard. The
what-next assessment preceded this selected review and was not an implementation
step. No implementation/refactoring skill work was performed during review.

## Architecture assessment

Recipient domain/application code moved into Core, with persistence referencing
Core and WebApp composing the layers. Referenced shared projects remain under
root src; duplicate service/src/Common.* projects are not the active dependencies.
The HTTP test wrapper/base remains shared across recipient creation, detail,
listing, and version consumers, with per-test database lifecycle and controlled
clock. The PostgreSQL subclass retains instance-scoped ownership/disposal; it was
inspected but not executed.

The real application boundary remains available, making the loss of its validation
scenarios unnecessary for the refactor. Provider uniqueness still protects the
stored code, but application-precheck and database-race paths now expose different
failure contracts. This is a behavior change, not just internal consolidation.

## Recorded deviations

- Core replaces the prior Domain/Application split; demo routes, message/scheduling
  setup, related tests, and the DemoModels table are removed. RemoveDemoModel is a
  new migration. These changes extend beyond the original recipient plan's stated
  internal scope and must be recorded/reconciled when completing review.
- Validation testing moved substantially from HTTP to DTO unit tests. This does
  not satisfy the original public-boundary agreement by itself (F004).
- Duplicate/nonpositive-ID responses differ from the unchanged story (F002/F003).
  User merge confirmation establishes merge state, not the intended resolution of
  those conflicting requirements.
- Entity-specific error codes were grouped and renamed under the new local
  convention. The bad AlreadyExists value remains a local-rule violation (F005).
- The shared paging helper gained the existing overflow protection while route
  validation now guards the list request, following the new local GET policy.
- Prior deviations and historical baseline acceptance remain recorded in
  review-01; unchanged historical evidence was not rerun.

## Unverified items and remaining risks

- Before PASS, complete inspection of the broader shared-library/namespace,
  service-owned infrastructure, deleted demo behavior, and migration changes in
  fa1a3ba/99ae0fd. This report's coverage is incomplete for that wider refactor.
- Independently retrieve the generated OpenAPI and verify all three recipient
  operations and request/error/Location schemas after changed Swagger registration
  and removal of OpenApiSpecTest. Do not infer correctness from class metadata alone.
- Check runtime model versus migration snapshot consistency before future Note
  migrations: the snapshot retains the old recipient CLR name and JobExecution
  model, while runtime Core/AppDbContext changed. A mismatch is not asserted to
  cause data loss here; the model difference needs actual verification.
- SQLite tests do not establish PostgreSQL migration, collation, locking, or race
  behavior. Current repository policy permits SQLite-only routine verification;
  any Testcontainers run still requires explicit approval.
- Default-parallel hang cause and PostgreSQL timestamp precision remain the
  carried risks R001/R002. No new claim is made about those checks.
- Human acceptance cannot be recorded while these blocking findings remain.
  Correction is a separate implementation action, followed by completion of the
  omitted review work and a new review. No source/story statuses changed here.
