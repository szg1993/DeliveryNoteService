Read and follow [the shared agent instructions](../agentic-engineering/agent-harness/AGENTS.md).

## API test execution

- Use SQLite API tests for routine verification; SQLite coverage is sufficient.
- Do not run PostgreSQL API tests by default. Exclude tests tagged `Database=PostgreSQL` with `--filter "Database!=PostgreSQL"` when running API tests or a solution containing them.
- Before running any Testcontainers tests, including PostgreSQL API tests, ask the user and wait for explicit approval. Do not start test containers without that approval.
