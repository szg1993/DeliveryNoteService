# Web frontend instructions

Read and follow [the shared agent instructions](../../AgenticEngineering/agent-harness/AGENTS.md)
and the [repository instructions](../CLAUDE.md). The simplicity preferences there apply here too.

These choices are project decisions. Keep them unless the developer agrees to a change.

- **Component library:** use Angular Material only. Do not add another component library.
- **Backend connection:** call the backend through the relative path `/api`. During development,
  `proxy.conf.json` forwards `/api` to the local backend at `http://localhost:5211`.
- **State:** keep state in signals inside components and services. Do not add NgRx or another state
  library.
- **API clients:** write a small typed API client by hand for each backend resource, such as
  `recipients/recipients-api.ts`. Do not generate clients.
- **Tests:** tests never call the real backend. Substitute it with `HttpTestingController`. Cover
  the major behaviors only. Do not add end-to-end tests.
