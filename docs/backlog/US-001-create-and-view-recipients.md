# US-001: Create and view recipients

- Status: In progress
- Epic: [Recipient management](epics/recipient-management.md)
- Implementation repository: DeliveryNoteService
- Implementation solution: `service/DeliveryNoteService.sln`
- Dependencies: No predecessor stories identified.

## User story

As a delivery-note user, I want to create recipients, list them, and retrieve their
details so that I can register and select the correct recipient when preparing a
delivery note.

## Scope

- Create a recipient through the new backend API.
- List recipients through the new backend API.
- Retrieve a recipient by its identifier.
- Deliver the behavior through the API, application logic, and persistence.

## Acceptance criteria

1. Given valid recipient details, creating a recipient returns HTTP 201, the
   created recipient in the demo's envelope format, and a Location header pointing
   to its detail endpoint.
2. Given a successfully created recipient, retrieving it by the returned
   identifier returns HTTP 200 and the normalized details supplied during creation,
   together with the server-generated ID, UTC creation time, and derived foreign
   status.
3. Given a successfully created recipient, listing recipients includes it without
   requiring the caller to refresh a cache explicitly.
4. Given recipient details that violate the agreed validation rules, creation is
   rejected with HTTP 400 and no recipient is created.
5. Given stored recipients, listing recipients returns their identifiers and the
   agreed recipient details in a deterministic order.
6. Given no stored recipients, listing recipients succeeds with an empty
   collection.
7. Given an existing recipient identifier, retrieving it returns HTTP 200 and its
   agreed details.
8. Given a valid but unknown recipient identifier, retrieving it returns HTTP 404.
9. All three operations are described in the generated OpenAPI document.
10. Creating recipients with the same name and different unique codes succeeds.
11. Creating a recipient with an existing normalized code is rejected and does not
    create a second recipient, including concurrent creation requests. Trim code
    whitespace and normalize to uppercase before comparing or storing codes. The
    response is HTTP 409 in the demo's error format with a stable error code.
12. Listing defaults to page 1 with 10 results, ordered by ID ascending. Callers
    can sort by ID or name; name sorting uses ID as a deterministic tie-breaker.
13. Foreign-recipient status is derived from country: destinations in Hungary are
    domestic and destinations outside Hungary are foreign. Accept recognized
    two-letter country codes, normalize them to uppercase, and treat HU as Hungary.
14. Code, name, country, ZIP, city, and address are required and cannot be blank.
    Trim surrounding whitespace. Maximum lengths are code 10, name 200, ZIP 20,
    city 50, and address 200; country must be a valid two-letter country code.
    Preserve ZIP as text, including leading zeros.
15. Use the demo's paging validation and paged response format, including limit=0
    to request all recipients.
16. Invalid fields, malformed or nonpositive IDs, and invalid paging or sorting
    return HTTP 400. A valid positive ID that does not exist returns HTTP 404.
    Errors use the demo's response format with stable error codes.
17. After trimming and uppercase normalization, codes contain only ASCII letters,
    digits, hyphens, and underscores. Embedded spaces and other characters are
    rejected with HTTP 400.
18. Address fields and ZIP are validated for requiredness and maximum length only.
    No country-specific postal-format checks or address verification are required.

These criteria reflect the complete story confirmed by the developer on
2026-11-08.

## Technical constraints

- Use Minimal APIs following `service/src/WebApp/RestApi/DemoEndpoints.cs` and the
  target solution's application and persistence patterns.
- Redesign the contract without requiring compatibility with legacy clients.
- Use TDD: one failing behavior test followed by the implementation needed to pass
  it, then continue with the next behavior.

## Agreed decisions

- A recipient is a target destination and can be a person or a company; see
  [the glossary](../../CONTEXT.md).
- Start with a fresh database. Legacy data migration is outside this story.
- Creating and viewing recipients work without login. Authentication will be
  implemented later.
- Follow the demo's listing patterns where straightforward. Ask the developer if
  applying a pattern presents difficulties rather than silently diverging.
- Test use cases through HTTP, following the demo's API testing approach.
- Keep code, name, country, ZIP, city, and address as recipient details. The server
  assigns the ID and creation time.
- Recipient codes are entered manually and must be unique. Names may repeat.
- Derive foreign-recipient status from country rather than accepting a separate
  user-supplied flag.
- Follow the demo's pagination and sorting approach: default page 1, limit 10,
  sorting by ID or name, default ID ascending, and ID as a tie-breaker for names.
  Searching is deferred.
- Normalize codes by trimming and converting to uppercase. Enforce uniqueness
  under concurrent requests as well as sequential requests.
- Use recognized two-letter country codes, normalized to uppercase; HU is domestic.
- All six input fields are required, trimmed, and subject to the limits in the
  acceptance criteria. ID and UTC creation time are server-assigned.
- Use POST /api/recipients, GET /api/recipients, and GET /api/recipients/{id}.
  Follow the demo's response envelopes. Creation and read responses expose all
  recipient details, the ID, UTC creation time, and derived foreign status.
- Omit caching. Successful creation must be immediately visible through reads.
- Retain the demo's limit=0 option for an unpaged list.
- Use the demo's error response format and stable error codes, with HTTP 400 for
  invalid input, HTTP 404 for missing recipients, and HTTP 409 for duplicate codes.
  The demo middleware needs an adjustment to map uniqueness conflicts to HTTP 409.
- Restrict normalized codes to ASCII letters, digits, hyphens, and underscores.
- Retain all explicitly agreed validation rules. Add no country-specific ZIP
  checks or address verification; this does not override the agreed country-code
  and recipient-code rules.

## Agreed test boundary

Exercise the public HTTP endpoints and assert their externally visible responses.
For the create-and-view flow, create test recipients through the creation endpoint
and verify them through the detail and list endpoints. This flow should be
testable without manually seeded recipient records or direct database assertions.
The developer confirmed HTTP as the public test boundary. Follow the demo's API
test infrastructure; no additional test boundaries have been agreed.

## Open questions

None. The developer confirmed shared understanding of the complete story on
2026-11-08.

## Excluded scope

- Updating or deleting recipients.
- Recipient count and newest-recipient dashboard queries.
- Frontend changes and delivery-note functionality.
- Legacy data migration and authentication.
- Recipient searching.

## Progress

Refinement is complete and the developer confirmed the specification on
2026-11-08: "Yes, we have the common understanding."

The [implementation plan and tasks](US-001-create-and-view-recipients/README.md)
record the task breakdown. No implementation, tests, or review has started. This
confirmation approves the specification; it does not record acceptance of an
implementation.
