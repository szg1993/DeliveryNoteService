# T002: Retrieve a saved delivery note

- Parent: [US-002](../README.md)
- Status: done
- Depends on: T001
- Increment: Extension

## Outcome

A caller can follow the creation response's Location URI to retrieve all saved
note details and current recipient details through GET.

## Scope

- Add GetNote under service/src/Core/UseCases/Notes and
  GET /api/delivery-notes/{id} in the capability route module. Reuse the T001
  aggregate/response DTO, existing repository query facilities, standard envelope,
  TechnicalUseCaseLogger<T>, and cancellation conventions.
- Load the note with contact/package/deadline/line records and its current
  recipient; do not return unloaded navigation properties or build a historical
  recipient snapshot. Preserve submitted line order using explicit persisted
  order if T001's mapping does not already guarantee it.
- Add the Location header to POST, pointing to the new GET resource. Describe it
  in POST's OpenAPI metadata in the same change.
- Add detail errors and complete the POST-to-GET workflow in the existing HTTP
  test language. Do not add listing, update, status transitions, user attribution,
  recipient mutation, or child routes to make a test possible.

## Acceptance criteria

- [x] Following Location after successful creation returns 200 with the expected
  standard envelope and the same note ID, number, Pending status, and creation
  timestamp.
- [x] A new GET request returns the exact supplied, persisted header/flags, complete
  contact/package/deadline details, and every line, including repeated part codes,
  optional text, decimal values, server child IDs, and original line order.
- [x] All three deadlines round-trip as the same yyyy-MM-dd calendar dates using
  DateOnly response properties, without offsets or timezone conversion.
- [x] Identical valid POSTs create separate notes; both can be retrieved through
  their distinct Location URIs with their own IDs/numbers and complete details.
- [x] GET exposes the referenced recipient's current details using the existing
  recipient DTO. No recipient snapshot is persisted on the note. Reference
  loading is verified using the recipient created through HTTP; no recipient
  update endpoint is required or added by this task.
- [x] Unknown integer note IDs, including zero and negative values, return 404
  with a stable Note error code. Malformed/out-of-range integer route input
  returns 400 using the current HTTP error convention.
- [x] Creation remains anonymous and GET is anonymous. Reads do not change the
  stored note or assign new identifiers/timestamps. Anonymous GET returns full
  contact/current recipient details; access control is explicitly deferred.
- [x] For representative failed validation and missing-recipient requests in a
  fresh identity-controlled test, GET confirms that no note was created at the
  next candidate ID before any subsequent valid creation is made.
- [x] Generated OpenAPI documents GET's route parameter, response fields,
  authentication, 200/400/404, and POST's new Location response header.

## Verification

Use TDD at the agreed HTTP boundary: create the recipient, POST a distinctive full
note, then GET via Location and compare the complete saved details. Include
multiple lines whose values and order are deliberately different, positive
fractional quantities, nondefault handling flags, optional text, and equal/past
dates. Verify exact optional text preservation and UTC timestamp consistency
through a fresh HTTP request rather than a tracked EF entity.

Exercise missing/malformed identifiers and rejected-creation readback. Inspect
the live generated OpenAPI in a test host with Swagger enabled before startup.
Run focused creation/detail/documentation and recipient tests, excluding
Database=PostgreSQL. Format this task's changed C# files and verify formatting
after the last edit. Record exact commands and red/green evidence. No container
execution is part of routine verification.

## Progress

- 2026-11-10: Implemented anonymous GET /api/delivery-notes/{id} with eager loading of the complete saved graph and the current recipient relationship. Reused NoteDetailsDto and its persisted Position ordering; the read query uses NoTracking, TechnicalUseCaseLogger<GetNote>, and cancellation throughout. Added Note.NotFound without changing recipient errors.
- POST now returns its existing complete envelope through Results.Created and advertises the saved note detail URI in Location. Both operation metadata and the generated document describe GET integer binding, anonymous full-detail responses, 200/400/404, calendar dates, current recipient details, and POST's Location header.
- TDD evidence: the first POST-to-GET HTTP tracer failed because the creation response had no Location. After adding the route/use case and header it passed with 200 and a complete saved response equivalent to creation. Subsequent HTTP cases verify all saved detail groups, positive child IDs, repeated part codes/original line order, 28-digit fractional weight without rounding, optional blank-to-null text, exact raw yyyy-MM-dd strings, separate retrieval of identical creations, missing/nonpositive IDs, malformed/out-of-range IDs, and no retrievable note after rejected input or missing recipient. Advancing the deterministic clock before GET verifies the creation instant is preserved. All requests use the anonymous real host.
- Refactoring assessment: inspected GetNote, endpoint metadata, NoteDetailsDto, NoteCreationTests, RecipientDetailsTests, NoteDocumentationTests, and existing TestSuite clients/request factories. Reused the existing repository, full response mapping and test host; no new graph projection or harness was justified. Replaced a reason-based branching rejection test with field/value/status/code theory inputs so scenario intent remains visible. Clean-code assessment: 10/10 for this scoped increment; names/responsibilities/error handling are focused and expected saved values are explicit independent literals.
- Test infrastructure reuse: added only Notes(id) to DeliveryNoteServiceApi, matching its existing recipient resource client and using the shared raw/success/error/parsing methods. NoteRequests and note-only identity setup remain capability composition around the same per-test database, clock and host. DeliveryNoteServiceApiTestBase owns host/client/database disposal; raw responses are disposed by their tests. Documentation configuration is established before startup; note identity is set after initialization and before requests. No new mutable shared state or internal mocks. Header assertions remain explicit in the two Location workflows; no additional wrapper adds useful reuse over the existing API suite language.
- Verification (repository root): dotnet test service/test/ApiTests/ApiTests.csproj --filter "FullyQualifiedName~NoteDetailsTests&Database!=PostgreSQL" -v minimal: 12 passed; the documentation filter passed 2 cases. dotnet test service/test/ApiTests/ApiTests.csproj --filter "FullyQualifiedName~Note&Database!=PostgreSQL" -v minimal: 167 passed. dotnet test service/DeliveryNoteService.sln --filter "Database!=PostgreSQL" -v minimal -- xUnit.ParallelizeTestCollections=false: 223 SQLite API tests and 171 unit tests passed. Scoped dotnet format service/DeliveryNoteService.sln --include service/src/Core/Errors/ErrorCodes.cs service/src/Core/UseCases/Notes/GetNote.cs service/src/WebApp/RestApi/NoteEndpoints.cs service/test/ApiTests/NoteDetailsTests.cs service/test/ApiTests/NoteDocumentationTests.cs service/test/ApiTests/TestSuite/DeliveryNoteServiceApi.cs --no-restore and the same command with --verify-no-changes passed. Final whitespace alignment was followed by formatting verification and focused detail/documentation checks. No Markdown formatter is configured; tracking follows repository conventions.
- Skills used: tdd (agreed POST/GET HTTP seam and red/green tracer), clean-code (focused implementation and production/test refactoring assessment), component-testing (required by .NET policy; real application/persistence with SQLite and deterministic clock). No loaded-but-unused skills. The implement-task pre-verification refactoring requirement takes precedence over the tdd skill's generic suggestion to defer refactoring to review.
- Remaining work: T003 owns concurrent creation, final-number competition and exhausted-creation readback. SQLite evidence does not prove PostgreSQL-specific migration/locking/error semantics; no PostgreSQL tests or Testcontainers ran. T002 introduced no migration. The prior JobExecutions snapshot drift remains documented in T001. Story remains In progress and plan active; no review or human acceptance is implied.
- 2026-11-10, review-01 correction attempt 1: F002 decimal response schemas now advertise decimal rather than double for every package/line field; the generated-document test covers all 14 request/detail numeric fields. F001 raw-JSON acceptance cases verify the saved exact value through a fresh GET, while rejection cases verify missing candidate notes. F003 normalized the covered raw-string call in NoteDetailsTests without changing its JSON value. See T001 correction evidence for 285 focused note tests, 342 SQLite API tests, 171 unit tests, formatting/refactoring/reuse assessment and skills. Task remains done, story In review and plan active; F001/F002/F003 resolution requires independent follow-up review.
- 2026-11-10, developer-authorized targeted cleanup: consolidated overlapping successful creation/detail checks into the full POST-Location-GET readback and repeated-creation workflows. The latter also verifies consecutive N00001/N00002. Representative missing-resource/binding/rejected-creation cases remain, including stable Guard-derived HTTP errors and no retrievable rejected note. DTO validation now has the explicitly agreed direct Validate unit seam; see T001 cleanup evidence for refactoring, test counts, commands and skills. Final solution checks pass 81 API and 179 unit tests. Task remains done; follow-up review of this cleanup is pending.
- 2026-11-10, developer-requested text/ID refinement: fresh GET now preserves the exact supplied header, contact, package and line strings, including surrounding whitespace and blank optional comments. Updated the existing complete-graph workflow and two existing rejected-creation cases for zero/negative RecipientId lookup failures; GET still confirms absence of a failed note. Final solution verification passed 83 SQLite API and 182 unit tests; scoped formatter verification passed. See T001 for red/green, refactoring/reuse and skill evidence. Task remains done; new behavior needs fresh follow-up review and explicit human acceptance remains separate.
