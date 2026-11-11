# US-001: Create and view recipients

- Status: Done
- Merge: [PR #1](https://github.com/szg1993/DeliveryNoteService/pull/1), merged into
  master on 2026-11-10 at `025ec1cc157ab38b5cd4e5b8a133eec5f55839f9`.
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
9. All three operations are described in the generated OpenAPI document,
   including their status codes and the creation Location header. Detailed
   schema annotations (required fields, descriptions, named sort enums) are not
   required (accepted by the developer on 2026-11-11 after review 03, F007).
10. Creating recipients with the same name and different unique codes succeeds.
11. Creating a recipient with an existing normalized code is rejected and does not
    create a second recipient, including concurrent creation requests. Trim code
    whitespace and normalize to uppercase before comparing or storing codes. The
    response is HTTP 400 in the demo's error format with the stable error code
    `Recipient.AlreadyExists`, for both the sequential check and a concurrent
    database uniqueness conflict. (Changed from 409 on 2026-11-11.)
    (Narrowed by the developer on 2026-11-11, commit `f8de9b2`: only the
    sequential check returns 400 `Recipient.AlreadyExists`. A concurrent
    request that reaches the database uniqueness constraint returns 409 with
    the common `UniqueKeyConflict` error code and still creates no second
    recipient. Concurrent creation is no longer covered by an automated test.)
12. Listing defaults to page 1 with 10 results, ordered by ID ascending. Callers
    can sort by ID or name; name sorting uses ID as a deterministic tie-breaker.
13. Foreign-recipient status is derived from country: destinations in Hungary are
    domestic and destinations outside Hungary are foreign. Accept recognized
    two-letter country codes, normalize them to uppercase, and treat HU as Hungary.
14. Code, name, country, ZIP, city, and address are required: null or empty
    values are rejected. Name, ZIP, city, and address are validated and stored
    exactly as supplied, including surrounding whitespace, following the
    repository DTO rules; whitespace-only values are accepted. Code and country
    are identifiers: they are trimmed and uppercased, and a code that is empty
    after trimming is rejected. Maximum lengths apply to the supplied values: code
    10, name 200, ZIP 20, city 50, and address 200; country must be a valid
    two-letter country code. Preserve ZIP as text, including leading zeros.
    (Changed from trimming all fields and rejecting blanks on 2026-11-11.)
15. Use the demo's paging validation and paged response format, including limit=0
    to request all recipients.
16. Invalid fields, malformed IDs, and invalid paging or sorting return HTTP 400.
    Any well-formed ID that does not identify a recipient, including zero or a
    negative number, returns HTTP 404. Errors use the demo's response format with
    stable error codes. (Nonpositive IDs changed from 400 to 404 on 2026-11-11.)
    (Narrowed by the developer on 2026-11-11: the recipient list no longer has
    its own paging or sorting validation. Invalid page or limit values return
    400 only when recipients exist; on an empty list they return 200. Unknown
    numeric `SortBy` or `SortDir` values return 200, and the order is then not
    guaranteed.)
17. After trimming and uppercase normalization, codes are non-empty and contain
    only ASCII letters, digits, hyphens, and underscores. Embedded spaces and other
    characters are rejected with HTTP 400.
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
- All six input fields are required and subject to the limits in the acceptance
  criteria. Only code and country are normalized; other text is preserved as
  supplied (revised 2026-11-11). ID and UTC creation time are server-assigned.
- Use POST /api/recipients, GET /api/recipients, and GET /api/recipients/{id}.
  Follow the demo's response envelopes. Creation and read responses expose all
  recipient details, the ID, UTC creation time, and derived foreign status.
- Omit caching. Successful creation must be immediately visible through reads.
- Retain the demo's limit=0 option for an unpaged list.
- Use the demo's error response format and stable error codes, with HTTP 400 for
  invalid input and duplicate codes, and HTTP 404 for missing recipients
  (duplicate codes revised from 409 on 2026-11-11).
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
test infrastructure.

Revised 2026-11-11, following `service/test/CLAUDE.md`: DTO validation is tested
with unit tests that call `Validate()` directly, which is an agreed public test
seam. API tests are kept to a small set proving complete workflows and essential
HTTP behavior, including at least one rejected creation that leaves the recipient
list unchanged. Validation matrices are not repeated through HTTP.

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
record the task breakdown. T001 through T004 are done. The
[independent agent review](US-001-create-and-view-recipients/review-01.md) returned
PASS for implementation head `5f5b513d23bf5a733b691dc8e1f7188a1f7c5226`, with
115/115 API tests passing at that reviewed revision.

On 2026-11-10 the developer confirmed: "I merged the first user story". Merge
commit `025ec1cc157ab38b5cd4e5b8a133eec5f55839f9` contains the recipient work in
master. The implementation is available as the dependency for US-002.

Commits `fa1a3ba` (Story implemented.) and `99ae0fd` (Cleanup) changed implementation
and tests after the recorded agent review, including recipient validation and
error behavior. The existing PASS does not cover those changes. Formal completion
remains In review with the plan active until a follow-up review covers the merged
implementation and the human acceptance prerequisites are satisfied. This update
records the merge; it does not claim a new review or test execution.

[Agent review 02](US-001-create-and-view-recipients/review-02.md) returned FAIL
with incomplete coverage. On 2026-11-11 the developer resolved the conflicting
requirements: duplicate codes return 400 (AC11); nonpositive IDs return 404
(AC16); name, ZIP, city, and address are preserved as supplied while code and
country remain normalized identifiers (AC14, AC17); and DTO validation unit tests
replace the HTTP validation matrix (agreed test boundary). Corrections for the
remaining findings and a complete follow-up review are next.

[Agent review 03](US-001-create-and-view-recipients/review-03.md) returned FAIL
with complete coverage on F007 only: the generated OpenAPI document no longer
details the recipient schemas (no required fields or descriptions on
`CreateRecipientDto`, numeric sort enums, undescribed paging parameters, assembly
name as title), and `CreateRecipientDtoSchema` is unregistered. On 2026-11-11 the
developer accepted this reduced contract ("Accept reduced contract"). AC9 requires
that all three operations, their status codes, and the Location header are
described; detailed schema annotations are not required for US-001.

[Agent review 05](US-001-create-and-view-recipients/review-05.md) returned PASS
with complete coverage after the unused schema filter was removed. On 2026-11-11
the developer approved human review ("Human review approved");
[human acceptance 06](US-001-create-and-view-recipients/review-06.md) records
completion. The correction commits are on the story branch and are not yet merged.
