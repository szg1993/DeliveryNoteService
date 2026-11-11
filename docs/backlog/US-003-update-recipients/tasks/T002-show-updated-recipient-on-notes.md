# T002: Show updated recipient on delivery notes

- Parent: [US-003](../README.md)
- Status: done
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

- [x] After a recipient update, retrieving a delivery note that references it
      returns the updated recipient details.

## Verification

Run the new test with SQLite API tests (`--filter "Database!=PostgreSQL"`). If it
passes without production changes, record in Progress that it characterizes
existing behavior rather than driving new code. Run the formatter on changed
files and verify formatting.

## Progress

### 2026-11-11

Added `UpdateRecipient_IsShownOnExistingDeliveryNote` to
`service/test/ApiTests/RecipientUpdateTests.cs`: create a recipient, create a
note for it, update the recipient through PUT, then read the note through
GET-by-ID and assert its `Recipient` equals the PUT response. It reuses
`RecipientRequests.Valid()`, `RecipientRequests.ValidUpdate()`, and
`NoteRequests.Valid(recipientId)`; no new helpers were needed.

The test passed on its first run without production changes. It characterizes
the existing US-002 live recipient relationship rather than driving new code.
It would fail on stale data, because the updated name, city, ZIP, and address
differ from the originals.

Refactoring assessment (clean-code): one short test that uses existing helpers;
no refactoring justified.

Verification:

- `dotnet format DeliveryNoteService.sln --include test/ApiTests/RecipientUpdateTests.cs`
  followed by `--verify-no-changes`: exit 0.
- `dotnet test DeliveryNoteService.sln --filter "Database!=PostgreSQL"`: ApiTests
  81/81 passed, UnitTests 190/190 passed.

Remaining work: none for T002.
