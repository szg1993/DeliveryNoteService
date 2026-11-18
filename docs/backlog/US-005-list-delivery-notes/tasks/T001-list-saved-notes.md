# T001: List saved delivery notes

- Parent: [US-005](../README.md)
- Status: done
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

- [x] A note created through POST appears in the list immediately with its saved
      details and current recipient details.
- [x] With no notes, the list returns 200 with an empty collection and page 1 paging
      metadata.
- [x] Defaults are page 1, limit 10, ID ascending; `Limit=0` uses the shared unpaged
      behavior.
- [x] The use case contains no paging or sorting validation.
- [x] The endpoint is anonymous and appears in the generated OpenAPI document with
      its summary, description, status codes, and response type.

## Verification

TDD through HTTP: write the two tests below, observe them fail (404 before the route
exists), then implement.

1. A created note is listed with its expected fields and current recipient.
2. With no notes, listing returns an empty collection and paging metadata.

Run the SQLite API tests with `--filter "Database!=PostgreSQL"`; existing note and
recipient tests must still pass. Run the repository formatter on changed files.

## Progress

2026-11-18:

- Added `GetDeliveryNotes` (`service/src/Core/UseCases/Notes/GetDeliveryNotes.cs`):
  `DeliveryNotesSortBy` with only `Id`, a `SortMap<Note>`, default `SortBy = Id`,
  `TechnicalUseCaseLogger<GetDeliveryNotes>.RunAsync`, dependency `repository`,
  `NoTracking()` plus the complete-note includes, `Paged(...)`, and
  `NoteDetailsDto.FromModel`. It does not validate `PageRequest` or `Sorting`.
- Mapped `GET /api/delivery-notes` in `DeliveryNoteEndpoints` with
  `[AsParameters] PageRequestQueryParam`, `[AsParameters] Sorting<DeliveryNotesSortBy>`
  and `[FromServices] GetDeliveryNotes`: anonymous, tag `Delivery notes`, name
  `GetNotes`, summary, description, `Produces<PagedEnvelope<NoteDetailsDto>>(200)` and
  `Produces<MedusaErrorResponse>(400)`.
- TDD: added `service/test/ApiTests/NoteListingTests.cs` with
  `ListNotes_IncludesCreatedNoteWithCurrentRecipient` (creates a recipient and a note
  through POST, updates the recipient through PUT, and expects the listed note to equal
  the created note with the updated recipient, total count 1) and
  `ListNotes_WithoutNotesReturnsEmptyFirstPage` (empty content, page 1, limit 10,
  total count 0, total pages 0). Both were observed failing before the route existed,
  with `MethodNotAllowed (405)` rather than the planned 404 because the existing
  `POST /api/delivery-notes` already owns the path. Both then passed.
- Defaults and `Limit=0`: verified with a temporary, uncommitted API test (11 notes).
  The default request returned the first 10 IDs in ascending order with 2 total pages;
  `Page=1&Limit=0` returned all 11 with page 1 and 1 total page. It passed and was
  deleted; T002 adds the committed paging test.
- OpenAPI: verified with a temporary, uncommitted test that resolved `ISwaggerProvider`
  from the test host. The `/api/delivery-notes` GET operation has operation ID
  `GetNotes`, a summary, tag `Delivery notes`, a description, responses 200 and 400, the
  200 schema `NoteDetailsDtoPagedEnvelope`, parameters Page, Limit, SortBy and SortDir,
  and no security requirement. It passed and was deleted; no generated-OpenAPI test is
  committed, per `service/test/CLAUDE.md`.
- Refactoring assessment (clean-code): the five-navigation `Include` chain was
  duplicated in `GetDeliveryNote` and `GetDeliveryNotes` and encodes what
  `NoteDetailsDto.FromModel` needs. Extracted it into the internal extension
  `IncludeDetails()` in `service/src/Core/UseCases/Notes/NoteDetailsIncludes.cs` and
  used it in both use cases; API tests reran green. No other production refactoring was
  justified. Test reuse: inspected `RecipientListingTests`, `NoteDetailsTests`,
  `NoteRequests`, `NoteAssertions` and `DeliveryNoteServiceApi`. The new tests reuse the
  shared host (`DeliveryNoteServiceApiTestBase`; `ApiTestBase` owns the per-test
  application, database and disposal), the `Api.Notes()` and `Api.Recipients()`
  boundary clients, and the `RecipientRequests` and `NoteRequests` DTO builders. No
  configuration before startup is needed and each test owns its data. The only
  remaining repetition is the empty-page paging assertions shared with
  `RecipientListingTests`; they are the scenario's expected outcome, so they stay
  visible and local assertions suffice.
- Formatting: `dotnet format DeliveryNoteService.sln --include` on the five changed C#
  files, then `--verify-no-changes` on the same files; both exited 0.
- Verification (from `service/`): `dotnet test test/ApiTests --filter
  "Database!=PostgreSQL"` passed 69/69. `dotnet test DeliveryNoteService.sln --filter
  "Database!=PostgreSQL"`: ApiTests 69/69 passed; UnitTests 181 passed and 6 failed. The
  6 failures are untagged PostgreSQL unit tests built on `SqlUnitTestBase`
  (`CaseInsensitiveComparerTest.PostgreSQL`, `DateTimeOffsetUtcConverterTests.PostgreSQL`,
  `NewEntityLoggerTests.PostgreSQL` and three `NavigationPropertyValidatorTests` cases
  with `usePostgres: True`). They
  failed with "Failed to connect to Docker endpoint" because Docker is not running, so
  no container started. They are unrelated to this task.

Remaining work: none for T001. T002 adds descending-order paging.
