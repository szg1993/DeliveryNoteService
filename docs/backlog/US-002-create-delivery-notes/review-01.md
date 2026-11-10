# US-002: Agent review 01

- Review type: agent review
- Review date and time: 2026-11-10 15:00 Europe/Budapest (UTC+01:00)
- Authoritative story: `docs/backlog/US-002-create-delivery-notes.md`
- Plan: `docs/backlog/US-002-create-delivery-notes/README.md`
- Harness repository and commit: `agentic-engineering`, `d19a8d75f1a6e59c2c5953d32aa0741e4fc0195f`
- Reviewed branch: `feature/US-002-create-delivery-notes`
- Reviewed implementation head: `d74a1c4d9bc291a0475184b1365a3f2c443c94ca`
- Comparison base: `master`, `025ec1cc157ab38b5cd4e5b8a133eec5f55839f9`
- Review mode: initial review
- Previous US-002 reports and correction attempts: None
- Review coverage: complete
- Explicitly included uncommitted changes: None
- Final verdict: FAIL

## Verdict and merge readiness

The complete creation/retrieval workflow and bounded numbering scenarios work, but the story is not ready for merge. Decimal JSON binding silently rounds values before validation, including accepting a negative weight as zero. Generated schemas advertise binary floating-point values instead of the agreed decimal contract. New code also has multiline-parenthesis policy violations that formatter verification does not detect.

All three task files report done, the story is In review, and the plan is active. Those records do not override the defects below. Correct F001-F003 in a separate implementation correction, update factual task evidence, and obtain a follow-up review. This report covers the frozen implementation head above, not its own report commit. No human acceptance is recorded.

## Blocking findings

### F001: Decimal binding silently changes input before validation

- Severity: Blocking
- First reported in: review-01
- Location: `service/src/Core/UseCases/Notes/CreatePackageDetailDto.cs:9`, `service/src/Core/UseCases/Notes/CreateNoteLineDto.cs:15`, `service/src/Core/UseCases/Notes/NoteInput.cs:21`
- Violated requirement: creation contract requires nonnegative weights/dimensions and decimal values without silent rounding; AC4 rejects invalid input without persistence.
- Evidence: the seven decimal request properties use ordinary nullable decimal binding. The runtime parser can round a JSON number to decimal before `NoteInput.Number` sees it. A reviewer-only raw-JSON HTTP probe, using the real application host and SQLite migrations, created an existing recipient through POST, changed only `packageDetail.tareWgt` in `NoteRequests.Valid`, POSTed, and followed Location through a fresh GET:

| Raw JSON numeric value | POST | Returned weight | GET | Saved weight |
| --- | --- | --- | --- | --- |
| `0.12345678901234567890123456789` | 201 | `0.1234567890123456789012345679` | 200 | `0.1234567890123456789012345679` |
| `0.00000000000000000000000000001` | 201 | `0.0000000000000000000000000000` | 200 | `0` |
| `-0.00000000000000000000000000001` | 201 | `0.0000000000000000000000000000` | 200 | `0` |

The last case is a negative weight accepted and persisted as a successful note. Existing precision tests construct representable C# decimal values and cannot detect this binding loss.

- Required correction: scope exact decimal input handling to the note contract and reject numbers that cannot be represented without changing their mathematical value, using the standard 400 error shape. Apply it to TareWgt, PkgQty, PkgSizeX/Y/Z, PartQty and PartWgt. Preserve all exactly representable decimals, explicit zero rules, optional-field behavior and unrelated endpoint contracts. Add raw JSON HTTP cases for rounding, positive/negative underflow, overflow and exactly representable high precision, and verify rejected requests leave no retrievable note. Do not introduce a business precision cap or round the input to make it valid.

### F002: Generated decimal schemas declare double precision

- Severity: Blocking
- First reported in: review-01
- Location: `service/src/WebApp/RestApi/NoteContractSchema.cs:45`, `service/src/WebApp/RestApi/NoteContractSchema.cs:74`
- Violated requirement: AC7 describes the agreed decimal creation/detail contract; the source explicitly requires decimals without silent rounding.
- Evidence: generated GET `/swagger/v1/swagger.json` contains `{"type":"number","format":"double"}` for each package/line decimal property in both request and detail schemas. For example CreatePackageDetailDto.properties.tareWgt and NoteLineDetailsDto.properties.partQty have that shape. The filter sets numeric bounds but retains the generator's double format. Binary64 consumer types cannot preserve the high-precision decimal values that the server accepts and its own HTTP tests use.
- Required correction: describe decimal properties consistently in the note request and response schema families without promising binary64 precision (for example an appropriate decimal format or unformatted JSON number with a precise decimal description). Retain required/nullability/range metadata. Extend generated-document checks to cover every decimal request/detail property, so the consumer contract does not regress to double. Keep unrelated schemas unchanged.

### F003: New wrapped calls violate the central parentheses layout

- Severity: Blocking (explicit code-guideline requirement)
- First reported in: review-01
- Location examples: `service/src/Persistence/Migrations/20261110131638_AddDeliveryNotes.cs:147`, `service/test/ApiTests/TestSuite/NoteRequests.cs:38`, `service/test/ApiTests/TestSuite/NoteDatabase.cs:22`; the added migration designer and snapshot additions also retain the competing generated layout.
- Violated rule: `agentic-engineering/agent-harness/technologies/dotnet/csharp-style.md`, Multiline parentheses, requires wrapped arguments below the opening parenthesis and the closing parenthesis on its own aligned line. It explicitly says Roslyn does not enforce this rule, so review must.
- Evidence: the new CreateIndex calls close as `unique: true);`; raw-string calls start as `JsonNode.Parse("""` / `ExecuteSqlRawAsync("""` and close beside the string delimiter. New generated multi-argument calls similarly close beside their final argument. Scoped `dotnet format --verify-no-changes` passes, which does not establish compliance with this separate manual rule. No repository instruction overrides the argument layout or exempts story-added generated migrations.
- Required correction: normalize the covered wrapped argument/parameter lists in story-added/modified C# code, including the new migration and new/changed generated metadata, without touching unrelated baseline code. Preserve generated semantics and formatter compatibility, then rerun scoped formatter verification and relevant tests. No brace-style change is requested where repository configuration takes precedence.

## Non-blocking findings

None.

## Requirement traceability

Paths below are repository-relative. Test evidence was independently rerun unless explicitly identified as inspection or the additional reviewer probe.

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
| --- | --- | --- | --- | --- |
| AC1: valid full note creation, existing recipient, 201 and generated ID/number/envelope | CreateNote, CreateNoteDto, NoteDetailsDto, NoteEndpoints | NoteCreationTests complete normalized creation, separate consecutive numbers; NoteDetailsTests saved graph | Met | None |
| AC2: server ID/number/UTC clock/Pending | Note.Id/NoteNbr, DTO MapTo clock and Pending assignment, strict request DTOs | fixed clock/server-field/unknown-field creation checks | Met | None |
| AC3: Location, GET 200, complete saved details, ordered lines and calendar dates | NoteEndpoints, GetNote NoTracking/includes, Position projection, DateOnly response fields | NoteDetailsTests Location, full graph, repeated codes, exact raw dates, separate creations | Met | None |
| AC4: invalid input rejected in standard errors without new records | DTO validation and strict JSON binding, error middleware, single graph save | ordinary invalid-input matrix passes; additional negative-underflow probe returns 201 and saved zero | Not met | F001 |
| AC5: atomic full note creation | one SaveChangesAsync, EF transaction, required child FKs and graph mapping | rejected-creation GET absence, exhausted-ID absence and unchanged full winner GET; transaction inspected | Met | None |
| AC6: concurrent uniqueness, permanent N00001..N99999 sequence, exhaustion 400 | database identity/PK, named range check, exact provider filtering | all NoteNumberingTests normal contention, final number, repeated exhaustion and final-number competition pass | Met | None |
| AC7: generated POST/GET contracts, responses and anonymous strict input | NoteEndpoints metadata, NoteContractSchema | NoteDocumentationTests pass; reviewer-generated document still declares double for decimals | Not met | F002 |
| Legacy field names/limits; trim before validation/storage; preserve internal whitespace | DTO mapping, NoteInput, NoteLimits and EF mapping | all text-at-boundary and oversized/blank cases; complete graph literals | Met | None |
| Optional EKAER/comments absent/null/blank become null; nonblank free-text units | NoteInput.Optional and DTO validators | creation/default request, whitespace normalization and required unit cases | Met | None |
| Required contact/package/deadline, nonempty nonnull ordered lines, repeated part codes, no extra line cap | CreateNoteDto.Validate and persisted Position | null/missing/empty-line matrix and multi-line fresh read | Met | None |
| Explicit decimal numbers; positive quantities, nonnegative weights/dimensions; no silent rounding | nullable decimal properties, NoteInput.Number | omissions/nulls/ordinary ranges pass; raw negative underflow and rounding probe fail contract | Not met | F001 |
| Omitted handling flags false and nondefault flags preserved | Boolean defaults and MapTo | case-insensitive/default-flags and full nondefault-flags tests | Met | None |
| DateOnly input/domain/output, exact yyyy-MM-dd; reject other formats/null/missing; ordered/equal/past dates | StrictNoteDateConverter, CreateDeadlineDto, NoteDeadline | all date matrix and raw GET string checks | Met | None |
| Nonpositive recipient ID 400; positive unknown 404; live relationship | validation, repository lookup, Recipient FK/DTO, GetNote recipient include | missing/nonpositive recipient tests; fresh GET current linked recipient projection | Met | None |
| No user/creator prerequisite; caller server fields and all unknown properties rejected locally | DTO family Disallow attributes; no User relationship | all-level unknown/server-field cases; anonymous host requests | Met | None |
| Case-insensitive known fields; recipient contract unaffected by note strictness | shared options preserved, scoped DTO attributes | case-insensitive note test and existing recipient regressions | Met | None |
| Each identical POST creates separately; no idempotency | fresh graph per CreateNote call | sequential identical creation and distinct retrievable Locations | Met | None |
| Missing note integer IDs including nonpositive 404; malformed/out-of-range integer route 400 | explicit route binding, GetNote NotFound | NoteDetailsTests missing and malformed IDs | Met | None |
| Stable entity codes, correct business/missing/error separation | ErrorCodes.Note and exact named persistence translation | standard errors, Note.NumberExhausted and similarly named technical-check test | Met | None |
| Use-case logging, cancellation, DTO-local mapping and explicit init properties | CreateNote/GetNote, DTO family, explicit route binding | code inspection plus real host resolution and regression tests | Met | None |
| Additive production schema, no user relationship/legacy migration/unplanned allocator | AddDeliveryNotes migration, snapshot, NoteMapping | migration/schema inspection and SQLite migration-backed tests | Met | None |
| Applicable code-guideline layout | added/modified C# files | formatter passes; manual wrapped-call inspection fails | Not met | F003 |
| Preserve version contract and documentation hosting | existing MapVersionsEndpoint and Swagger hosting retained; scoped filter registration | generated document contains `/api/v1/Versions/{connectedServices}`; existing solution tests pass | Met | None |
| Excluded scope remains excluded | no note update/delete/list/status/child/user routes or UI/idempotency work | route/module and complete diff inspection | Met | None |

## Verification performed

Working directory for commands: repository root of DeliveryNoteService.

1. `git status --short --branch`, `git log --oneline 025ec1cc157ab38b5cd4e5b8a133eec5f55839f9..HEAD`, full changed-file inspection: reviewed head and base resolved; working tree clean before report. Earlier US-001 bookkeeping/review records on the branch were inspected as prior administrative changes, not treated as new US-002 implementation.
2. `dotnet test service/DeliveryNoteService.sln --filter "Database!=PostgreSQL" -v minimal -- xUnit.ParallelizeTestCollections=false`: passed, 228 API tests and 171 unit tests, zero failures/skips. This rebuilt the net8.0 projects with the configured analyzers/style-in-build settings. PostgreSQL-tagged tests were excluded.
3. `$files = @(git diff --name-only --diff-filter=ACMR 025ec1cc157ab38b5cd4e5b8a133eec5f55839f9 HEAD -- '*.cs'); dotnet format service/DeliveryNoteService.sln --include $files --verify-no-changes --no-restore`: passed, exit 0. No formatter mutation was run.
Markdown has no configured formatter; the report follows repository Markdown conventions.

4. `git diff --check 025ec1cc157ab38b5cd4e5b8a133eec5f55839f9 HEAD`: passed.
5. Additional reviewer-only HTTP probe, compiled and run from ignored local Git metadata, exercised the existing real host/SQLite migrations. It created a recipient through its endpoint, used raw numeric JSON literals in an otherwise valid note, followed each Location and recorded exact persisted results in F001. Only external clock setup and test-host configuration were substituted; application validators, use cases, EF, mappings and transactions remained real. The probe initially needed standalone content-root/logging setup; those probe-host setup failures were resolved before the successful evidence above. No application code or tests were changed.
6. The same probe fetched generated Swagger with Swagger enabled before startup. Inspected every note request schema's required/additionalProperties/bounds/date/line metadata, POST/GET response/Location metadata, decimal formats, and the existing version operation. F002 records the decimal format mismatch.
7. Inspected production migration identity/range/relationship/date/numeric metadata and exact PostgreSQL check-name translation. No PostgreSQL connection, container or migration execution was performed.

## Code-guideline assessment

Applicable policies: .NET C# style, dependency injection, ASP.NET Core, .NET testing, Web API OpenAPI documentation and version endpoint. Repository settings take precedence where they differ from central formatting preferences. Formatter success does not waive F003.

Use cases are small orchestration units with complete TechnicalUseCaseLogger coverage and cancellation. Mapping and normalization remain in explicit-init DTOs, with NoteLimits reused across validation, persistence and documentation. Clean-code assessment: 8/10 for the scoped change; correcting exact numeric input, truthful numeric schemas and wrapped-call layout is required before a 10/10 assessment. No broad cleanup is requested.

Skills used: `clean-code` for implementation/readability review and `component-testing` as required by the .NET testing policy. No loaded-but-unused skills. TDD was not loaded because this review made no behavioral change.

Test architecture was inspected across NoteCreationTests, NoteDetailsTests, NoteNumberingTests, NoteDocumentationTests, ConcurrentCreationTests, existing recipient tests and TestSuite. All application acceptance checks use the agreed HTTP boundary with real internal services. The one focused deterministic-handler test verifies concurrency test-infrastructure cleanup rather than replacing the application. Existing application host/clock/database ownership remains with DeliveryNoteServiceApiTestBase; documentation config precedes startup and datastore identity/trigger setup precedes requests. ConcurrentCreation is actually shared by recipient/note clients and disposes completed responses on peer failure. NoteAssertions is shared across detail/numbering consumers. Requests retain separate scopes/connections and isolated databases; no application serialization lock or replacement allocator was introduced. The missing raw-number cases explain F001; current representable-decimal tests cannot provide that evidence.

## Architecture assessment

The cumulative design follows Core/Persistence/WebApp boundaries. Creation maps one required note graph linked to an existing recipient and performs one atomic save. Database identity and primary-key uniqueness provide the number source; the named range check enforces exhaustion and exact provider filtering preserves unrelated technical errors. GET explicitly reloads the graph/current recipient and orders lines by saved Position. The schema change is additive, and there is no User/placeholder creator, snapshot recipient or extra allocator.

Consumer consequences are material and approved: anonymous POST and GET expose full contact/current recipient details; retrying POST may create a second note; the permanent sequence can exhaust with gaps. F001 currently breaks the numeric input boundary and F002 misstates it to consumers. Apply the additive migration before serving the new routes. Production PostgreSQL fidelity remains limited to inspection, as explicitly permitted by repository instructions.

## Recorded deviations

- No approved product-scope deviation was identified. F001 and F002 are unapproved deviations from the numeric contract, not accepted changes to it.
- The scaffold attempted to drop pre-existing JobExecutions metadata because the runtime model had already removed it. T001 records retaining its prior snapshot/designer metadata and removing the unrelated DropTable operation. Review confirmed the migration remains additive and preserves the prior table; the runtime/snapshot drift predates this story and is not an additional blocking finding.
- The existing version endpoint is retained under the approved central contract in `agentic-engineering/agent-harness/technologies/web-api/version-endpoint.md`; no version-contract retrofit was introduced.
- No PostgreSQL execution was required or inferred from SQLite success. The component-testing skill's general higher-fidelity advice is subordinate to the explicit repository rule that SQLite coverage is sufficient and containers require approval.

## Unverified items and remaining risks

- PostgreSQL-specific identity allocation, concurrent locking, actual migration execution and provider error behavior were inspected but not executed. SQLite success does not prove those semantics. No container was started.
- Pre-existing JobExecutions runtime/snapshot drift remains documented for future migration authors; do not accept a later scaffolded unrelated table drop without review.
- No UI/manual task or external dependency is required by this plan. Human review and explicit acceptance remain outstanding after corrections and a current agent PASS. No push, pull request, merge or human approval was performed.
