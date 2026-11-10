# T001: Create a complete delivery note

- Parent: [US-002](../README.md)
- Status: todo
- Depends on: US-001 recipient creation (verified in published master)
- Increment: Skeleton

## Outcome

A caller can create a valid complete note anonymously through POST and receive
its server-generated identity, number, Pending status, UTC creation time, agreed
details, and current recipient details in a 201 envelope.

## Scope

- Read the confirmed source story and the parent's refinements together. Target
  service/src/Core/Models and
  Core/UseCases/Notes, Persistence, and WebApp/RestApi in the existing solution.
- Add Note, its contact/package/deadline/line graph, limits, initial status, and
  DTOs. Include all legacy business inputs from the source, including optional
  EKAER/comments and the four handling flags. The request accepts RecipientId,
  not new recipient data or a user/status choice.
- Implement the complete confirmed validation and mapping in DTOs from the start.
  This includes required nested objects and text, at least one nonnull line,
  trimmed length limits, explicitly supplied decimal quantities/weights/dimensions,
  and required DateOnly calendar dates in DTOs/domain with exact yyyy-MM-dd
  input/output and their agreed ordering. Reject omitted/null numeric values
  rather than defaulting to zero. Preserve fractional values, internal text
  whitespace, line sequence, and distinct lines even when part codes repeat.
- Reject every unknown property, including note/nested server IDs, note number,
  status, creation time, and legacy UserId supplied in JSON. Retain case-insensitive
  matching of known properties. Apply this only to the new creation DTO family.
  Preserve the existing JSON contract of other capabilities.
- Add an additive PostgreSQL migration, model snapshot, and SQLite-compatible
  mapping. Configure required relationships, database-generated IDs, and one
  atomic graph save. Do not add a User foreign key, migrate legacy data, or change
  recipient behavior.
- Derive NoteNbr from the database-generated Note.Id as specified by the parent.
  Add the named 1..99999 database range constraint and translate only its violation
  to Note.NumberExhausted/400 at the persistence boundary. Implement this invariant
  now, not in T003. Do not compute the next number by querying existing notes.
  The sequence is permanent without reset or reuse; permitted gaps consume its
  finite capacity. Exhaustion stops creation until a later feature changes it.
- Treat every POST as a separate creation, including identical requests and
  retries after uncertain responses. Do not introduce idempotency handling.
- Follow CreateRecipient's logging, DTO mapping, repository/unit-of-work, and
  response conventions; inject the existing clock. Explicitly bind API parameters
  and pass CancellationToken through queries and persistence. Return current
  recipient data using RecipientDetailsDto after verifying RecipientId exists.
- Expose POST /api/delivery-notes and its generated OpenAPI operation. Return
  Envelope<NoteDetailsDto>, using legacy note names for response fields and
  server-assigned child IDs where represented. Business validation failures use
  stable codes and 400; an unknown positive recipient uses 404. Describe no 409
  business response for this operation.
- Extend existing TestSuite and request factories; add note-specific infrastructure
  setup for the first-number scenario because the shared fixture seeds IDs above
  zero. Add a narrowly scoped Swagger-document HTTP test using existing hosting.
  No new test runner, CLI, or checked-in OpenAPI artifact is required.

GET and the creation Location header are introduced together in T002. This task
must be independently verifiable through POST and generated documentation; it
must not call an unimplemented GET route or advertise a URI that returns 404.

## Acceptance criteria

- [ ] Valid full input creates a note and returns 201 with the standard envelope,
  all mapped details/current recipient details, and positive server-assigned IDs.
- [ ] With the note identity reset in an isolated SQLite test, the first creation
  is N00001 and the next is N00002. Arbitrary initial fixture IDs are not treated
  as a production numbering rule. Input does not assign the generated fields.
- [ ] The fixed clock controls UTC CreatedDate and status is always Pending;
  creation requires no authentication and stores no placeholder creator.
- [ ] Text normalization and every source length limit apply after trimming.
  Optional EKAER/comments may be null, absent, or blank; blank becomes null.
  False flags and free-form nonblank unit labels are accepted.
- [ ] Missing/null required objects or required text, whitespace-only text,
  oversized values, empty/null lines, and null line entries return standard 400
  errors. Validate each nested field rather than relying on EF annotations.
- [ ] Positive decimal quantities, nonnegative decimal weights/dimensions, and
  fractional values are accepted. Nonpositive quantities and negative
  weights/dimensions are rejected with 400, without silently rounding. Every
  numeric field must be explicitly supplied; missing/null values return 400,
  while explicit zero weights/dimensions remain valid.
- [ ] All deadline dates are required and ordered; equal/past dates are accepted,
  reversed dates are rejected. All three deadlines use DateOnly in DTOs/domain,
  accept/return exactly yyyy-MM-dd, and undergo no timezone conversion.
  Timestamps, date-time strings, blanks, missing/null values, and invalid dates
  return 400; the permissive shared converter must not bypass these rules.
- [ ] Unknown properties, including caller-assigned server fields, at the note or
  any nested level are rejected with 400; known names match case-insensitively.
  Null bodies and malformed JSON fail as requests with the standard error
  format; existing recipient request behavior remains intact.
- [ ] Invalid/nonpositive RecipientId is 400; an unknown positive recipient is
  404. Business errors have stable codes grouped under Note when entity-specific.
- [ ] Required relationships, identity uniqueness/range, and transactional save
  prevent partial persisted graphs. Number exhaustion is mapped specifically to
  400 rather than a generic uniqueness conflict or unhandled provider exception.
- [ ] Two sequential identical valid POSTs each return 201 with distinct IDs and
  note numbers. No duplicate suppression or idempotent retry behavior is implied.
- [ ] Generated OpenAPI describes required/nested inputs, numeric/date/text rules,
  rejection of server fields, anonymous access, 201/400/404, and response schemas.

## Verification

Use TDD one behavior at a time through the real HTTP host. Arrange recipients
through POST; use a full valid request factory and vary only the field under
test. Add table-driven field/range/date cases as useful, with clear assertions
for the standard error shape and relevant stable code. Do not add internal
service mocks or a second unit-test copy of the same DTO matrix.

Exercise successful responses for optional fields, flags, one/multiple lines,
repeated part codes, decimals, and normalization, along with the rejection cases.
T002 will add durable GET readback; these tests must pass without it. Review the
transaction/migration invariants here; T003 supplies additional contention and
end-of-range failure evidence, not their initial implementation.

Inspect the HTTP-generated Swagger document using configuration set before host
startup. Run the focused Note creation/documentation tests and existing recipient
regressions with --filter "Database!=PostgreSQL". Scope dotnet format and its
--verify-no-changes pass to this task's changed C# files, including migration and
snapshot files, after the last edit. Record observed red/green results and exact
commands here. Never start Testcontainers without explicit approval.

## Progress

No implementation started. Record dated work, verification evidence, and remaining
work here during implementation.
