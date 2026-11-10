# T001: Create a complete delivery note

- Parent: [US-002](../README.md)
- Status: done
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
  length limits on supplied text, explicitly supplied decimal quantities/weights/dimensions,
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
  stable codes and 400; any absent recipient, including zero or negative IDs, uses 404. Describe no 409
  business response for this operation.
- Extend existing TestSuite and request factories; add note-specific infrastructure
  setup for the first-number scenario because the shared fixture seeds IDs above
  zero. Add a narrowly scoped Swagger-document HTTP test using existing hosting.
  No new test runner, CLI, or checked-in OpenAPI artifact is required.

GET and the creation Location header are introduced together in T002. This task
must be independently verifiable through POST and generated documentation; it
must not call an unimplemented GET route or advertise a URI that returns 404.

## Acceptance criteria

- [x] Valid full input creates a note and returns 201 with the standard envelope,
  all mapped details/current recipient details, and positive server-assigned IDs.
- [x] With the note identity reset in an isolated SQLite test, the first creation
  is N00001 and the next is N00002. Arbitrary initial fixture IDs are not treated
  as a production numbering rule. Input does not assign the generated fields.
- [x] The fixed clock controls UTC CreatedDate and status is always Pending;
  creation requires no authentication and stores no placeholder creator.
- [x] Every source length limit applies to text exactly as supplied, including
  surrounding whitespace. Optional EKAER/comments may be null or absent;
  supplied empty or blank strings are preserved.
  False flags and free-form nonblank unit labels are accepted.
- [x] Missing/null required objects or required text, whitespace-only text,
  oversized values, empty/null lines, and null line entries return standard 400
  errors. Validate each nested field rather than relying on EF annotations.
- [x] Positive decimal quantities, nonnegative decimal weights/dimensions, and
  fractional values are accepted. Nonpositive quantities and negative
  weights/dimensions are rejected with 400, without silently rounding. Every
  numeric field must be explicitly supplied; missing/null values return 400,
  while explicit zero weights/dimensions remain valid.
- [x] All deadline dates are required and ordered; equal/past dates are accepted,
  reversed dates are rejected. All three deadlines use DateOnly in DTOs/domain,
  accept/return exactly yyyy-MM-dd, and undergo no timezone conversion.
  Timestamps, date-time strings, blanks, missing/null values, and invalid dates
  return 400; the permissive shared converter must not bypass these rules.
- [x] Unknown properties, including caller-assigned server fields, at the note or
  any nested level are rejected with 400; known names match case-insensitively.
  Null bodies and malformed JSON fail as requests with the standard error
  format; existing recipient request behavior remains intact.
- [x] RecipientId is not validated by sign; any absent recipient, including zero
  or negative IDs, is 404. Business errors have stable codes grouped under Note when entity-specific.
- [x] Required relationships, identity uniqueness/range, and transactional save
  prevent partial persisted graphs. Number exhaustion is mapped specifically to
  400 rather than a generic uniqueness conflict or unhandled provider exception.
- [x] Two sequential identical valid POSTs each return 201 with distinct IDs and
  note numbers. No duplicate suppression or idempotent retry behavior is implied.
- [x] Generated OpenAPI describes required/nested inputs, numeric/date/text rules,
  rejection of server fields, anonymous access, 201/400/404, and response schemas.

## Verification

Use TDD one behavior at a time through the real HTTP host. Arrange recipients
through POST; use a full valid request factory and vary only the field under
test. Add table-driven field/range/date cases as useful, with clear assertions
for the standard error shape and relevant stable code. Do not add internal
service mocks or a second unit-test copy of the same DTO matrix.

Exercise successful responses for optional fields, flags, one/multiple lines,
repeated part codes, decimals, and exact supplied text, along with the rejection cases.
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

- 2026-11-10: Implemented anonymous POST creation through the agreed HTTP seam, with full note/contact/package/deadline/line graph, existing recipient reference, DTO-local validation/normalization/mapping, UTC clock, Pending status, and complete response envelope. No Location header is advertised until T002 implements GET.
- TDD evidence: first HTTP tracer failed with 404 before the route existed, then passed with 201 and normalized full details/server fields. Documentation tracer failed because required schema constraints were absent, then passed after the scoped schema filter. Added field/date/numeric/unknown-property rejection cases, text boundary acceptance, exact decimal/line-order/flags scenarios, malformed bodies, missing recipient, and repeated creation. Basic exhaustion check initially exposed a no-op test sequence UPDATE; note-only setup now inserts an absent sqlite_sequence row before updating, and the request returns Note.NumberExhausted/400.
- Persistence: one SaveChangesAsync persists the entire graph under EF's automatic transaction. Named CK_Notes_NumberRange enforces 1..99999; only this check violation is translated at the persistence boundary for SQLite and PostgreSQL. NoteNbr derives from the immutable generated ID; no separate counter, process lock or mutable number string. PostgreSQL decimal columns are unconstrained numeric; SQLite migration uses TEXT matching EF decimal storage to avoid numeric-affinity rounding. Dates remain DateOnly/date.
- Migration inspection found pre-existing snapshot drift: JobExecution was still recorded although its CLR/runtime mapping had already been removed. The scaffold attempted to drop JobExecutions. Removed that unrelated operation and retained its prior metadata in the new designer/snapshot, preserving the existing table and keeping this migration additive. Runtime behavior is unchanged by that preservation; a future migration author must account for this existing drift rather than accepting another unrelated drop. The Recipient CLR namespace is now reflected as Core without changing the existing table contract.
- Refactoring assessment (production and tests): centralized text limits in NoteLimits across validation, EF mapping and OpenAPI; shared NoteInput owns only validation/normalization primitives, and each DTO owns mapping. Reviewed logger/clock/cancellation, complete response projections and named provider-error filtering. Clean-code assessment: 9/10; schema metadata remains a larger declarative method, with further rule grouping useful only if that contract grows. No speculative abstraction or unrelated cleanup was justified. Followed implement-task's required pre-verification refactoring despite the tdd skill's generic suggestion to defer it to review.
- Reuse assessment: inspected RecipientCreationTests, RecipientDetailsTests, RecipientConcurrencyTests and the existing TestSuite. Extended DeliveryNoteServiceApi with Notes(), composed NoteRequests and note-only identity setup around the existing host, and reused recipient setup. Each test retains its own database/clock/client; DeliveryNoteServiceApiTestBase remains the sole host/database/client disposal owner. Documentation configuration is set before startup; identity setup runs after initialization and before requests. No new harness or internal mocks. Scenario-specific data stays in tests; capability helpers remove JSON/protocol/setup mechanics.
- Verification commands (repository root): dotnet test service/test/ApiTests/ApiTests.csproj --filter "FullyQualifiedName~Note&Database!=PostgreSQL" -v minimal: 154 passed. dotnet test service/test/ApiTests/ApiTests.csproj --filter "Database!=PostgreSQL" -v minimal -- xUnit.ParallelizeTestCollections=false: 210 passed. dotnet test service/DeliveryNoteService.sln --filter "Database!=PostgreSQL" -v minimal -- xUnit.ParallelizeTestCollections=false: 210 API and 171 unit tests passed. Scoped dotnet format service/DeliveryNoteService.sln --include <all task C# files> --no-restore and the same command with --verify-no-changes passed. No Markdown formatter is configured; Markdown follows the existing repository layout. Final whitespace-only migration alignment was followed by scoped formatter verification and focused note checks.
- Tooling: SDK 10.0.302 targets the existing net8.0 projects; installed dotnet-ef 8.0.10 only under ignored .git/tools for migration scaffolding. No tool manifest, production dependency, or runtime upgrade was added.
- Skills used: tdd (HTTP red/green behavior), clean-code (implementation and scoped refactoring), component-testing (required by .NET testing policy; real HTTP host/persistence with SQLite and deterministic external substitutes). No loaded-but-unused skills.
- Limits/remaining work: SQLite does not verify PostgreSQL identity/locking/migration/constraint-error semantics; production migration and exact named translation were inspected, and no Testcontainers were started. Durable GET readback and Location belong to T002; concurrent/final-number/rollback workflows belong to T003. Story remains In progress and plan active; human acceptance has not occurred.
- 2026-11-10, review-01 correction attempt 1 (F001/F002/F003): added note-property-local ExactNoteDecimalConverter to all seven package/line numeric inputs. It compares normalized base-10 digits, exponent and sign against the parsed decimal, rejecting any changed mathematical value through the standard InvalidHttpRequest/400 envelope. Equivalent exponent/trailing-zero forms and signed zero remain valid; missing/null numeric validation is preserved. BigInteger exponent normalization prevents exponent overflow without adding a business precision cap. No global JSON option or recipient contract changed.
- F001 red/green evidence: a raw JSON negative weight of -0.00000000000000000000000000001 first returned 201/zero; after the converter it returned 400 and GET of the candidate ID returned Note.NotFound. The expanded HTTP matrix covers all seven fields: rounding, positive/negative underflow, overflow, extreme exponents, exact 28-place values, 1e-28 and 10e-29, maximum decimal, exponent/trailing-zero equivalence, and valid zero forms. All 113 raw numeric cases pass, with fresh GET verifying rejection absence or exact saved values.
- F002 red/green evidence: the generated-document tracer first failed because format was double; it now verifies format decimal and an exact-base-10 description for every numeric request/detail property (14 fields across four schemas). Required/nullability/range metadata remains covered by existing documentation tests. The filter is limited to note DTO families.
- F003: normalized covered wrapped invocation/parameter lists throughout story-added/modified C# files, including the additive migration, designer, changed snapshot metadata and raw-string calls. A temporary ignored Roslyn utility checked covered list layout. Syntax-token comparison confirmed layout-only migration/designer/snapshot and affected HTTP/SQL test code were unchanged, normalizing only raw literal line endings for that comparison. Unchanged baseline snapshot sections were preserved; no migration operation changed.
- Correction refactoring/reuse assessment: one converter owns exact numeric binding and normalization; DTO attributes select its scope, and NoteContractSchema.DescribeDecimals owns note decimal metadata. Inspected existing creation/detail/documentation tests and NoteRequests/NoteAssertions consumers. Reused the existing host, per-test database/clock, JSON path helpers and API clients; PostRawNumberAsync removes repeated raw-body transport mechanics while inputs/expected literals remain visible in NoteDecimalTests. That helper owns/disposes request content, tests own raw responses, and the existing test base owns host/client/database disposal. Configuration timing and isolation remain unchanged. Clean-code assessment: 10/10 for this correction scope; no additional abstraction or unrelated cleanup was justified. Applied implement-task's required refactoring before final checks despite tdd's generic deferral guidance.
- Correction verification (repository root): dotnet test service/test/ApiTests/ApiTests.csproj --filter "(FullyQualifiedName~NoteDecimalTests|FullyQualifiedName~NoteDocumentationTests)&Database!=PostgreSQL" -v minimal: 116 passed. dotnet test service/test/ApiTests/ApiTests.csproj --filter "FullyQualifiedName~Note&Database!=PostgreSQL" -v minimal: 285 passed after final C# formatting. dotnet test service/DeliveryNoteService.sln --filter "Database!=PostgreSQL" -v minimal -- xUnit.ParallelizeTestCollections=false: 342 API and 171 unit tests passed. Scoped dotnet format service/DeliveryNoteService.sln --include service/src/Core/UseCases/Notes/CreateNoteLineDto.cs service/src/Core/UseCases/Notes/CreatePackageDetailDto.cs service/src/Persistence/Migrations/20261110131638_AddDeliveryNotes.Designer.cs service/src/Persistence/Migrations/20261110131638_AddDeliveryNotes.cs service/src/Persistence/Migrations/AppDbContextModelSnapshot.cs service/src/WebApp/RestApi/NoteContractSchema.cs service/test/ApiTests/NoteDetailsTests.cs service/test/ApiTests/NoteDocumentationTests.cs service/test/ApiTests/TestSuite/NoteDatabase.cs service/test/ApiTests/TestSuite/NoteRequests.cs service/src/Core/UseCases/Notes/ExactNoteDecimalConverter.cs service/test/ApiTests/NoteDecimalTests.cs --no-restore and its --verify-no-changes pass succeeded; git diff --check passed. No Markdown formatter is configured; records follow existing conventions.
- Correction skills used: tdd (HTTP numeric and schema red/green), clean-code (implementation/refactoring), component-testing (real HTTP host/persistence and reusable capability mechanics). No loaded-but-unused skills. No PostgreSQL tests or Testcontainers ran, and the existing PostgreSQL fidelity/snapshot-drift limits remain. F001/F002/F003 each have one attempted correction; no follow-up verdict or unsuccessful-fix count is inferred. Story stays In review, plan active; independent follow-up review is required.
- 2026-11-10, developer-authorized targeted cleanup after review-02: adopted the confirmed hobby-project agreement in AGENTS.md. Replaced NoteInput with existing Guard required/length/collection checks on trimmed values, straightforward numeric/date/order conditions, and DTO-local blank-to-null mapping. No shared Guard API or replacement validation helper was added. CreateNote translates Guard's FieldMissingException/FieldTooLongException once to retain the existing HTTP Note.InvalidInput code/envelope; direct DTO validation uses the existing Guard exceptions. Custom parsing, strict request binding, schemas and business rules retain their agreed behavior.
- Testing refinement explicitly supersedes the old HTTP-only seam and exhaustive per-field matrices: eight representative unit cases call DTO .Validate() directly, while note API coverage falls from 285 cases to 24 complete-workflow, HTTP binding/error, numbering and documentation cases. Numeric parsing now has three representative rejection and two exact-value HTTP cases. No matrix was merely moved into unit tests. Existing full readback and repeated-creation workflows absorb overlapping happy-path tests; dead NoteRequests.Remove was removed.
- Refactoring/reuse assessment: inspected existing Guard/StringExtensions, recipient validation/unit tests, note DTOs and creation/detail/decimal/numbering/documentation tests. Reused Guard and the existing HTTP host/clients/AssertSavedNote helper. The eight unit cases share one local valid-note factory; it constructs DTOs without host, serialization, mocks or new framework infrastructure. Each API scenario retains the existing per-test database/clock and host/client disposal owner; tests dispose their raw responses. No startup configuration or lifecycle change was needed. Further abstraction, generic numeric guards, or whole-suite cleanup would add maintenance cost without a current benefit.
- Verification: characterization tests were initially green (11 representative Validate cases before/after the refactor); reduced the final selection to eight distinct cases in line with the agreement. This behavior-preserving cleanup needed no manufactured red failure. After the last C# edit, scoped dotnet format service/DeliveryNoteService.sln --include (the eleven added/modified C# files) --no-restore and its --verify-no-changes pass succeeded. Final dotnet test service/DeliveryNoteService.sln --filter "Database!=PostgreSQL" -v minimal -- xUnit.ParallelizeTestCollections=false passed 81 API tests and 179 unit tests (previous suite: 342 API and 171 unit). git diff --check passed. No Markdown formatter is configured; tracking follows existing conventions. No PostgreSQL tests or containers ran.
- Skills: clean-code used for reuse and the scoped refactoring assessment; component-testing used for the proportionate HTTP/unit split and existing host lifecycle. tdd loaded but not used for a red/green behavior change because this cleanup preserves behavior; its agreed-seam guidance informed the direct Validate characterization checks. Latest user/repository testing preferences take precedence over broader historical test advice. Task remains done, story In review and plan active; fresh follow-up review is pending. F001/F002/F003 retain their prior resolved history; this is an authorized cleanup, not another failed correction attempt.
- 2026-11-10, developer-requested text/ID refinement: replaced the earlier trim-and-normalize contract with exact supplied string values. Header, contact, package and line text maps directly, including optional empty/blank strings; omitted/null optional values remain null. Length checks use raw values, with straightforward nonblank required-text checks and existing Guard length checks. RecipientId has no zero/negative guard and follows normal repository lookup, returning Recipient.NotFound/404 when absent. AGENTS.md and current story/plan/task requirements plus OpenAPI descriptions/minimum metadata record this explicit override. Historical progress and reviews remain intact.
- TDD: the existing full POST-to-GET graph test first failed because returned values lost surrounding whitespace and a blank optional line comment became null; after direct mapping it passed. Added zero/negative cases to the existing rejected-creation workflow; both first failed with 400, then passed with 404 after removing the sign guard. Representative direct Validate cases cover raw length (including surrounding spaces), nonblank required text and ID sign acceptance. No field matrix or new test seam was introduced.
- Refactoring/reuse assessment: inspected the four changed note DTOs, CreateNote error adaptation, NoteCreationTests, NoteDetailsTests, NoteDocumentationTests, direct DTO unit tests and existing NoteRequests/NoteAssertions consumers. Kept Guard for supported checks; used simple whitespace conditions without rewriting strings or adding a validation abstraction. Removed redundant explicit field-name arguments when Guard can infer raw property names. Reused the complete-graph and rejection workflows, per-test host/database/clock and existing disposal ownership; no new helper, mutable shared state or fixture configuration was needed. Exact numeric token parsing is unrelated to business string trimming and was not modified. Existing recipient creation behavior and server number-range enforcement remain outside this change.
- Verification (repository root): dotnet test service/test/ApiTests/ApiTests.csproj --filter "FullyQualifiedName~RetrievedNote_PreservesCompleteGraphAndOrderedDistinctLines&Database!=PostgreSQL" -v minimal passed after the red tracer. The same command with filter "FullyQualifiedName~RejectedCreation_LeavesNoRetrievableNote&Database!=PostgreSQL" passed 5 cases after its two red ID cases. Final dotnet test service/DeliveryNoteService.sln --filter "Database!=PostgreSQL" -v minimal -- xUnit.ParallelizeTestCollections=false passed 83 SQLite API tests and 182 unit tests. Scoped dotnet format service/DeliveryNoteService.sln --include <the ten changed C# files> --no-restore and the same command with --verify-no-changes passed after the final C# edit; git diff --check passed. No Markdown formatter is configured; followed repository conventions. No PostgreSQL tests or Testcontainers ran.
- Skills used: tdd (raw-text and ID red/green slices), clean-code (direct mapping, Guard reuse and scoped refactoring), component-testing (existing HTTP workflow/host alongside the agreed direct Validate unit seam). No loaded-but-unused skills. All tasks remain done, story In review and plan active. Fresh independent follow-up review must cover the new text/ID behavior before human acceptance; no push occurred.
