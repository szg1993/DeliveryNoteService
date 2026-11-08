# US-001: Create and view recipients

- Source: DeliveryNoteService/docs/backlog/US-001-create-and-view-recipients.md
- Lifecycle: active
- Created: 2026-11-08
- Updated: 2026-11-08
- Branch: feature/US-001-create-and-view-recipients

## Approach

Deliver creation followed by retrieval through HTTP as the first working
increment, with input validation and database-enforced code uniqueness included
from the outset. Extend it with paged listing, then verify concurrent creation on
the production database provider. Keep the source story authoritative for product
requirements; this plan describes execution only.

## Ordered tasks

| Order | Task | Status | Depends on | Increment |
| --- | --- | --- | --- | --- |
| 1 | [T001: Establish the versioned demo baseline](tasks/T001-establish-demo-baseline.md) | todo | None | Human prerequisite |
| 2 | [T002: Create a recipient and retrieve its details](tasks/T002-create-and-retrieve-recipient.md) | todo | T001 | Skeleton |
| 3 | [T003: List recipients with paging and sorting](tasks/T003-list-recipients.md) | todo | T002 | Extension |
| 4 | [T004: Verify concurrent code uniqueness on PostgreSQL](tasks/T004-verify-concurrent-code-uniqueness.md) | todo | T002, T003 | Edge case |

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

- **T001, human gate:** Only `.gitignore` was tracked at planning time. The existing
  demo and shared source trees are untracked. A developer must establish their
  intended, reviewed baseline before implementation commits can be reproduced
  from Git. Do not silently commit all untracked files during story execution.
- .NET SDK, package restore access, and OpenAPI CLI availability are required for
  coding-task verification. Their availability was not tested during planning.
  Reuse the existing OpenAPI generation scripts; restore or document a local tool
  manifest if the baseline does not supply the CLI. Do not silently upgrade it.
- T004 requires a working container runtime and permission to pull the PostgreSQL
  image already specified by the fixture. Availability is unverified. If absent,
  record the verification blocker rather than passing or skipping required
  concurrency coverage without disclosure.

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

- Implementation requested on 2026-11-08. Preflight stopped at the outstanding
  T001 human prerequisite: no baseline commit exists, and the solution/shared
  source trees remain untracked. No coding task or agent review was started.
  The developer manually removed `service/demo`; its absence was verified.
- Baseline inventory: 93 service application/test/configuration files, 249 root
  shared-source files, and 2 root configuration/script files (344 candidates).
  Another 158 files under `service/src/Common.*` are duplicate shared-source
  copies outside the target project's references; leave them untouched pending
  the baseline decision. Counts exclude files ignored by Git.
- Next action: the developer commits the intended baseline or explicitly
  authorizes the agent to review and commit those baseline inputs separately.
  Then verify the baseline, record T001 completion, re-check reservation ownership,
  and delegate T002 through the story workflow.
- Planning only; no implementation or application tests were run.
- T001 is the next action, owned by the developer. Coding tasks remain todo and
  are not eligible until their dependencies are satisfied.
- Story reservation confirmed on origin with attempt
  `74e21d5e-edaf-4437-b7b6-92eb677a5733` and marker commit
  `c6be19026ee5a106720359173614e3ca098a8738`. The originating chat retains the local
  record. Re-check shared ownership before implementation or a planning resume.
- The empty reservation marker was published under harness policy. Planning and
  implementation commits are not authorized for push by this invocation.
- When required tasks are satisfied, follow the harness's separate agent review
  and human acceptance workflow. No agent review or human acceptance has occurred.
