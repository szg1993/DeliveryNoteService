# Recipient management

- Status: Draft
- Implementation repository: DeliveryNoteService

## Goal

Allow delivery-note users to find and maintain recipients in the redesigned
backend, preserving agreed business rules from the legacy implementation.

## Initial story

- [US-001: Create and view recipients](../US-001-create-and-view-recipients.md):
  create recipients, list them, and retrieve a recipient's details by ID. Merged
  into master in [PR #1](https://github.com/szg1993/DeliveryNoteService/pull/1) on
  2026-11-10; formal closure is pending the follow-up review recorded in the story.

## Candidate stories to refine

- Update a recipient, including validation and missing-recipient behavior.
- Delete a recipient, including the existing restriction on recipients with
  delivery notes.
- View recipient dashboard information, including total count and newest
  recipients. Decide whether these are separate stories during refinement.

These candidates are not approved specifications or implementation tasks.

## Dependencies and decisions

- Deletion requires a way to determine whether delivery notes reference a
  recipient. Resolve that dependency before making the deletion story ready.
- Decide which legacy behavior to retain deliberately, including name matching,
  list ordering, and cache freshness. Existing implementation details do not
  automatically become requirements.
- Data migration and deployment or cutover requirements remain undecided.

## Legacy references

- `CustomDeliveryNote/Backend/API/Controllers/RecipientsController.cs`
- `CustomDeliveryNote/Backend/WebApp/RestApi/RecipientEndpoints.cs`
- `CustomDeliveryNote/Backend/BusinessLogicLibrary/Services/RecipientService.cs`
- `CustomDeliveryNote/Backend/BusinessLogicLibrary/Cache/RecipientCache.cs`
