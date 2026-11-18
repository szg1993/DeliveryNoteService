# T002: Page through notes newest first

- Parent: [US-005](../README.md)
- Status: done
- Depends on: T001
- Increment: Extension

## Outcome

A caller can request the note list newest first with `SortDir=Desc` and page through
it, as the US-006 list page will.

## Scope

- Add the agreed third HTTP test to `NoteListingTests.cs`.
- Change production code only if the test exposes a gap in T001's sort map or
  paging wiring; T001's `SortMap` and the shared `Paged` helper are expected to
  support it already.
- Out of scope: other sort fields, paging or sorting validation tests.

## Acceptance criteria

- [x] With three notes, `SortDir=Desc&Limit=2` returns the two newest notes, newest
      first, with paging metadata reporting three notes in total.
- [x] Page 2 of the same request returns only the oldest note.

## Verification

HTTP test 3 from the story's agreed test boundary. If it passes without a production
change, record that it is a characterization of T001's behavior; otherwise observe
the failure first, then fix. Run the SQLite API tests with
`--filter "Database!=PostgreSQL"` and the repository formatter on changed files.

## Progress

2026-11-18:

- Added HTTP test 3, `ListNotes_PagesThroughNotesNewestFirst`, to
  `service/test/ApiTests/NoteListingTests.cs`. It creates one recipient and three
  notes through their endpoints, requests `sortDir=Desc&page=1&limit=2` and
  `sortDir=Desc&page=2&limit=2` (no `SortBy`, so the use case's default `Id` sort
  applies), and asserts the first page holds the newest and middle note IDs in that
  order with `TotalCount` 3, and the second page holds only the oldest note ID.
- Outcome: characterization of T001's behavior. The test passed on its first run
  without any production change; T001's `SortMap` (ID), default `SortBy`, and the
  shared `Paged` helper already honour `SortDir=Desc` with paging. No gap was
  exposed, so no red phase was possible; production code is unchanged.
- Refactoring assessment (`clean-code`, test code only): the test follows the
  file's existing Arrange-Act-Assert layout and reuses the suite helpers
  `RecipientRequests.Valid`, `NoteRequests.Valid`, `Api.Notes()` and the
  `QS()`/`Page()` query-string helpers. The existing `Sort(sortBy, sortDir)` helper
  was not used because it always sends `SortBy`, and the agreed scenario sends only
  `SortDir`. The two `QS("sortDir", "Desc")` calls are scenario-visible inputs, so
  no new helper is justified. Reuse assessment (`component-testing`): consumers
  inspected were `NoteListingTests` and `RecipientListingTests`; both use the shared
  `DeliveryNoteServiceApiTestBase` host (per-test host and client created and disposed by
  `ApiTestBase`) and local per-scenario arrangement. A shared "create N notes" or
  "newest first query" helper would have a single consumer, so local code suffices.
  No refactoring was applied.
- Formatting: `dotnet format DeliveryNoteService.sln --include
  test/ApiTests/NoteListingTests.cs`, then the same with `--verify-no-changes`
  (exit 0).
- Verification: `dotnet test test/ApiTests --filter "Database!=PostgreSQL"` from
  `service/`: Passed 70, Failed 0, Skipped 0.
- Remaining work: none for T002. US-005 moves to In review.
