Read and follow [the shared agent instructions](../agentic-engineering/agent-harness/AGENTS.md).

## Use case logging

- Always follow the logging pattern in `CreateRecipient` for every use case created or modified in the future: inject `TechnicalUseCaseLogger<TUseCase>` and wrap the entire use case operation in `logger.RunAsync(...)`.

## DTO style

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
