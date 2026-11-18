# US-005: List delivery notes

- Status: Ready
- Type: Backend
- Area: Delivery notes
- Implementation repository: DeliveryNoteService
- Implementation solution: `service/DeliveryNoteService.sln`
- Dependencies: Delivery-note creation and `NoteDetailsDto` from
  [US-002](US-002-create-delivery-notes.md).
- Needed by: [US-006: Delivery note pages](US-006-delivery-note-pages.md), whose list
  page has no data source without this endpoint.
- Refinement: Developer confirmed shared understanding after grilling on 2026-11-18.
- Breakdown: [Implementation plan and tasks](US-005-list-delivery-notes/README.md).
- Branch: feature/US-005-list-delivery-notes

## User story

As a delivery-note user, I want to list the saved delivery notes so that I can see
which notes exist without knowing their identifiers.

## Scope

- Add `GET /api/delivery-notes`, a paged list of saved notes, following the
  `GET /api/recipients` paging and sorting pattern from
  [US-001](US-001-create-and-view-recipients.md).
- Deliver the behavior through the API, a `GetDeliveryNotes` use case, and
  persistence, following the `CreateRecipient` logging pattern and the existing use
  case conventions.

## Acceptance criteria

1. Listing returns HTTP 200 with the saved notes as `PagedEnvelope<NoteDetailsDto>`,
   including paging metadata. Each item is the same complete note returned by
   `GET /api/delivery-notes/{id}`, including current recipient details. No new DTO is
   introduced.
2. A newly created note appears in the list without an explicit cache refresh.
3. With no saved notes, listing succeeds with an empty collection.
4. Paging and sorting match recipients: page 1, limit 10, and ID ascending by default;
   `Limit=0` returns all notes with page 1 metadata. `SortBy` accepts only `Id`;
   `SortDir` accepts `Asc` or `Desc`.
5. The use case does not validate `PageRequest` or `Sorting`, following the
   repository's GET endpoint rule; the shared paging behavior inherited from the
   recipient list applies (invalid paging values return 400 only when notes exist).
6. The endpoint is anonymous and described in the generated OpenAPI document with its
   summary, description, status codes, and response type, at the level accepted for
   US-001 (no detailed schema annotations or named sort enums required).

## Agreed decisions

- Reuse `NoteDetailsDto` for list items for simplicity; the larger payload is
  accepted, including for `Limit=0`. A summary DTO can be added later if payload size
  becomes a concrete problem.
- Sort by ID only. ID order equals creation and note-number order. The frontend
  requests `SortDir=Desc` for newest first.
- No filtering or search.

## Agreed test boundary

HTTP tests through the existing host with SQLite, creating prerequisite recipients and
notes through their endpoints. Exactly these three tests:

1. A created note is listed with its expected fields and current recipient.
2. With no notes, listing returns an empty collection and paging metadata.
3. With three notes, `SortDir=Desc&Limit=2` returns the two newest, and page 2 returns
   the oldest.

No paging or sorting validation tests and no unit tests.

## Open questions

None. The developer confirmed shared understanding on 2026-11-18.

## Excluded scope

- Filtering, searching, and sorting by fields other than ID.
- A summary list DTO.
- Updating, deleting, and status transitions.
- Frontend changes (covered by US-006).
- Authentication and creator attribution.
- Correcting the `POST /api/delivery-notes` OpenAPI description, which says text is
  trimmed. The note DTOs contain no trimming, so only the description contradicts
  US-002 and the repository DTO rules. Candidate follow-up: a documentation-only fix.

## Progress

Drafted on 2026-11-18 as the backend prerequisite for the delivery-note list page.
Refined on 2026-11-18 through a grilling session; the developer confirmed the agreed
specification ("Agree."). The [task breakdown](US-005-list-delivery-notes/README.md)
was created on 2026-11-18 (T001–T002, all todo).
