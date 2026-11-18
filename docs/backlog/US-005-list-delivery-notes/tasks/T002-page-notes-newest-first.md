# T002: Page through notes newest first

- Parent: [US-005](../README.md)
- Status: todo
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

- [ ] With three notes, `SortDir=Desc&Limit=2` returns the two newest notes, newest
      first, with paging metadata reporting three notes in total.
- [ ] Page 2 of the same request returns only the oldest note.

## Verification

HTTP test 3 from the story's agreed test boundary. If it passes without a production
change, record that it is a characterization of T001's behavior; otherwise observe
the failure first, then fix. Run the SQLite API tests with
`--filter "Database!=PostgreSQL"` and the repository formatter on changed files.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
