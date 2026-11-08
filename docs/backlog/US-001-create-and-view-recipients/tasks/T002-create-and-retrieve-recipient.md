# T002: Create a recipient and retrieve its details

- Parent: [US-001](../README.md)
- Status: todo
- Depends on: T001
- Increment: Skeleton

## Outcome

A caller can create a valid recipient and follow its Location header to retrieve
the persisted, normalized details without login.

## Scope

- Add the recipient model, request/read DTOs, EF mapping, normalized-code unique
  index, and additive PostgreSQL migration under `service/src`. Keep migration and
  model changes compatible with the existing SQLite test fixture.
- Add create and detail use cases through the established repository/unit-of-work
  and use-case registration patterns. Follow existing ID and UTC timestamp
  conventions; derive foreign status from normalized country instead of accepting
  it from the caller. Do not accept client-assigned ID or creation time as inputs.
- Implement all source-story input invariants in this first affected slice:
  required fields, trimming, length limits, allowed code characters, recognized
  uppercase two-letter countries, and normalized code uniqueness. Duplicate names
  remain valid. Preserve ZIP as text and add no postal or address verification.
- Add `RecipientEndpoints` following `DemoEndpoints`, with explicit parameter
  bindings and cancellation tokens. Register POST /api/recipients and
  GET /api/recipients/{id} from the existing composition root.
- Return the demo's envelopes, HTTP 201 and Location on creation, HTTP 200 on detail,
  HTTP 400 on invalid input, HTTP 404 on unknown positive ID, and HTTP 409 on code
  conflict. Use stable error codes and the existing error envelope. Update the
  actually referenced root `src/Common.AspNetCore` middleware's uniqueness mapping
  as needed, preserving existing error behavior for other exception types.
- Reuse `ApiTests` and its SQLite/messaging fixtures. Add only small recipient HTTP
  client/setup helpers, not a new test runner, repository mock, or persistence fake.
- Add complete OpenAPI metadata and refresh `service/openapi.json` in this task.

## Acceptance criteria

- [ ] POST followed by GET of Location returns normalized recipient details, a
      server-generated ID, UTC creation time, and correct domestic/foreign status.
- [ ] Blank, missing, overlength, invalid-country, and invalid-code inputs return
      400. Boundary-length inputs and textual ZIPs with leading zeros work.
- [ ] Names and addresses retain internal text while surrounding whitespace is
      removed; a nonempty ZIP is not rejected for country-specific format.
- [ ] Sequential code variants such as `ACME`, `acme`, and ` ACME ` conflict after
      normalization; two recipients with the same name and different codes work.
- [ ] A database unique constraint protects normalized codes from the first
      implementation; a pre-check alone is insufficient.
- [ ] Invalid creation followed by valid creation of the same candidate code
      succeeds, showing rejection did not reserve that code or persist a recipient.
- [ ] Malformed/nonpositive IDs return 400, and unknown positive IDs return 404 in
      the agreed error format.
- [ ] Both endpoints are anonymous and documented accurately in generated OpenAPI.

## Verification

Use TDD at the confirmed HTTP boundary, one failing behavior test followed by
minimal passing implementation. Name the new suite `RecipientCreationTests` and
`RecipientDetailsTests` so verification can target them. Run from the repository:

```powershell
dotnet test service/test/ApiTests/ApiTests.csproj --filter 'FullyQualifiedName~RecipientCreationTests|FullyQualifiedName~RecipientDetailsTests|FullyQualifiedName~OpenApiSpecTest'
```

Run focused existing HTTP error regressions affected by the middleware change.
Observe effects through responses and follow-up HTTP calls, not database queries.
Do not require T003's listing endpoint for this task's verification. Run applicable
formatters on changed files and verify formatting before committing. Record exact
commands, red/green evidence, and any environmental blockers.

## Progress

No implementation started.
