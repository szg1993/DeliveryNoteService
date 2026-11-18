# US-005: List delivery notes

- Source: DeliveryNoteService/docs/backlog/US-005-list-delivery-notes.md
- Refinement source: conversation: US-005 grilling session, 2026-11-18
- Lifecycle: active
- Created: 2026-11-18
- Updated: 2026-11-18
- Branch: feature/US-005-list-delivery-notes

## Approach

The story adds one read endpoint on established patterns, so no prerequisite task is
needed. T001 is the first working increment: `GET /api/delivery-notes` returns saved
notes as `PagedEnvelope<NoteDetailsDto>` with default paging (page 1, limit 10, ID
ascending), including current recipient details, through a new `GetDeliveryNotes`
use case and OpenAPI metadata. T002 extends it with the caller-selected ID sort
direction and proves newest-first paging, which the US-006 list page relies on.

Both tasks change the same use case and test file, so run them in order.

## Ordered tasks

| Order | Task | Status | Depends on | Increment |
| --- | --- | --- | --- | --- |
| 1 | [T001: List saved delivery notes](tasks/T001-list-saved-notes.md) | done | None | Skeleton |
| 2 | [T002: Page through notes newest first](tasks/T002-page-notes-newest-first.md) | done | T001 | Extension |

The task file owns its status and progress. Keep the status above synchronized with
the task file.

## Acceptance coverage

| Source criterion | Tasks |
| --- | --- |
| AC1: 200 with `PagedEnvelope<NoteDetailsDto>`, full details and current recipient | T001 |
| AC2: created note listed without cache refresh | T001 |
| AC3: empty collection when no notes | T001 |
| AC4: defaults page 1, limit 10, ID ascending; `Limit=0` returns all | T001 (defaults, `Limit=0` via shared helper), T002 (paging verified) |
| AC4: `SortBy=Id` only; `SortDir` Asc/Desc | T001 (sort map with ID), T002 (Desc verified) |
| AC5: no paging/sorting validation in the use case | T001 |
| AC6: anonymous; OpenAPI at the US-001 level | T001 |
| Test boundary tests 1–2 | T001 |
| Test boundary test 3 | T002 |

## External prerequisites

- US-002 `POST /api/delivery-notes`, `GET /api/delivery-notes/{id}`, and
  `NoteDetailsDto`: Done and merged in master. Verified in
  `service/src/WebApp/RestApi/DeliveryNoteEndpoints.cs` and
  `service/src/Core/UseCases/Notes/`.

## Assumptions and open questions

No open questions. Agreed decisions are recorded in the source story.

Verified implementation patterns (2026-11-18):

- `GetRecipients` (`service/src/Core/UseCases/Recipients/GetRecipients.cs`) shows the
  list pattern: a sort-by enum, a `SortMap<T>`, a default `SortBy`, and
  `repository.Paged(pageRequest, SortMap.Get(sorting), ct).As(Dto.FromModel)`.
- `GetDeliveryNote` loads a complete note with `NoTracking()` and `Include(...)` for
  recipient, contact, package, deadline, and lines. `Include` returns
  `IReadOnlyRepository<T>`, which the `Paged` extension accepts, so the two patterns
  combine without new infrastructure.
- `RecipientEndpoints` shows the `[AsParameters] PageRequestQueryParam` and
  `Sorting<T>` binding.
- API test helpers exist: `NoteRequests.Valid(recipientId)` and
  `NoteAssertions` in `service/test/ApiTests/TestSuite/`.

Run API tests with `--filter "Database!=PostgreSQL"`; PostgreSQL tests need explicit
approval.

## Resume notes

- Reservation: attempt `26005bd4-61ed-4e47-944a-033eb2459788`, remote `origin`,
  branch `feature/US-005-list-delivery-notes`, marker
  `8f43afb2840fac266aa7c42acab189ab3571d3f2`, confirmed 2026-11-18.
- 2026-11-18 implementation run stopped after agent review: T001 (`e2afcd4`) and
  T002 (`bbc0840`) done; [review-01](review-01.md) `PASS` at `bbc0840` with no
  findings and no correction cycles. Story is `In review`; plan stays `active`.
  Next: developer human review and optional Swagger spot-check of `Limit=0` and
  `SortDir=Desc`, then explicit approval to record human acceptance.
