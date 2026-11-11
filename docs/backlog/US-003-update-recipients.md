# US-003: Update recipients

- Status: In progress
- Breakdown: [Implementation plan and tasks](US-003-update-recipients/README.md).
- Epic: [Recipient management](epics/recipient-management.md)
- Implementation repository: DeliveryNoteService
- Implementation solution: `service/DeliveryNoteService.sln`
- Dependencies: Recipient creation and retrieval from
  [US-001](US-001-create-and-view-recipients.md). Delivery-note retrieval from
  [US-002](US-002-create-delivery-notes.md) shows current recipient details.
- Refinement: Open questions answered by the developer on 2026-11-11; all
  recommended options were accepted.

## User story

As a delivery-note user, I want to correct an existing recipient's details so that
new and existing delivery notes point to an accurate destination.

## Scope

- Replace the editable details of an existing recipient through
  `PUT /api/recipients/{id}`.
- Deliver the behavior through the API, an `UpdateRecipient` use case, and
  persistence, following the `CreateRecipient` patterns and logging.
- Return the updated recipient so the caller does not need a follow-up read.

## Acceptance criteria

1. Given an existing recipient and valid details, updating it returns HTTP 200
   with the updated recipient in the `Envelope<RecipientDetailsDto>` format.
2. After a successful update, retrieving the recipient by ID and listing
   recipients return the new details immediately.
3. The update replaces name, country, ZIP, city, and address. All five fields are
   required, as in creation (full replacement, not a partial patch).
4. The update applies the US-001 field rules unchanged: name, ZIP, city, and
   address are validated and stored exactly as supplied, with the same maximum
   lengths; country is trimmed, uppercased, and must be a recognized two-letter
   code.
5. Foreign status is re-derived from the new country (HU is domestic). A caller
   cannot supply foreign status directly.
6. The recipient code, ID, and UTC creation time are not changed by an update.
   A `Code` or other unknown property in the request body is ignored by normal
   JSON binding.
7. Invalid details are rejected with HTTP 400 in the existing error format and
   stable error codes, and the stored recipient remains unchanged.
8. A malformed route ID returns HTTP 400. A well-formed ID with no matching
   recipient, including zero or negative values, returns HTTP 404 with
   `Recipient.NotFound`.
9. Retrieving a delivery note that references the updated recipient shows the
   updated recipient details (live relationship agreed in US-002).
10. The endpoint is described in the generated OpenAPI document with its status
    codes. Detailed schema annotations follow the reduced contract accepted for
    US-001.

## Agreed decisions

Confirmed by the developer on 2026-11-11.

- The recipient code is immutable, as in legacy. The update contract has no
  `Code`; a mistyped code is handled by creating a new recipient (deletion is a
  later story).
- The ID comes only from the route. The request body has no `Id`, so the legacy
  route/body mismatch error disappears.
- Normal JSON binding: unknown properties, including a supplied `Code`, are
  ignored rather than rejected, as for recipient creation.
- `PUT` performs a full replacement of the five editable fields. No `PATCH`.
- Last write wins for concurrent updates. No optimistic concurrency (ETag or
  version field).

## Implementation defaults

Routine choices following existing conventions; not separately negotiated.

- Validate the body before looking up the recipient, matching the use case
  order (validation, lookup, mapping, persistence, response). An invalid body
  for an unknown ID returns 400.
- No `UpdatedAt` timestamp or change history.
- Anonymous access, as for the other recipient endpoints.
- No caching; updates are immediately visible.

## Agreed test boundary

Follow the US-001 revision and `service/test/CLAUDE.md`:

- Unit tests call the update DTO's `Validate()` directly for field rules. Reuse
  the creation rules rather than duplicating the full validation matrix.
- A small set of HTTP tests: successful update observed through GET-by-ID, a
  rejected update leaving the recipient unchanged, 404 for an unknown ID, code and
  creation time preserved (including when a `Code` is supplied), and one test
  showing a delivery note with the updated recipient details (AC9).

## Legacy behavior

Reference only; the new contract is redesigned (see the backlog README).

`PUT /api/recipients/{id}` maps to `RecipientService.UpdateRecipientAsync`:

1. Accept `RecipientUpdateDto` with `Id`, `Name`, `Country`, `Zip`, `City`,
   `Address`, and `IsOutlander`. `Code` is not part of the update DTO, so the code
   cannot be changed.
2. Reject a null DTO or a body `Id` that differs from the route ID with a
   bad-request error.
3. Load the recipient; a missing recipient returns not found.
4. AutoMapper copies the DTO onto the existing entity, including the
   caller-supplied `IsOutlander` flag. `CreatedDate` is kept.
5. Save through the unit of work; a failed save returns bad request.
6. Return HTTP 201 Created with a `ResponseBase` success message and no recipient
   data. The controller version sets Location to the collection.

Gaps: no field validation (lengths, requiredness, country) on update, the
foreign flag can contradict the country, 201 is the wrong status for an update,
and the recipient cache is not refreshed after the update.

## Open questions

None. All refinement questions were answered on 2026-11-11.

## Progress

Refinement is complete. On 2026-11-11 the developer confirmed the complete
story ("Yes, confirmed"). The same day the developer requested task breakdown;
the story is reserved on `feature/US-003-update-recipients` and the
[implementation plan](US-003-update-recipients/README.md) defines two tasks
(T001 and T002), both todo.

## Excluded scope

- Deleting recipients and recipient dashboard queries.
- Changing a recipient's code.
- Partial updates and optimistic concurrency.
- Historical snapshots of recipient details on delivery notes.
- Frontend changes, authentication, audit history, and legacy data migration.

## Legacy references

Paths below are relative to the workspace root.

- `CustomDeliveryNote/Backend/WebApp/RestApi/RecipientEndpoints.cs`
- `CustomDeliveryNote/Backend/API/Controllers/RecipientsController.cs`
- `CustomDeliveryNote/Backend/BusinessLogicLibrary/Services/RecipientService.cs`
- `CustomDeliveryNote/Backend/TransferLibrary/Dtos/Recipient/RecipientUpdateDto.cs`
- `CustomDeliveryNote/Backend/BusinessLogicLibrary/Mapper/RecipientMappings.cs`
