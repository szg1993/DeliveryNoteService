# T003: Verify concurrent creation and number exhaustion

- Parent: [US-002](../README.md)
- Status: todo
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

- [ ] Two valid requests started together in an isolated nonexhausted database
  both return 201, with distinct IDs and note numbers; both complete note graphs
  can be retrieved through their Location URIs with their own supplied details.
- [ ] Setting the note identity to 99998 allows creation of N99999. A subsequent
  valid creation returns standard HTTP 400 with Note.NumberExhausted, not 409 or
  an unhandled database error.
- [ ] At exhaustion, repeated valid requests remain 400 and do not alter the
  previously saved N99999 note or its children. GET for a failed next candidate
  note ID remains 404; the successful note remains fully retrievable.
- [ ] When two valid creations compete for the last available number, exactly
  one returns 201/N99999 and the other 400/Note.NumberExhausted. No duplicate or
  out-of-range note is observable; the winner's full graph reads back correctly.
- [ ] Named database-constraint translation is limited to the note range error;
  other technical database errors are not misreported as number exhaustion.
- [ ] The test arrangement uses the real host, real atomic persistence, and
  database identity allocation. No row-count assertions against application
  tables or internal-service mocks replace the HTTP behavior checks.
- [ ] All ordinary SQLite API tests pass with PostgreSQL cases excluded, and
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

No implementation started. Record dated work, verification evidence, and remaining
work here during implementation.
