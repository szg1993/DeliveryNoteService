# T003: Verify concurrent creation and number exhaustion

- Parent: [US-002](../README.md)
- Status: done
- Depends on: T001, T002
- Increment: Edge case

## Outcome

Concurrent requests and end-of-range creation preserve the note numbering and
atomic-creation contract, with exhaustion reported as a business error (400).

## Scope

- Extend the existing note TestSuite helpers with bounded concurrent HTTP requests
  and per-test SQLite identity configuration. Use independent application requests
  and database connections; do not serialize them using an application test lock
  or replace the real numbering/persistence logic.
- Configure the Notes identity through isolated datastore setup, not by inserting
  99,999 notes or introducing a production sequence-control endpoint. The shared
  fixture already configures sqlite_sequence; narrow helpers to the current
  test's Notes identity and leave all other entities and tests unchanged.
- Verify normal contention, N99999 allocation, exhaustion, and competition for the
  final available number through POST and GET. The range constraint, atomic graph
  save, and stable 400 translation must already exist from T001.
- Fix defects exposed by these cases within the pinned allocation design. Do not
  add a distributed allocator, provider upgrade, general retry framework, or new
  test harness. Do not change unrelated global 409 mappings.
- Keep generated OpenAPI consistent with any corrected error behavior. No
  PostgreSQL/Testcontainers task is required by the repository's current policy.

## Acceptance criteria

- [x] Two valid requests started together in an isolated nonexhausted database
  both return 201, with distinct IDs and note numbers; both complete note graphs
  can be retrieved through their Location URIs with their own supplied details.
- [x] Setting the note identity to 99998 allows creation of N99999. A subsequent
  valid creation returns standard HTTP 400 with Note.NumberExhausted, not 409 or
  an unhandled database error.
- [x] At exhaustion, repeated valid requests remain 400 and do not alter the
  previously saved N99999 note or its children. GET for a failed next candidate
  note ID remains 404; the successful note remains fully retrievable.
  Exhaustion does not reset or reuse the permanent sequence. Gaps are allowed;
  do not require 99,999 saved notes before the allocated range can be exhausted.
- [x] When two valid creations compete for the last available number, exactly
  one returns 201/N99999 and the other 400/Note.NumberExhausted. No duplicate or
  out-of-range note is observable; the winner's full graph reads back correctly.
- [x] Named database-constraint translation is limited to the note range error;
  other technical database errors are not misreported as number exhaustion.
- [x] The test arrangement uses the real host, real atomic persistence, and
  database identity allocation. No row-count assertions against application
  tables or internal-service mocks replace the HTTP behavior checks.
- [x] All ordinary SQLite API tests pass with PostgreSQL cases excluded, and
  generated endpoint metadata still describes business failures as 400.

## Verification

Give each scenario a separate existing test database and fixed test clock.
Configure only that database's note identity, create the recipient through HTTP,
and use a start barrier plus a bounded cancellation deadline for concurrent POSTs.
Use a small fixed request count, not repeated stress loops or timing sleeps.
Reuse disposal ownership in DeliveryNoteServiceApiTestBase; dispose raw HTTP
responses and cancellation resources in the helper/test that owns them.

Exercise boundary state directly through datastore setup rather than creating
large record counts. Observe success, rejection, saved contents, and missing
failed IDs through HTTP. Document that these observations plus the named
constraint and single graph transaction establish the note-level failure
contract; child records have no independent public route in this story.

Run the focused numbering/concurrency cases first, then:

```text
dotnet test service/test/ApiTests/ApiTests.csproj --filter "Database!=PostgreSQL" -v minimal -- xUnit.ParallelizeTestCollections=false
```

Format changed C# files and verify formatting before final tests. Record exact
commands, providers, red/green observations, and the remaining PostgreSQL fidelity
limit. Inspect the production migration and named error translation for both
configured providers; do not claim SQLite proves PostgreSQL locking/migrations.
Never start Testcontainers without separate explicit approval.

## Progress

- 2026-11-10: Completed the four bounded HTTP numbering scenarios: simultaneous successful creations with distinct N00001/N00002 and full Location readback; allocation of N99999 followed by repeated 400/Note.NumberExhausted; unchanged full saved graph after exhaustion with no retrievable note 100000; and competition for the final number with exactly one complete successful graph and one business rejection. Distinct request comments correlate each saved result to its caller. Every application scenario uses its own SQLite database and fixed clock. Only Notes identity is reset/advanced using the existing helper; no bulk note inserts, row-count assertions, internal mocks, locks or replacement allocator were introduced.
- TDD evidence: the normal contention and exhaustion tests were initially green against T001/T002's required invariants, so no production change was made for those scenarios. The technical-constraint scenario exposed a genuine red: an unrelated check named CK_Notes_NumberRange_Other returned 400/Note.NumberExhausted because the SQLite translation used a prefix Contains match. Narrowed it to the exact terminated constraint name; the same HTTP scenario then returned the expected standard 500 while GET confirmed the failed note was absent. The real named range violation still returns 400. A test-only table/trigger supplies that datastore failure; it is not an application endpoint or internal substitute.
- Provider/atomicity inspection: AppDbContext translates only SQLite extended code 275 with the exact named check, or PostgreSQL CheckViolation with exact ConstraintName. The migration and runtime mapping retain CK_Notes_NumberRange (1..99999), generated identities and unconstrained PostgreSQL numeric; no migration changed in T003. Each creation still performs one EF graph SaveChangesAsync transaction. Failed-note absence plus complete unchanged winner readback and this single transaction establish the note-level atomicity contract; children have no independent public observation route. Gaps consume capacity without requiring 99,999 saved notes. SQLite observations do not prove PostgreSQL identity, locking, migration or constraint-error behavior; no PostgreSQL tests or Testcontainers ran.
- Refactoring assessment: reviewed AppDbContext, NoteNumberingTests, NoteDetailsTests, RecipientConcurrencyTests, NoteCreationTests, NoteDocumentationTests and their TestSuite consumers. Renamed RecipientConcurrency to ConcurrentCreation and reused one start-barrier/30-second cancellation protocol behind note/recipient capability methods. Requests retain independent HTTP scopes and database connections; no serialization gate surrounds creation. Added cleanup of completed raw responses when a peer faults/cancels, with a focused protocol lifecycle test using a deterministic HTTP handler. That test verifies test infrastructure only; application acceptance remains at the agreed real POST/GET boundary.
- Test-infrastructure reuse/lifecycle: NoteAssertions now owns repeated creation-envelope/Location/fresh-GET/equivalence mechanics and is consumed by both detail and numbering tests. Scenario-specific comments, exact numbers, errors and full-detail expectations remain visible. Existing explicit dual-Location assertions in NoteDetailsTests retain their distinct scenario meaning. Per-test host/database/clock isolation and disposal remain owned by DeliveryNoteServiceApiTestBase. Datastore setup occurs after initialization and before HTTP actions. Successful raw creation responses are owned/disposed by the test; observation helpers dispose their GET responses; the concurrency helper owns only its cancellation source and failure-path responses. No second application harness or mutable shared state was added.
- Clean-code assessment: 10/10 for this increment's scope; extracted shared protocol/observation mechanics, removed duplicated concurrency setup, kept the persistence fix narrow and preserved visible expected outcomes. No further production refactoring was justified. The implement-task pre-verification refactoring requirement takes precedence over the tdd skill's generic suggestion to defer refactoring to review.
- Verification (repository root): dotnet test service/test/ApiTests/ApiTests.csproj --filter "FullyQualifiedName~NoteNumberingTests&Database!=PostgreSQL" -v minimal: four passed after the exact-name fix. dotnet test service/test/ApiTests/ApiTests.csproj --filter "(FullyQualifiedName~NoteNumberingTests|FullyQualifiedName~ConcurrentCreationTests|FullyQualifiedName~NoteDetailsTests)&Database!=PostgreSQL" -v minimal: 17 passed after refactoring. dotnet test service/test/ApiTests/ApiTests.csproj --filter "Database!=PostgreSQL" -v minimal -- xUnit.ParallelizeTestCollections=false: 228 passed, including generated OpenAPI checks. Final dotnet test service/DeliveryNoteService.sln --filter "Database!=PostgreSQL" -v minimal -- xUnit.ParallelizeTestCollections=false: 228 API tests and 171 unit tests passed after the last C# edit and formatting.
- Formatting: dotnet format service/DeliveryNoteService.sln --include service/src/Persistence/AppDbContext.cs service/test/ApiTests/NoteNumberingTests.cs service/test/ApiTests/NoteDetailsTests.cs service/test/ApiTests/ConcurrentCreationTests.cs service/test/ApiTests/TestSuite/ConcurrentCreation.cs service/test/ApiTests/TestSuite/NoteAssertions.cs service/test/ApiTests/TestSuite/NoteDatabase.cs --no-restore, followed by the same command with --verify-no-changes: passed. git diff --check passed. No Markdown formatter is configured; tracking follows repository conventions.
- Skills used: tdd (red/green for the reproduced error-translation defect), clean-code (scoped implementation/refactoring assessment), component-testing (required by .NET testing policy; real HTTP host and persistence, SQLite, deterministic clock, reusable suite infrastructure). No loaded-but-unused skills.
- Remaining work: all three coding tasks are done. Story moves to In review and plan remains active. Independent story review and explicit human acceptance remain outstanding; this task does not record either. The pre-existing JobExecutions snapshot drift remains documented under T001 and was not changed here. Reservation aa94e8f5-208c-4204-a53b-50dac3e54bd0 was verified and reused; no commits were pushed.
