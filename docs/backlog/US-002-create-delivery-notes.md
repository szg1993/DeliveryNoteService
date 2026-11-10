# US-002: Create delivery notes

- Status: In review
- Area: Delivery notes
- Implementation repository: DeliveryNoteService
- Implementation solution: `service/DeliveryNoteService.sln`
- Breakdown: [Implementation plan, confirmed refinements, and tasks](US-002-create-delivery-notes/README.md).
- Dependencies: Recipient creation from [US-001](US-001-create-and-view-recipients.md).
  Creator attribution and access control are deferred; no user-management dependency.
- Refinement: Developer confirmed shared understanding after grilling on 2026-11-10.

## User story

As a delivery-note user, I want to create a delivery note for an existing recipient
so that I can record the details needed to prepare a delivery.

## Confirmed scope

- Introduce the Note model and persist notes through a creation use case exposed
  by POST /api/delivery-notes.
- Create the complete legacy note: header, contact details, package details,
  deadlines, and at least one line, linked to an existing recipient.
- Include GET /api/delivery-notes/{id} and return complete saved details and
  current recipient details from both endpoints, using Envelope<T>.

The requirements below incorporate the developer's initial refinements and
subsequent grilling answers, confirmed on 2026-11-10. A three-task breakdown
exists; all coding tasks are complete and [agent review 02 passed](US-002-create-delivery-notes/review-02.md).
The subsequent developer-requested Guard/test cleanup requires follow-up review.
Human review and explicit acceptance remain. Legacy observations are historical
reference, not the new contract.

## Creation contract

- Retain legacy business field names and maximum lengths listed below. Trim
  text before length validation and storage, preserving internal whitespace.
  Optional EKAER/comments may be absent, null, or blank; blank becomes null.
  Require legacy-required text and nonblank free-text unit labels; no unit list
  or new category enumeration is introduced.
- Require ContactInfo, PackageDetail, Deadline, and a nonempty NoteLines
  collection without null entries. Preserve line order and distinct lines even
  when part codes repeat. No additional maximum line count is imposed.
- Use decimal quantities, weights, and dimensions without business-specific
  precision limits or silent rounding. Quantities must be positive; weights and
  dimensions must be nonnegative and explicitly supplied, including zero.
  Missing/null numeric values are invalid. Omitted handling flags are false.
- TakeoverDate, ShipDate, and EstimatedArrivalDate use DateOnly in request and
  response DTOs and the domain model. Accept and return exactly yyyy-MM-dd;
  reject timestamps, date-time strings, blanks, missing/null values, and invalid
  dates. Do not convert calendar dates to UTC instants. Require TakeoverDate <=
  ShipDate <= EstimatedArrivalDate; equal and past dates are allowed.
- The caller supplies RecipientId for an existing recipient. Nonpositive IDs
  are invalid input (400); unknown positive IDs return 404. Recipient data is a
  live relationship, not a historical snapshot.
- The server assigns note/child IDs, NoteNbr, UTC CreatedDate using ITimeService,
  and initial Pending status. No User relationship or placeholder creator is
  introduced. Both creation and full detail retrieval are anonymous; access
  control and creator attribution are deferred.
- Reject caller-supplied server fields, including nested IDs and legacy UserId.
  Reject every unknown property at every request level with 400. Retain
  case-insensitive property matching and apply these rules only to the new note
  contract.
- Use one permanent service-wide sequence N00001 through N99999, without reset
  or reuse. Successful concurrent creations have distinct numbers. Gaps are
  allowed and may exhaust the sequence before 99,999 notes have been saved.
  Exhaustion stops creation with 400 until a later feature changes the scheme.
- Each POST is a separate creation, even when content is identical. A retry
  after an uncertain response may create another note; idempotency is deferred.
- POST returns 201 and Location pointing to GET-by-ID, which returns 200. Both
  expose full note details, server fields, and current recipient details.
  Business-rule failures return 400; missing resources return 404. Use the
  existing error envelope and stable error codes.

## Legacy creation flow

The Minimal API maps POST /api/delivery-notes/ to NoteService.AddNoteAsync:

1. Accept NoteCreateDto and reject a null DTO in the service.
2. Map the supplied fields and nested objects to Note with AutoMapper.
3. Read the last note number and generate the next number.
4. Assign CreatedDate using DateTime.Now.
5. Add the note and save through the unit of work; report a bad-request exception
   if saving returns false.
6. Return HTTP 201 with a ResponseBase success message. The response does not
   expose the created note or its identifier. Location points to the collection.

### Data accepted by the legacy DTO

| Group | Legacy inputs |
| --- | --- |
| Note header | EkaerNbr, TakeoverPlace, Category, Comments |
| Flags | IsUrgent, IsCrackable, IsDangerous, IsOwnCost |
| References | RecipientId, UserId, NoteStatusId |
| ContactInfo | AssignTo, AssignToPhone, Contact, ContactPhone |
| PackageDetail | TareWgt, TareWgtUm, PkgQty, PkgScale, PkgSizeX/Y/Z, PkgSizeUm |
| Deadline | TakeoverDate, ShipDate, EstimatedArrivalDate |
| NoteLines | PartCode, PartDesc, PartCmt, PartQty, PartQtyUm, PartWgt, PartWgtUm |

Legacy nested DTOs also accept Id; the new contract rejects these inputs.
Legacy NoteLines defaults to an
empty list. The Note model references one recipient and one user, has contact,
package, and deadline navigation properties, and holds a collection of lines.

### Legacy constraints and gaps

- NoteNbr is server-generated: N00001 initially, then the numeric suffix is
  incremented. The implementation compares against 55555, so N55555 is the last
  generated number, despite a five-digit suffix allowing larger values. The
  existing test checks rejection at N99999, not the actual boundary.
- The repository uses LastOrDefaultAsync without explicit ordering to select the
  preceding number. The read-then-increment flow provides no concurrency-safe
  number allocation by itself.
- The creation service copies the caller's NoteStatusId. It does not force Pending
  or validate status there. Defined statuses are Pending, Approved, and Rejected.
- UserId and RecipientId come from the caller. The creation service does not
  explicitly check that either referenced record exists.
- Model annotations specify required TakeoverPlace (maximum 30) and Category
  (maximum 20), optional EkaerNbr (maximum 10), and Comments (maximum 200).
- Contact model annotations require both names (maximum 200 each) and both phone
  numbers (maximum 30 each). Line annotations require code (maximum 100),
  description (maximum 200), and quantity/weight units (maximum 10 each), with an
  optional comment (maximum 200). Package scale and size unit have maximum 10.
- These are model annotations, not an established request validation contract:
  NoteCreateDto has no validation attributes, and the inspected creation service
  does not validate field lengths, blank values, numeric ranges, units, or date
  ordering. Do not assume the Minimal API enforces model annotations on this DTO.
- CreatedDate uses local server time. CancellationToken is accepted by the route
  but is not passed to AddNoteAsync.

## Acceptance criteria

Apply the confirmed creation contract to every criterion.

1. Creating a note with valid, agreed details and an existing recipient persists
   it and returns HTTP 201 with its generated identifier and note number in the
   service's response envelope.
2. The server assigns the note identifier, note number, and UTC creation time.
   Initial status is always Pending.
3. Saved details can be observed through GET /api/delivery-notes/{id}; creation
   returns a Location header pointing to that resource. Retrieval preserves
   submitted line order and calendar dates without timezone conversion.
4. Invalid input is rejected using the service's error format and stable error
   codes, without saving a note or any of its new dependent records. This includes
   missing numeric values, invalid date formats, and unknown nested properties.
5. The complete creation, including nested details and lines, succeeds or
   fails together; partial notes are not left behind.
6. Successful concurrent creations receive different note numbers in the permanent
   N00001..N99999 sequence; gaps are allowed and exhaustion returns 400.
7. The generated OpenAPI document describes the agreed creation contract,
   responses, and detail endpoint, including anonymous access and strict inputs.

## Open questions

None blocking. The developer confirmed shared understanding after grilling.

## Agreed test boundary

The developer refined the test agreement on 2026-11-10: test DTO validation with
a few unit tests calling `.Validate()` directly, and keep API coverage to a small
representative set of complete workflows and essential HTTP behavior. This
supersedes the earlier HTTP-only validation agreement and exhaustive field matrices.
For HTTP tests, use the existing host and create prerequisite recipients through
their endpoint. Verify saved details through POST and GET-by-ID. Parsing remains
an HTTP concern, covered by representative cases rather than every numeric field.

Use TDD against the confirmed public boundary and product criteria. Routine
API verification uses SQLite. Testcontainers execution requires separate explicit
approval under repository instructions.

## Excluded scope

- Updating, deleting, listing, dashboard queries, and status transitions.
- Frontend changes, authentication/access control, creator attribution, and legacy
  data migration.
- Standalone note-line management endpoints.
- Request idempotency and numbering reset, reuse, or expansion beyond N99999.

## Legacy references

Paths below are relative to the workspace root.

- `CustomDeliveryNote/Backend/WebApp/RestApi/DeliveryNoteEndpoints.cs`
- `CustomDeliveryNote/Backend/BusinessLogicLibrary/Services/NoteService.cs`
- `CustomDeliveryNote/Backend/BusinessLogicLibrary/Mapper/NoteMappings.cs`
- `CustomDeliveryNote/Backend/TransferLibrary/Dtos/Note/NoteCreateDto.cs`
- `CustomDeliveryNote/Backend/DataAccessLibrary/Models/Note.cs`
- `CustomDeliveryNote/Backend/DataAccessLibrary/Models/NoteStatuses.cs`
- `CustomDeliveryNote/Backend/DataAccessLibrary/Repository/NoteRepository.cs`
- `CustomDeliveryNote/Backend/UnitTestLibrary/Services/NoteServiceTests.cs`
