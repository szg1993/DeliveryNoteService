# T002: Show updated recipient on delivery notes

- Parent: [US-003](../README.md)
- Status: todo
- Depends on: T001
- Increment: Extension

## Outcome

It is proven through HTTP that a delivery note shows its recipient's current
details after the recipient is updated, confirming the live relationship agreed
in US-002.

## Scope

- Add one HTTP test, either in `service/test/ApiTests/RecipientUpdateTests.cs` or
  `service/test/ApiTests/NoteDetailsTests.cs`, whichever reads better: create a
  recipient, create a delivery note for it, update the recipient through
  `PUT /api/recipients/{id}`, then retrieve the note through
  `GET /api/delivery-notes/{id}` and assert its `Recipient` matches the updated
  details.
- Reuse existing request factories (`RecipientRequests`, `NoteRequests`) and the
  T001 update request.
- Production code changes only if the test exposes a defect; delivery-note
  retrieval is expected to load the current recipient already.

## Acceptance criteria

- [ ] After a recipient update, retrieving a delivery note that references it
      returns the updated recipient details.

## Verification

Run the new test with SQLite API tests (`--filter "Database!=PostgreSQL"`). If it
passes without production changes, record in Progress that it characterizes
existing behavior rather than driving new code. Run the formatter on changed
files and verify formatting.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
