# Testing instructions

These instructions apply to API tests and unit tests in this directory and its
subdirectories, in addition to the inherited repository instructions.

## Test design

- Write a few happy-path and unhappy-path tests. Choose representative cases
  for meaningful behavior; do not test every possible scenario or multiply
  shared validation checks across every field.
- Test DTO validation with unit tests that call `.Validate()` directly. This is
  an agreed public test seam. Keep API tests to the smallest useful set proving
  complete workflows and essential HTTP behavior; do not repeat DTO-validation
  matrices through HTTP. These preferences supersede broader testing advice in
  shared skills and older plans, including HTTP-only validation test agreements.
- Keep test helpers small and reuse the existing test host. Extract helpers when
  they remove real duplication or make scenarios easier to understand.
- Justify specialized parsing and generated-OpenAPI tests by a concrete consumer
  need or material risk. Do not add exhaustive checks merely to mirror endpoint
  metadata or enforce every framework edge case. Keep representative coverage
  of meaningful application behavior.

## API test execution

- Use SQLite API tests for routine verification; SQLite coverage is sufficient.
- Do not run PostgreSQL API tests by default. Exclude tests tagged `Database=PostgreSQL` with `--filter "Database!=PostgreSQL"` when running API tests or a solution containing them.
- Before running any Testcontainers tests, including PostgreSQL API tests, ask the user and wait for explicit approval. Do not start test containers without that approval.
