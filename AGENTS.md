Read and follow [the shared agent instructions](../agentic-engineering/agent-harness/AGENTS.md).

## Project scale and engineering tradeoffs

This is a simple hobby project. Prioritize code the developer can understand and
maintain, then useful progress. Apply engineering practices in proportion to this scale.
These repository preferences take precedence over generic recommendations in
shared guides and skills.

- Prefer the simplest implementation that satisfies the agreed requirements.
  Add abstractions or infrastructure only to solve a concrete current problem.
- Inspect and reuse existing utilities and patterns before introducing new ones.
  In particular, use `Guard` for checks it already supports; keep business rules
  and normalization with the existing DTO/model conventions. Use straightforward
  conditions for unsupported business rules; extend `Guard` when actual reuse
  justifies a broadly useful check.
- Use the framework's normal property parsing and binding. Create custom parsing
  only for a necessary, concrete business requirement the framework cannot meet.
  Validate ordinary user mistakes and protect persisted data.
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
- Run checks appropriate to the change. Broaden or repeat verification when a
  meaningful edit, failure, or unresolved concern warrants it.
- Review for correctness, clarity, and reuse. Avoid speculative refactoring and
  work done solely to reach an arbitrary clean-code score.
- During refinement, call out requirements whose complexity is disproportionate
  to this hobby project. Before adding custom parsing or substantial infrastructure,
  explain normal framework behavior and its practical limitation, and recommend
  the simpler option. Make routine implementation choices autonomously. Once
  requirements are explicitly agreed, preserve them until the developer authorizes
  a change.
- Keep the existing task delegation and independent review process. Apply these
  proportionality and reuse expectations during reviews.

## Use case logging

- Always follow the logging pattern in `CreateRecipient` for every use case created or modified in the future: inject `TechnicalUseCaseLogger<TUseCase>` and wrap the entire use case operation in `logger.RunAsync(...)`.

## DTO style

- Preserve supplied string values. Do not trim them during validation or mapping,
  or replace optional empty/whitespace strings with null. Apply length limits to
  the supplied value.
- Do not reject IDs because they are zero or negative. Use normal binding and
  resource lookup; an absent referenced resource is not found.
- For every DTO created or modified in the future, declare every property explicitly with a public `init` setter. Do not use positional record parameters to declare DTO properties.
- Keep DTO-to-model mapping and normalization in the DTO, following `CreateRecipientDto`: `ToModel(...)` creates a model and delegates to `MapTo(model, ...)`; `MapTo(...)` assigns the mapped properties to the supplied model and returns that same instance. Use cases call these methods instead of duplicating the mapping.
- Pass dependencies needed for mapping, such as `ITimeService`, explicitly to `ToModel` and `MapTo`, and forward them when delegating.
- Use block bodies with explicit `return` statements for `FromModel` and `MapTo`. A delegating `ToModel` method may use an expression body, as in `CreateRecipientDto.ToModel`.

## Error codes

- Group entity-specific error codes in public nested static classes inside `ErrorCodes`, named after the entity, such as `ErrorCodes.Recipient`.
- In each entity class, define `private const string _prefix = nameof(Entity) + ".";`, replacing `Entity` with that class's name. Define each public string constant as `_prefix + "ErrorName"`, with a suffix matching the constant's name. For example, `ErrorCodes.Recipient.NotFound` has the value `"Recipient.NotFound"`.
- Follow this pattern for every error code created or modified in the future.

## GET endpoints

- Never validate `PageRequest` or `Sorting` in use case classes for GET endpoints.

## API test execution

- Use SQLite API tests for routine verification; SQLite coverage is sufficient.
- Do not run PostgreSQL API tests by default. Exclude tests tagged `Database=PostgreSQL` with `--filter "Database!=PostgreSQL"` when running API tests or a solution containing them.
- Before running any Testcontainers tests, including PostgreSQL API tests, ask the user and wait for explicit approval. Do not start test containers without that approval.
