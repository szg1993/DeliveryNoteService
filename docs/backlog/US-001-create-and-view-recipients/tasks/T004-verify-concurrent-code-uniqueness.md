# T004: Verify concurrent code uniqueness on PostgreSQL

- Parent: [US-001](../README.md)
- Status: done
- Depends on: T002, T003
- Increment: Edge case

## Outcome

Competing HTTP requests for equivalent codes leave exactly one recipient and
return one successful creation and one conflict on the production provider.

## Scope

- Add a narrow provider-verification mode or fixture selection using the existing
  root `src/Common.Testing/Database/TestContainerPostgres/PostgresDatabaseFixture`.
  Keep the main suite on its existing SQLite fixture and messaging substitute.
  Keep provider selection isolated; do not mutate static fixture state while
  unrelated tests run or point tests at a developer's database.
- Apply existing migrations plus T002's migration to a fresh disposable PostgreSQL
  database. Keep the real application host, EF mappings, repositories, and unique
  constraint in the HTTP path. No new public test seam is introduced.
- Use a bounded concurrent start and independent request scopes against the same
  isolated database, with equivalent normalized codes. Avoid timing sleeps or
  assertions about the winning request.
- Observe the final recipient through GET /api/recipients with the agreed unpaged
  query. Verify the loser has the agreed 409 error envelope and stable error code.
- Fix only a uniqueness, provider-error translation, or migration defect revealed
  by this scenario. The invariant is already implemented in T002; this task adds
  confidence against differences hidden by the SQLite substitute.

## Acceptance criteria

- [x] Fresh PostgreSQL initialization applies the actual migrations successfully.
- [x] Concurrent equivalent-code creation yields one 201 and one 409, not a 500.
- [x] A subsequent HTTP list contains exactly one recipient with that normalized
      code; its details match the successful response.
- [x] Existing sequential duplicate-code behavior still passes on SQLite.
- [x] The PostgreSQL fixture is independently selectable and cleans up its test
      databases/containers; existing tests retain their SQLite default.
- [x] Runtime/image requirements and the exact verification command are recorded.

## Verification

Use TDD for the new HTTP concurrency scenario and any discovered behavior fix.
Use a dedicated `RecipientConcurrencyTests` class/category and run its PostgreSQL
configuration explicitly; do not silently skip it when the runtime is unavailable.
Re-run SQLite duplicate-code coverage after any shared error-handler change.
Format changed files and verify formatting. Record provider, commands, migration
initialization result, response results, and any required external action.

## Progress

2026-11-08: Started provider-specific HTTP verification. The parent confirmed the
Docker engine and cached PostgreSQL 17 Alpine image are available. Reusing the
existing application host with an instance-scoped database-fixture selection hook;
the PostgreSQL fixture owns its disposable container and database lifecycle.

### PostgreSQL verification

The dedicated `RecipientConcurrencyTests` carries the `Database=PostgreSQL` trait
and inherits the same DemoService application host, HTTP clients, clock, and
in-memory messaging substitute as the main suite. An overridable `TestDatabase`
hook in the shared API base selects the datastore per instance; its default still
returns the injected SQLite fixture. Create/configure/clear operations, null
sorting conventions, and scheduler lock selection all use that selected fixture.
There is no global or static provider switch and no alternate application host.

The thin PostgreSQL base owns its `PostgresDatabaseFixture` and initializes it
before the host. That fixture starts `postgres:17-alpine`, applies the actual
migrations to a fresh template, and clones an isolated test database. Disposal
awaits the application/messaging shutdown before clearing database pools and
disposing the container in a finally block. Existing creation, detail, listing,
and demo tests keep their original host and SQLite fixture.

Test-first verification reused the already implemented database uniqueness
invariant; no production correctness fix was needed. The first real PostgreSQL
run reached the expected 201 and 409 responses, including the PostgreSQL unique
constraint violation path, but the final exact persisted-details assertion exposed
the provider's microsecond timestamp precision versus the live clock's finer
resolution. The test now uses the existing controllable clock with a fixed,
exactly representable UTC instant, preserving the exact response/list assertion.
This is a determinism correction, not a change to the production timestamp
contract. The next PostgreSQL run passed 1/1, with no failures or skips.

The HTTP concurrency helper releases both asynchronous requests from a common
start gate and bounds the operation with a 30-second cancellation deadline. It
does not sleep, mock repositories, query tables, or assume which request wins.
Assertions verify one 201, one 409 with the stable UniqueKeyConflict error code,
and one normalized recipient whose listed details exactly match the winner.

Exact provider command, from the repository root:

```powershell
dotnet test service/test/ApiTests/ApiTests.csproj --filter 'FullyQualifiedName~RecipientConcurrencyTests' --no-restore -v minimal --blame-hang --blame-hang-timeout 90s -- xUnit.ParallelizeTestCollections=false
```

The runtime requires Docker with Linux-container support and the fixture's
`postgres:17-alpine` image. The parent verified Docker engine 29.6.1 and the cached
image before execution. The test does not silently skip when Docker is absent.
It can also be selected by `--filter 'Database=PostgreSQL'`. To run the ordinary
SQLite suite without the dedicated provider test, use
`--filter 'Database!=PostgreSQL'`; the existing tests do not start PostgreSQL.

### Scoped refactoring assessment

Reviewed the existing API base, DemoService host/scheduling configuration, and
creation/detail/listing helper consumers. Provider selection is the only added
shared application mechanic. `PostgresDemoServiceApiTestBase` owns only fixture
lifecycle; the small `RecipientConcurrency` helper owns bounded HTTP invocation
mechanics. Scenario inputs, statuses, normalized code, and winner/list comparison
remain visible in the test. Configuration occurs before application startup,
mutable database/container state is per test, and the established host owns its
own disposal. No broader framework extraction, internal mocks, or changes to the
production endpoints were justified.

### Regression verification

The existing SQLite normalized-code regression passed 3/3 with no failures or
skips:

```powershell
dotnet test service/test/ApiTests/ApiTests.csproj --filter 'FullyQualifiedName~CreateRecipient_RejectsExistingNormalizedCode' --no-restore -v minimal
```

The complete API suite, including the dedicated PostgreSQL test and the existing
SQLite tests, passed 115/115 with no failures or skips in 11 seconds:

```powershell
dotnet test service/test/ApiTests/ApiTests.csproj --no-restore -v minimal --blame-hang --blame-hang-timeout 90s -- xUnit.ParallelizeTestCollections=false
```

The run completed normally; blame-hang reported all tests finished without a
sequence file. Sequential execution is the same explicit verification mode used
for T003 after its default-parallel full-suite stall; this task does not claim to
have diagnosed or fixed that earlier parallel-run issue.

Scoped `dotnet format` and `--verify-no-changes` passed on all five changed C#
files. After the final manual multiline-parenthesis layout correction in the new
test, its scoped formatter and verifier again passed cleanly, followed by another
PostgreSQL run passing 1/1 with no failures or skips (396 ms). No source edits
followed those checks. No Markdown formatter is configured; tracking links,
whitespace, and Git diff checks were verified separately.

Skills used: `tdd` for test-first provider verification and the observed
determinism red/green correction; `component-testing` for HTTP-boundary tests,
external-system fidelity, isolation, and shared application-fixture ownership;
`clean-code` for the scoped production/test infrastructure assessment and final
readability pass. No loaded skill was unused.

T004 is complete. All required implementation tasks are done; the story is In
review, with the plan active. Independent agent review and human acceptance have
not occurred. No task-specific blocker remains.
