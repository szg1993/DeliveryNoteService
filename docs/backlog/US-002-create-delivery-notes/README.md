# US-002: Create delivery notes

- Source: DeliveryNoteService/docs/backlog/US-002-create-delivery-notes.md
- Refinement source: conversation: US-002 creation rules, 2026-11-10
- Grilling: Shared understanding explicitly confirmed by the developer, 2026-11-10
- Lifecycle: active
- Created: 2026-11-10
- Updated: 2026-11-10
- Branch: feature/US-002-create-delivery-notes

## Approach

Deliver creation of a complete note through POST first, including its mandatory
validation, atomic persistence, and safe numbering. This first working increment
returns the saved note in the standard envelope. Add independent retrieval and
the creation Location header next, then exercise contention and number exhaustion
through the same HTTP boundary. Each increment includes its own tests and OpenAPI
metadata; correctness prerequisites are implemented in the first affected task.

The source story is the authoritative confirmed specification, now In review after
all three coding tasks completed. Its legacy
investigation remains historical reference. The refinements below summarize the
agreed contract for this breakdown; keep them synchronized with the story.

## Confirmed refinements

- Create the full legacy note, including contact details, package details,
  deadlines, and at least one line. Retain legacy business field names and maximum
  lengths from the source's data and constraint sections. Text is trimmed before
  validating lengths and storing it. Optional EKAER and comment fields may be
  absent; normalize blank optional text to null. Unit labels are nonblank free
  text, subject to existing length limits where specified, without a unit list.
- Require ContactInfo, PackageDetail, and Deadline, all legacy-required text
  fields, and a nonempty NoteLines collection without null elements. Package and
  line quantities are positive decimals; weights and dimensions are nonnegative
  decimals. Numeric values must be explicitly supplied; missing/null values are
  invalid, including weights and dimensions for which explicit zero is valid.
  Do not introduce rounding or additional precision limits. False
  handling flags and zero weights/dimensions are valid. There is no extra maximum
  line count in this story.
- Use calendar dates for all three deadlines, with TakeoverDate <= ShipDate <=
  EstimatedArrivalDate. Past dates and equal dates are allowed. Require all three
  values as DateOnly in DTOs and the domain model. Accept and return exactly
  yyyy-MM-dd, without timezone conversion. Reject timestamps, date-time strings,
  blanks, missing/null values, and invalid dates. Creation time is a UTC instant assigned
  by the server using ITimeService, separate from these calendar dates.
- Creation and full GET-by-ID retrieval work anonymously; access control is
  deferred. Creation always starts Pending. Creator attribution is
  deferred; no User relationship, user prerequisite, or placeholder user is
  introduced. The caller supplies RecipientId for an existing recipient. Other
  note/nested IDs, note number, status, creation time, and legacy UserId are not
  accepted as caller-assigned creation fields; their presence is rejected.
  Reject every other unknown property at every request level with 400 as well;
  retain case-insensitive matching of known properties.
- Number notes N00001 through N99999. Distinct successful creations, including
  concurrent ones, must have different numbers. Gaps are allowed. Exhaustion is
  a business failure returning HTTP 400. This is one permanent service-wide
  sequence without reset or reuse; gaps can exhaust it before 99,999 notes are
  saved. Creation then stops until a later feature changes the numbering scheme.
  Do not retain the legacy 55555 boundary.
- Each POST is a separate creation, even for identical content. Retrying after
  an uncertain response may create another note; no idempotency is promised.
- Business-rule failures return HTTP 400; missing notes or referenced recipients
  return 404. Use the existing error envelope and stable codes. This supersedes
  the earlier proposed 409 exhaustion response. It does not change the recipient
  endpoints or the global mapping of unrelated uniqueness conflicts.
- POST /api/delivery-notes returns 201; GET /api/delivery-notes/{id} returns 200.
  Both use Envelope<T> and expose all agreed note details, server fields, and
  current recipient details. Recipient information remains a live relationship,
  without a historical snapshot. Once GET exists, POST supplies its detail URI in
  Location. Note and child identifiers in read responses are server-assigned.
- POST and GET-by-ID are the agreed application test boundary. Arrange recipients
  through their existing POST endpoint. No new application test seam is required.

## Ordered tasks

| Order | Task | Status | Depends on | Increment |
| --- | --- | --- | --- | --- |
| 1 | [T001: Create a complete delivery note](tasks/T001-create-complete-note.md) | done | US-001 recipient creation, verified present | Skeleton |
| 2 | [T002: Retrieve a saved delivery note](tasks/T002-retrieve-saved-note.md) | done | T001 | Extension |
| 3 | [T003: Verify concurrent creation and number exhaustion](tasks/T003-verify-number-boundaries.md) | done | T001, T002 | Edge case |

There are three coding tasks. Execute them sequentially: they share note DTOs,
route registration, and HTTP helpers, and T003 verifies results through T002.
There are no independent parallel tasks. The task file owns its status and
progress; keep the table synchronized with those files.

## Implementation choices

- Target the actual .NET 8 solution, with models and use cases under
  service/src/Core, persistence under service/src/Persistence, and route modules
  under service/src/WebApp/RestApi. Earlier recipient planning paths naming
  Domain/Application no longer describe this checkout. Follow actual project
  references to the repository-root src/Common.* projects; do not edit the unused
  duplicates under service/src/Common.*.
- Follow CreateRecipient for injected use cases, TechnicalUseCaseLogger<T>,
  DTO-local Validate/ToModel/MapTo, explicit init properties, mapping dependencies,
  and response FromModel methods. Group new codes in ErrorCodes.Note using the
  required prefix convention. Pass cancellation through all asynchronous work.
- Model the note and its new contact/package/deadline/line records as one creation
  graph with required relationships. Persist the graph atomically. Link an
  existing recipient; do not create a second recipient or cascade note creation
  into recipient creation. Do not add status-change or child-management routes.
- Use the database-generated Note.Id as the numeric note-number suffix and expose
  NoteNbr as N plus that ID padded to five digits. Do not independently allocate
  or store a mutable number string. Enforce 1 <= Id <= 99999 with a named database
  check constraint in the production migration and SQLite-compatible schema.
  Database identity allocation and the primary key provide concurrent uniqueness;
  avoid read-last/increment logic and process-local locks. Translate only this
  named range-constraint violation to ErrorCodes.Note.NumberExhausted and HTTP
  400 in the persistence boundary. Keep unrelated technical failures distinct.
- Keep numeric values decimal and deadline dates DateOnly in the model and DTOs.
  Enforce the strict calendar-date contract locally: the shared DateOnly
  converter also accepts timestamps/nulls and cannot establish this contract on
  its own. Preserve required-value detection instead of silently substituting
  zero or a default date. Use existing UTC timestamp conventions for CreatedDate;
  response property names follow
  the legacy note names, including NoteNbr, CreatedDate, and NoteStatusId.
  Status is represented by the service's normal enum serialization. Reuse
  RecipientDetailsDto for current recipient data.
- Restrict unknown-property rejection, including server fields, to the new note
  creation contract at every nested level. Retain case-insensitive matching.
  Ensure model-binding failures use the standard 400 error shape;
  do not change unknown-property behavior for existing recipient requests.
- Extend the existing DeliveryNoteServiceApi wrapper, request factories, and
  TestSuite helpers. Keep the real host, validation, models, EF mappings,
  repositories, and transactions. Reuse the existing deterministic test clock
  and external substitutes; do not mock internal services or replace allocation.
- SQLiteDatabaseFixture deliberately seeds entity identities to hundreds. Add a
  note-specific per-test infrastructure helper to reset or advance only the Notes
  identity when a scenario requires a particular number. Do not alter shared
  fixture defaults. Test data still enters through HTTP; identity configuration
  is datastore setup, not an application test seam or a production endpoint.
- Test helpers share the existing host lifecycle, per-test database, clock, and
  disposal owner. Use capability helpers around that host rather than a new test
  harness. Configure Swagger before host startup in documentation tests; ordinary
  per-test identity setup runs after initialization and before requests.
- The current checkout has no tracked service/openapi.json, generation script, or
  OpenApiSpecTest. Add focused HTTP checks against the generated Swagger document
  with Swagger:Enabled in the test host. Do not restore deleted demo tooling or
  add a CLI merely because the old recipient plan mentioned it. Preserve service
  documentation hosting and the existing version endpoint; no service-wide
  OpenAPI retrofit is required.

## Acceptance coverage

Criterion numbers refer to the source's confirmed acceptance criteria.

| Source criterion or refinement | Tasks |
| --- | --- |
| AC1: Persist full creation and return 201 with ID/number/envelope | T001 |
| AC2: Server-generated fields and initial Pending status | T001 |
| AC3: Retrieve saved details and creation Location | T002 |
| AC4: Invalid input and stable errors without saved partial notes | T001, T002, T003 |
| AC5: Atomic creation of note and all dependent records | T001, T002, T003 |
| AC6: Unique concurrent numbers, correct range and exhaustion | T001, T003 |
| AC7: Generated OpenAPI for POST and GET | T001, T002, T003 |
| Required nested data, at least one line, text normalization/limits | T001, T002 |
| Explicit numeric inputs, decimal ranges, strict DateOnly dates and ordering | T001, T002 |
| Reject unknown/server fields at every level | T001 |
| Anonymous creation and full detail retrieval | T001, T002 |
| Each repeated POST creates separately; no idempotency | T001, T002 |
| Permanent sequence, no reset/reuse, exhaustion despite allowed gaps | T001, T003 |
| Existing recipient reference, current recipient response, unknown ID | T001, T002 |
| HTTP 400 business failures and 404 missing resources | T001, T002, T003 |

## Task-size audit

| Task | Single observable outcome | Change surfaces and sizing | Bounded verification |
| --- | --- | --- | --- |
| T001 | A valid complete note can be created through POST | New aggregate/DTO validation, additive persistence mapping/migration, and one thin POST use case/route. These three production surfaces stay together because they make one usable creation path; repositories, clock, logging, HTTP host, datastore fixture, and error middleware already exist. ID-derived numbering avoids a separate allocator adapter. No GET, new runner, UI, hosting work, or reusable harness is included. | Focused creation, rejection, and POST schema HTTP scenarios using SQLite |
| T002 | A saved note can be retrieved at its advertised URI | One read use case/route and response projection over T001's aggregate, plus Location and HTTP scenarios. No new schema foundation or infrastructure. | POST-to-GET workflow, read errors, and GET/Location schema checks |
| T003 | Creation remains correct under contention and at exhaustion | Note-specific SQLite identity setup, bounded concurrent HTTP requests, and range/error/rollback regression cases. Production invariants already belong to T001; fix any newly exposed defect here without redesigning allocation. | Small isolated contention and 99999-boundary scenarios, then SQLite regression suite |

T001 is bounded to creation; durable readback and its additional behavior matrix
are deliberately a separate task. Each task's verification uses only its listed
dependencies. Edge-case checks in T003 add distinct contention/boundary confidence
instead of deferring the basic validation and transaction requirements of T001.

## External prerequisites and gates

- US-001 is merged into master through PR #1 at
  025ec1cc157ab38b5cd4e5b8a133eec5f55839f9, confirmed by the developer on
  2026-11-10. Its recipient-creation dependency is satisfied. Formal US-001 closure
  remains pending a follow-up review of changes after its recorded PASS; this
  does not make the existing recipient implementation unavailable to US-002.
- .NET SDK, restored packages, and local formatter/test availability must be
  checked at implementation start. This planning invocation does not run builds,
  tests, containers, or dependency installation.
- SQLite API coverage is sufficient under current repository instructions. Do not
  add a required PostgreSQL task. SQLite cannot prove PostgreSQL-specific
  identity, migration, or constraint-error translation; review the generated
  production mapping and document this fidelity limit. Any requested additional
  Testcontainers execution requires explicit approval first.
- After coding tasks: independent agent review and explicit human acceptance are
  separate workflow gates, not additional coding tasks. They are not satisfied
  by this breakdown and do not start automatically during planning.

## Assumptions and open questions

No unresolved product question blocks this breakdown. The developer confirmed
shared understanding after grilling; the source now records those decisions.
Harmless representation choices:
blank optional text is null, absent Boolean flags are false, positive unknown
RecipientId is 404, and nonpositive RecipientId is invalid input (400). Follow
the current recipient detail route for integer binding: malformed/out-of-range
route IDs are 400, and absent integer note IDs, including nonpositive values,
are 404. Do not add new category/unit enumerations or reinterpret legacy names.

If the current database identity strategy cannot satisfy the named range
constraint and atomic graph save for both configured providers, surface that
implementation blocker rather than switching to an unplanned allocator. No
external adapter or runtime upgrade is assumed.

## Resume notes

- 2026-11-10: Grilling completed and shared understanding explicitly confirmed.
  Updated the authoritative story, index, and existing tasks without starting
  implementation. Confirmed permanent numbering without reset/reuse, separate
  creation on repeated POSTs, anonymous full retrieval, explicit numeric inputs,
  strict unknown-property rejection, and DateOnly deadline DTOs/domain values
  with exact yyyy-MM-dd input/output and no timezone conversion. The interim
  DateTimeOffset proposal was withdrawn; CreatedDate remains a UTC instant.
- 2026-11-10: Developer confirmed full legacy creation, Pending, anonymous access,
  deferred creator attribution, HTTP POST/GET test boundary, live recipient data,
  range through N99999 with gaps, validation rules, decimals, calendar dates, and
  full detail responses. Developer corrected business errors to HTTP 400 and
  retained 404 for not found. No implementation started.
- Reservation confirmed on origin for attempt
  aa94e8f5-208c-4204-a53b-50dac3e54bd0, branch
  feature/US-002-create-delivery-notes, marker
  aafe717307555d4d8dc865b2dd4240ddeaee091d. The originating chat retains the local
  record. Recheck ownership before implementation or a later planning resume.
- Only the empty reservation marker was published. This skill authorizes a local
  planning commit; it does not authorize pushing the planning commit, opening a
  pull request, merging, or starting implementation.

- 2026-11-10: Resumed reservation aa94e8f5-208c-4204-a53b-50dac3e54bd0 after explicit developer direction; origin marker aafe717307555d4d8dc865b2dd4240ddeaee091d still matches the persisted attempt. T001 complete with 154 focused note tests, 210 SQLite API regressions and 171 unit tests passing; scoped formatter verification passed. See T001 Progress for migration/provider fidelity and preserved pre-existing JobExecutions snapshot drift. Next eligible task is T002 (GET and Location); T003 depends on it. Story remains In progress, plan active; no agent review or human acceptance yet.
- 2026-11-10: T002 complete: GET fully reloads saved details/current recipient, POST advertises its Location, and generated metadata covers both. All 167 focused note tests, 223 SQLite API regressions and 171 unit tests passed; scoped formatting verification passed. See T002 Progress for red/green and reuse evidence. Next eligible task is T003 (concurrency/exhaustion through HTTP). Story remains In progress, plan active; no agent review or human acceptance yet. Reuse the same confirmed reservation attempt and local Git metadata record during delegation.
- 2026-11-10: T003 complete: bounded HTTP contention, N99999/repeated exhaustion and final-number competition pass with saved graph readback. A real unrelated-check scenario exposed prefix-based SQLite error translation; exact named matching now preserves the technical 500. Shared concurrency/observation infrastructure and failure-path response disposal are verified. Final solution verification passed 228 non-PostgreSQL API tests and 171 unit tests; scoped formatting verification passed. See T003 Progress for exact commands, refactoring and provider fidelity. All three tasks are done; story is In review and plan remains active. Next action is a fresh independent initial story review against master; no prior US-002 report or human acceptance exists. Reuse reservation aa94e8f5-208c-4204-a53b-50dac3e54bd0 and its persisted local record. Nothing was pushed.
