# T003: List recipients with paging and sorting

- Parent: [US-001](../README.md)
- Status: todo
- Depends on: T002
- Increment: Extension

## Outcome

A caller can discover created recipients through a deterministic, paged list.

## Scope

- Add the list use case and GET /api/recipients in the existing recipient route
  module. Reuse `PageRequestQueryParam`, sorting abstractions, repository paging,
  and `PagedEnvelope<T>` from the actual referenced shared projects.
- Return the agreed read fields, default to page 1/limit 10 and ID ascending, allow
  ID/name ordering, and append ID as a deterministic name-sort tie-breaker. Follow
  existing direction and paging query conventions rather than adding a new API.
- Keep the demo's limit=0 behavior, paging validation, and stable 400 errors for
  invalid paging/sorting. No search or cache is introduced.
- Extend recipient HTTP helpers as necessary. Arrange test recipients through
  POST; assert only HTTP-visible results.
- Add complete endpoint metadata and refresh `service/openapi.json` in this task.

## Acceptance criteria

- [ ] An empty database returns HTTP 200 with an empty paged collection.
- [ ] A successful POST is immediately visible in the list without a refresh flag.
- [ ] List items contain normalized details and server-assigned fields consistent
      with creation/detail responses.
- [ ] Default page/limit/order, explicit paging, supported sort directions, name
      ties, and limit=0 return the expected results and demo pagination metadata.
- [ ] Invalid query values return 400 in the demo's error format with stable codes.
- [ ] Invalid creation has no effect on a subsequent list; no direct database
      assertion is used to prove absence.
- [ ] The generated OpenAPI document accurately describes all three Recipient
      operations, query parameters, schemas, successful statuses, and errors.

## Verification

Use one HTTP red/green cycle at a time. Add `RecipientListingTests` with literal
expected examples covering empty lists, multiple pages, repeated names, sorting,
unpaged results, and invalid queries. Run:

```powershell
dotnet test service/test/ApiTests/ApiTests.csproj --filter 'FullyQualifiedName~Recipient|FullyQualifiedName~OpenApiSpecTest'
```

Then run the complete existing ApiTests suite once for shared paging/host
regressions. Do not require T004's PostgreSQL harness to pass this task's tests.
Format changed files and verify formatting. Record actual test commands and
red/green evidence.

## Progress

No implementation started.
