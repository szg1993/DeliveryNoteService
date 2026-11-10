# US-002: Create delivery notes

- Status: Draft
- Area: Delivery notes
- Implementation repository: DeliveryNoteService
- Implementation solution: `service/DeliveryNoteService.sln`
- Breakdown: [Implementation plan, confirmed refinements, and tasks](US-002-create-delivery-notes/README.md).
- Dependencies: Recipient creation from [US-001](US-001-create-and-view-recipients.md).
  User attribution remains unresolved; no user-management dependency is agreed.

## User story

As a delivery-note user, I want to create a delivery note for an existing recipient
so that I can record the details needed to prepare a delivery.

## Scope under refinement

- Introduce the Note model and persist notes through a creation use case exposed
  by POST /api/delivery-notes.
- Decide whether this increment includes the complete legacy note or only its
  header. Contact details, package details, deadlines, and lines are not yet
  confirmed scope.
- Define the creation response and a public way to verify saved details.

The user selected note creation as the next story and requested investigation of
the legacy POST endpoint. The proposals below are not an approved specification.
No task breakdown or implementation has started.

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

Nested DTOs also accept Id. Decide the new creation contract explicitly rather
than carrying these identifiers into it automatically. NoteLines defaults to an
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

## Proposed acceptance criteria

These criteria require refinement and developer confirmation.

1. Creating a note with valid, agreed details and an existing recipient persists
   it and returns HTTP 201 with its generated identifier and note number in the
   service's response envelope.
2. The server assigns the note identifier, note number, and UTC creation time.
   Initial status is Pending, subject to confirmation of the status policy.
3. Saved details can be observed through the agreed public API boundary. Decide
   whether to include GET /api/delivery-notes/{id}; if included, creation returns
   a Location header pointing to that resource.
4. Invalid input is rejected using the service's error format and stable error
   codes, without saving a note or any of its new dependent records. Exact
   validation rules and unknown-recipient response remain to be agreed.
5. If nested details and lines are included, the complete creation succeeds or
   fails together; partial notes are not left behind.
6. Successful concurrent creations receive different note numbers. Number format,
   upper bound, exhaustion response, and gap policy remain to be agreed.
7. The generated OpenAPI document describes the agreed creation contract,
   responses, and any detail endpoint included in this story.

## Open questions

1. Full legacy note creation or header only? If full, which nested details are
   mandatory, and are zero lines allowed?
2. Should all notes start Pending, with approval and rejection handled later?
3. How should the creator be represented before authentication and user management
   exist? The previous story's anonymous-access decision applies to recipients;
   note creation needs its own decision.
4. Keep N plus five digits? Is the legacy 55555 limit intentional? Are gaps in
   numbering acceptable after failures, and what happens when numbers run out?
5. Which legacy fields and limits remain? Clarify Category, TakeoverPlace,
   IsCrackable, IsOwnCost, AssignTo, TareWgt, and PkgScale before renaming them or
   prescribing new rules. Agree requiredness, trimming, numeric ranges, supported
   units, and whether deadlines are dates or instants with ordering constraints.
6. Include detail retrieval for verification? Agree creation/read response fields,
   unknown-recipient behavior, and whether recipient details are referenced live
   or captured as a snapshot when the note is created.

## Proposed test boundary

Use HTTP through the existing API test host, creating prerequisite recipients
through their endpoint. Verify creation responses and, if agreed, retrieve the
saved note through its detail endpoint. HTTP is proposed for this story; the
recipient story's agreement does not automatically confirm it here.

Use TDD after confirmation of the public boundary and product criteria. Routine
API verification uses SQLite. Testcontainers execution requires separate explicit
approval under repository instructions.

## Excluded scope proposed for this increment

- Updating, deleting, listing, dashboard queries, and status transitions.
- Frontend changes, authentication implementation, and legacy data migration.
- Standalone note-line management endpoints.

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
