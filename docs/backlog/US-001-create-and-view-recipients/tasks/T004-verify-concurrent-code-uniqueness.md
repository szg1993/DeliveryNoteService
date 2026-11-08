# T004: Verify concurrent code uniqueness on PostgreSQL

- Parent: [US-001](../README.md)
- Status: todo
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

- [ ] Fresh PostgreSQL initialization applies the actual migrations successfully.
- [ ] Concurrent equivalent-code creation yields one 201 and one 409, not a 500.
- [ ] A subsequent HTTP list contains exactly one recipient with that normalized
      code; its details match the successful response.
- [ ] Existing sequential duplicate-code behavior still passes on SQLite.
- [ ] The PostgreSQL fixture is independently selectable and cleans up its test
      databases/containers; existing tests retain their SQLite default.
- [ ] Runtime/image requirements and the exact verification command are recorded.

## Verification

Use TDD for the new HTTP concurrency scenario and any discovered behavior fix.
Use a dedicated `RecipientConcurrencyTests` class/category and run its PostgreSQL
configuration explicitly; do not silently skip it when the runtime is unavailable.
Re-run SQLite duplicate-code coverage after any shared error-handler change.
Format changed files and verify formatting. Record provider, commands, migration
initialization result, response results, and any required external action.

## Progress

No implementation started. Container-runtime availability was not checked during
planning; this must be verified before claiming the provider scenario passed.
