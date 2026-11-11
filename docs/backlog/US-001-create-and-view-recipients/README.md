# US-001: Create and view recipients

- Source: DeliveryNoteService/docs/backlog/US-001-create-and-view-recipients.md
- Lifecycle: complete
- Created: 2026-11-08
- Updated: 2026-11-11
- Branch: feature/US-001-create-and-view-recipients
- Merge: [PR #1](https://github.com/szg1993/DeliveryNoteService/pull/1), master,
  `025ec1cc157ab38b5cd4e5b8a133eec5f55839f9`, 2026-11-10.

## Approach

Deliver creation followed by retrieval through HTTP as the first working
increment, with input validation and database-enforced code uniqueness included
from the outset. Extend it with paged listing, then verify concurrent creation on
the production database provider. Keep the source story authoritative for product
requirements; this plan describes execution only.

## Ordered tasks

| Order | Task | Status | Depends on | Increment |
| --- | --- | --- | --- | --- |
| 1 | [T001: Establish the versioned demo baseline](tasks/T001-establish-demo-baseline.md) | done | None | Human prerequisite |
| 2 | [T002: Create a recipient and retrieve its details](tasks/T002-create-and-retrieve-recipient.md) | done | T001 | Skeleton |
| 3 | [T003: List recipients with paging and sorting](tasks/T003-list-recipients.md) | done | T002 | Extension |
| 4 | [T004: Verify concurrent code uniqueness on PostgreSQL](tasks/T004-verify-concurrent-code-uniqueness.md) | done | T002, T003 | Edge case |

There are three coding tasks and one separate human prerequisite. The task file
owns its status and progress. Keep the table synchronized with task files. Execute
in the order shown; no parallel execution is planned because the tasks share API
and test files, and T004 observes its result through T003's list endpoint.

## Implementation choices

- Target `service/DeliveryNoteService.sln` and its existing .NET 8 projects.
- Add recipient behavior under `service/src/Domain`, `Application`, `Persistence`,
  and `WebApp/RestApi`, using injected use cases and explicit Minimal API bindings.
- Reuse `Envelope<T>`, `PagedEnvelope<T>`, repositories, unit of work, paging,
  sorting, error format, and existing UTC timestamp conventions.
- Follow actual project references for shared code: WebApp, Application,
  Persistence, and ApiTests reference the repository-root `src/Common.*` projects.
  The similarly named copies under `service/src/Common.*` are not the shared
  dependencies selected by those project references. Do not edit both copies.
- Use existing `ApiTests`, its real application host, SQLite fixture, and in-memory
  messaging substitute. Keep application services, validation, EF mappings, and
  repositories real. Arrange recipient data through POST and observe it through
  HTTP responses. Do not mock internal services or assert directly against tables.
- Retain PostgreSQL as the production provider already used by existing migrations.
  T004 reuses the existing Testcontainers PostgreSQL adapter for a narrowly scoped
  higher-fidelity HTTP test; it does not replace the main SQLite suite.
- Keep the current demo host and namespaces. Renaming the solution, deleting demo
  behavior, redesigning shared infrastructure, and legacy data migration are not
  part of this story.
- Document each new endpoint in the task introducing it, regenerate
  `service/openapi.json`, and keep `OpenApiSpecTest` passing. Preserve existing
  documentation configuration rather than undertaking a service-wide retrofit.
- Apply TDD one observable behavior at a time at the agreed HTTP boundary. Record
  the observed failing test and passing verification in each coding task.

## Acceptance coverage

Criterion numbers refer to the source story's Acceptance criteria section.

| Source criterion | Tasks |
| --- | --- |
| AC1: Create, envelope, ID, Location | T002 |
| AC2: Retrieve normalized created details | T002 |
| AC3: Newly created recipient immediately visible in list | T003 |
| AC4: Invalid creation rejected without creating a recipient | T002, T003 |
| AC5: Deterministic recipient list | T003 |
| AC6: Empty list succeeds | T003 |
| AC7: Existing recipient details | T002 |
| AC8: Unknown ID returns 404 | T002 |
| AC9: OpenAPI covers all three operations | T002, T003 |
| AC10: Duplicate names allowed | T002 |
| AC11: Normalized code uniqueness, including concurrent requests | T002, T004 |
| AC12: Paging defaults and ID/name sorting | T003 |
| AC13: Country codes and derived foreign status | T002 |
| AC14: Required fields, lengths, trimming, textual ZIP | T002 |
| AC15: Demo paging format, validation, limit=0 | T003 |
| AC16: 400/404 errors and stable error format | T002, T003 |
| AC17: Code character restrictions | T002 |
| AC18: No postal-format or address-existence validation | T002 |

## External prerequisites

- **T001, satisfied:** The developer authorized the agent to review and commit the
  baseline separately. Commit `96c28175a57702594123080c992b423b4f8fc2b3` tracks the
  344 selected demo/shared/configuration files; 158 unused duplicate shared files
  remain untracked. Do not silently add those remaining files during execution.
- .NET SDK, package restore access, and OpenAPI CLI availability are required for
  coding-task verification. Their availability was not tested during planning.
  Reuse the existing OpenAPI generation scripts; restore or document a local tool
  manifest if the baseline does not supply the CLI. Do not silently upgrade it.
- **T004, satisfied:** Docker engine 29.6.1 and the cached `postgres:17-alpine`
  image were verified. The dedicated PostgreSQL HTTP concurrency scenario passed
  with actual migrations and disposable-container cleanup. Re-running that
  scenario still requires Docker; it does not silently skip without the runtime.

## Task-size audit

| Task | Single observable outcome | Change surfaces and size rationale | Bounded verification |
| --- | --- | --- | --- |
| T001 | A clean checkout contains the agreed existing demo | Human review and Git baseline only; excluded from coding count | Inspect tracked paths and commit |
| T002 | A valid recipient can be created and read back | Recipient model/mapping, use case, two thin endpoints, HTTP scenarios; three production surfaces stay together because the layers, persistence adapter, test host, and error envelope already exist. No new harness or external integration | Recipient create/detail HTTP tests, existing OpenAPI test, focused error regression |
| T003 | Recipients can be discovered in a deterministic paged list | One list use case, thin endpoint, query validation, HTTP scenarios; reuses T002's model and test setup | List HTTP tests and OpenAPI test |
| T004 | Competing creates produce one recipient and one conflict | Targeted fixture selection plus concurrent HTTP scenario and only any correctness fix it reveals; separate because provider-specific test infrastructure is a distinct surface | Narrow SQLite and PostgreSQL duplicate-code HTTP tests |

Each coding task includes its own tests and endpoint documentation. There is no
deferred testing phase. T004 provides distinct provider-specific confidence;
production uniqueness enforcement is required in T002 rather than deferred.

## Assumptions and open questions

The product specification is confirmed. No product questions block the breakdown.
Country codes must come from a deterministic recognized two-letter code set; no
network lookup belongs in the request path. Exact type names and error-code names
are implementation details, following existing conventions. Escalate a demo-pattern
difficulty that materially changes the task instead of redesigning it silently.

## Resume notes

- 2026-11-11: Complete. Review-05 passed with complete coverage at
  `bbc5c93`; the developer approved human review and
  [human acceptance 06](review-06.md) records completion. The correction commits
  on `feature/US-001-create-and-view-recipients` still need to be pushed and
  merged by the developer; the reservation holds until merge.
- 2026-11-11: Review-04 F007 correction attempt 1 (narrowed dead code) is committed
  as `1cb679d`: deleted the unregistered `CreateRecipientDtoSchema.cs`. Evidence:
  no remaining references in `service`/`src`; solution build 0 warnings/0 errors;
  non-PostgreSQL solution tests passed (ApiTests 74/74, UnitTests 182/182). Details
  are in T002 progress. Next: a follow-up review, then explicit human acceptance.
  Task statuses are unchanged.
- 2026-11-11: Review-03 (complete coverage) returned FAIL on F007 only; F001-F006
  resolved or no longer applicable. The developer accepted the reduced generated
  OpenAPI contract ("Accept reduced contract"); AC9 was revised in `8324535`. No
  implementation change for F007. A follow-up review against the revised AC9 is
  next, then explicit human acceptance.
- 2026-11-11: Review-02 correction attempt 1 for F001, F002, F004, F005 and F006
  (F003 needed no code change; tests and docs already express 404) is committed as
  `fa3f912`. Evidence: red then green DTO and HTTP tests; non-PostgreSQL solution
  tests passed (ApiTests 74/74, UnitTests 182/182); scoped `dotnet format`
  verification passed for 50 formatted files and review-02 include scope. The
  PostgreSQL concurrency test was updated to expect 400 `Recipient.AlreadyExists`
  but not run (needs user approval). Details are in T002 and T004 progress. Next:
  a complete-coverage follow-up review. Task statuses are unchanged.
- 2026-11-11: The developer asked to close US-001. Review-02 (FAIL, incomplete
  coverage) remains the latest agent review. The developer revised AC11 (duplicate
  codes return 400 `Recipient.AlreadyExists`, including the database race),
  AC14/AC17 (only code and country normalized; other text preserved; empty
  trimmed code rejected), AC16 (nonpositive IDs return 404), and the test boundary
  (DTO `Validate()` unit tests). F003 and most of F004 become no longer applicable
  under these revisions. Required corrections: F001 (empty trimmed code, preserved
  text), F002 (400 for both duplicate paths, OpenAPI), F004 remainder (one HTTP
  rejected-create/no-save check), F005, F006. Then a complete-coverage follow-up
  review. New reservation: attempt `a0122952-a1e5-4968-8e9f-28d32ff010f4`, remote
  `origin`, branch `feature/US-001-create-and-view-recipients`, marker
  `63dc447b36ed367be9b898bd3d0eb162863ea9db` on master `7720314`.
- 2026-11-10: The developer confirmed the story was merged. PR #1 is present in
  master at `025ec1cc157ab38b5cd4e5b8a133eec5f55839f9`; all four task statuses remain
  done. US-002 can use the merged recipient implementation. The latest committed
  agent review remains review-01, covering `5f5b513d23bf5a733b691dc8e1f7188a1f7c5226`.
  Later commits `fa1a3ba` and `99ae0fd` changed implementation and tests, so the
  closure workflow requires a follow-up review before recording Done/complete.
  No new acceptance record, agent verdict, or test execution is claimed. The
  earlier notes below describe historical checkpoints, including their then
  outstanding merge and review actions.
- 2026-11-08: Independent [agent review 01](review-01.md) returned PASS with
  complete coverage and no findings, committed as `347e49b`. It covers
  implementation head `5f5b513d23bf5a733b691dc8e1f7188a1f7c5226` and independently
  verified all 115 API tests and formatting of all 26 changed C# files. All four
  tasks are done; no required technical work remains. Human review and explicit
  acceptance are next. Keep the story In review and plan active. No pull request
  was created or merge performed; implementation commits have not been pushed.
  This final handoff updates tracking only, not the reviewed implementation.
- 2026-11-08: T004 is complete. The dedicated PostgreSQL concurrency check passed,
  the existing SQLite duplicate-code regression passed 3/3, and the complete API
  suite passed 115/115 sequentially, with no failures or skips. Scoped formatting
  and verification passed. All four tasks are done. The story is In review and
  the plan remains active; independent agent review is next. No human acceptance
  has occurred. The earlier task-selection checkpoints below are historical.
- PostgreSQL verification uses an instance-scoped fixture selection hook in the
  existing host; ordinary tests retain SQLite. Select the dedicated scenario with
  Database=PostgreSQL or RecipientConcurrencyTests. The initial exact timestamp
  comparison exposed PostgreSQL microsecond precision; using the existing fixed
  test clock restored deterministic exact assertions without production changes.
- 2026-11-08: T003 is complete: paged/sorted recipient listing and OpenAPI are
  implemented. All 75 Recipient/OpenAPI checks passed with default parallelism;
  the complete API suite passed 114/114 with explicit sequential xUnit execution.
  The first default-parallel full-suite attempt stalled and was cancelled; its
  root cause remains unestablished. Scoped formatting and verification passed.
  T004 is the next eligible task. Use page=1/limit=0 or limit=0 alone in Recipient
  requests; the demo's Unpaged() test helper sends page=0 and is not suitable.
  The earlier T002 next-task checkpoint below is historical and superseded.
- 2026-11-08: T002 is complete. Creation/detail HTTP behavior and OpenAPI metadata
  are implemented; 59 final focused tests passed, including 48 Recipient cases,
  OpenAPI freshness, and related existing Demo/Paging cases. Scoped formatting and
  formatter verification passed. T003 is the next eligible task, followed by T004.
  Story remains In progress and plan active; no story review or human acceptance
  has occurred. The checkpoints below are historical and superseded where noted.
- Migration scaffolding initially exposed an imported model/snapshot mismatch for
  the existing DemoModel UniqueId index. T002 preserves that existing index and
  makes the Recipient migration additive. See its task record for verification.
- Current environment: .NET SDK 10.0.302 with .NET 8 runtimes, pinned Swagger CLI
  6.9.0 and EF tooling 8.0.10 used successfully. Docker engine availability was
  checked by the parent and the PostgreSQL scenario was verified in T004.
- Historical T002 checkpoint, resolved by completion: the initial POST scenario
  reached green after its table migration. GET was not yet implemented and its
  test awaited execution. Test ownership moved between parent and task agent to
  avoid overlapping SQLite initialization while command approvals were resolved.
- Historical baseline handoff: the developer explicitly authorized baseline review and a separate
  baseline commit, then continuation of US-001. T001 is complete with baseline
  commit `96c28175a57702594123080c992b423b4f8fc2b3`. T002 is the next eligible
  coding task. The prior baseline stopping point below is historical and resolved.
- Implementation requested on 2026-11-08. Preflight stopped at the outstanding
  T001 human prerequisite: no baseline commit exists, and the solution/shared
  source trees remain untracked. No coding task or agent review was started.
  The developer manually removed `service/demo`; its absence was verified.
- Baseline inventory: 93 service application/test/configuration files, 249 root
  shared-source files, and 2 root configuration/script files (344 candidates).
  Another 158 files under `service/src/Common.*` are duplicate shared-source
  copies outside the target project's references; leave them untouched pending
  the baseline decision. Counts exclude files ignored by Git.
- Historical next action (resolved by T001): the developer commits the intended baseline or explicitly
  authorizes the agent to review and commit those baseline inputs separately.
  Then verify the baseline, record T001 completion, re-check reservation ownership,
  and delegate T002 through the story workflow.
- Historical planning state: no feature implementation or application tests had run. Coding tasks remained
  todo; T002 is eligible after the recorded baseline completion.
- Story reservation confirmed on origin with attempt
  `74e21d5e-edaf-4437-b7b6-92eb677a5733` and marker commit
  `c6be19026ee5a106720359173614e3ca098a8738`. The originating chat retains the local
  record. Re-check shared ownership before implementation or a planning resume.
- The empty reservation marker was published under harness policy. Planning and
  implementation commits are not authorized for push by this invocation.
- Agent review is complete. Follow the harness's human acceptance workflow only
  after explicit developer approval; no human acceptance has occurred.
