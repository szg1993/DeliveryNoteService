# T001: List saved delivery notes

- Parent: [US-005](../README.md)
- Status: todo
- Depends on: None
- Increment: Skeleton

## Outcome

A caller can list saved delivery notes through `GET /api/delivery-notes` and receive
each note's complete details, including current recipient details, in a paged
envelope with the default paging and order.

## Scope

- Add `GetDeliveryNotes` in `service/src/Core/UseCases/Notes/`, following
  `GetRecipients`: a `DeliveryNotesSortBy` enum with only `Id`, a `SortMap<Note>`,
  `Id` as the default `SortBy`, the `TechnicalUseCaseLogger<GetDeliveryNotes>`
  `RunAsync` wrapper, and the same `Include(...)` set as `GetDeliveryNote`, mapped
  with `NoteDetailsDto.FromModel`. Name the repository dependency `repository`. Do not
  validate `PageRequest` or `Sorting`.
- Map `GET /api/delivery-notes` in `DeliveryNoteEndpoints` with
  `[AsParameters] PageRequestQueryParam` and `Sorting<DeliveryNotesSortBy>`, as in
  `RecipientEndpoints`. Mark it anonymous and add tags, name, summary, description,
  and `Produces<PagedEnvelope<NoteDetailsDto>>(200)` plus 400 at the US-001 level.
- Add HTTP tests in a new `service/test/ApiTests/NoteListingTests.cs`, creating
  recipients and notes through their endpoints with the existing helpers.
- Out of scope: descending-order paging tests (T002), a summary DTO, filtering.

## Acceptance criteria

- [ ] A note created through POST appears in the list immediately with its saved
      details and current recipient details.
- [ ] With no notes, the list returns 200 with an empty collection and page 1 paging
      metadata.
- [ ] Defaults are page 1, limit 10, ID ascending; `Limit=0` uses the shared unpaged
      behavior.
- [ ] The use case contains no paging or sorting validation.
- [ ] The endpoint is anonymous and appears in the generated OpenAPI document with
      its summary, description, status codes, and response type.

## Verification

TDD through HTTP: write the two tests below, observe them fail (404 before the route
exists), then implement.

1. A created note is listed with its expected fields and current recipient.
2. With no notes, listing returns an empty collection and paging metadata.

Run the SQLite API tests with `--filter "Database!=PostgreSQL"`; existing note and
recipient tests must still pass. Run the repository formatter on changed files.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
