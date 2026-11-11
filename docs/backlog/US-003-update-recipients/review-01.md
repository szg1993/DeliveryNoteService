# US-003: Review 01

- Review type: agent review
- Review date and time: 2026-11-11 13:36 (+01:00)
- Authoritative story: `docs/backlog/US-003-update-recipients.md`
- Plan: `docs/backlog/US-003-update-recipients/README.md`
- `agentic-engineering` commit: `d19a8d75f1a6e59c2c5953d32aa0741e4fc0195f`
- Reviewed head: `feature/US-003-update-recipients` at `8ac80ffd85ac8d374058b7ba2a5a059ecace6f55`
- Comparison base: `master` at `2c80d390b49c26e26bf018425f949f469cc50ea5` (repository default branch; no pull request exists; merge base equals the base commit)
- Review mode: initial review (no earlier review reports exist for US-003)
- Review coverage: complete
- Explicitly included uncommitted changes: none (working tree clean)
- Final verdict: **PASS**

## 1. Verdict and merge readiness

**PASS.** Every acceptance criterion is met, and no blocking finding remains. The
implementation adds `PUT /api/recipients/{id}` through an `UpdateRecipient` use case
that follows the `CreateRecipient`/`GetRecipient` patterns, logging, DTO, and error
conventions. The build, formatter verification, and all non-PostgreSQL tests pass.
The generated OpenAPI document describes the operation with 200, 400, and 404.

The branch is ready for human review and manual verification. PostgreSQL
(Testcontainers) API tests were not run; see section 9.

Commits reviewed (`master..feature/US-003-update-recipients`):

| Commit | Subject | Content |
|---|---|---|
| `bd3a064` | US-003: Reserve story | Story reservation metadata |
| `5a4560b` | US-003: Break down update recipients | Plan and task files |
| `fd9faf3` | T001: Update an existing recipient | DTO, use case, endpoint, unit and API tests |
| `8ac80ff` | T002: Show updated recipient on delivery notes | One API test and task records |

## 2. Blocking findings

None.

## 3. Non-blocking findings

### F001: PUT not-found tests omit a negative ID

- Location: `service/test/ApiTests/RecipientUpdateTests.cs`, `UpdateRecipient_UnknownId_ReturnsNotFound`
- Rule: AC8 ("including zero or negative values") and the story's agreed test boundary.
- Evidence: the theory uses `0` and `123456`. A negative ID is not exercised for
  PUT. The agreed test boundary only requires "404 for an unknown ID", and the PUT
  handler uses the same `[FromRoute] int id` binding and `repository.GetAsync(id, ct)`
  lookup that `RecipientDetailsTests.NonPositiveRecipientId_ReturnsNotFound` already
  covers with `-1` for GET, so the behavior is met by construction.
- Suggested correction (optional): replace or extend `0` with `-1` in the inline data.

### F002: Code-supplied test does not also assert the creation time

- Location: `service/test/ApiTests/RecipientUpdateTests.cs`, `UpdateRecipient_IgnoresSuppliedCode`
- Rule: T001 verification text ("code and `CreatedAt` preserved while a `Code` is
  supplied"); the story's test boundary lists "code and creation time preserved
  (including when a `Code` is supplied)".
- Evidence: creation-time preservation is asserted in
  `UpdateRecipient_ReplacesDetailsAndKeepsCodeAndCreationTime` (no `Code` in the
  body), and code preservation with a supplied `Code` is asserted in
  `UpdateRecipient_IgnoresSuppliedCode`. Together they cover the behavior, because
  `UpdateRecipientDto` has no `Code` or `CreatedAt` property and `MapTo` never
  touches either field. The split is a plan-level detail, not a story deviation.
- Suggested correction (optional): none required; keep the split if the developer
  prefers the narrower tests.

## 4. Requirement traceability

| Requirement or acceptance criterion | Implementation evidence | Test evidence | Status | Finding |
|---|---|---|---|---|
| AC1: Valid update of an existing recipient returns HTTP 200 with the updated recipient in `Envelope<RecipientDetailsDto>` | `service/src/WebApp/RestApi/RecipientEndpoints.cs` (`MapPut` returns `UpdateRecipient.RunAsync` result); `service/src/Core/UseCases/Recipients/UpdateRecipient.cs` returns `RecipientDetailsDto.FromModel(recipient).Envelop()` | `RecipientUpdateTests.UpdateRecipient_ReplacesDetailsAndKeepsCodeAndCreationTime` asserts 200 and the envelope content; rerun passed | Met | None |
| AC2: GET-by-ID and listing return the new details immediately | `UpdateRecipient` saves through `IUnitOfWork.SaveChangesAsync`; no caching | Same test asserts GET-by-ID and list equal the expected updated recipient; rerun passed | Met | None |
| AC3: Update replaces name, country, ZIP, city, and address; all five required (full replacement) | `UpdateRecipientDto` has exactly five `init` properties; `Validate()` calls `Guard.ThrowIfMissing` on each; `MapTo` assigns all five | `UpdateRecipientDtoTests.NameMissing`, `MissingProperties`; API happy-path test changes all five fields; rerun passed | Met | None |
| AC4: US-001 field rules unchanged (as-supplied text, same max lengths, country trimmed/uppercased/recognized) | `UpdateRecipientDto.Validate()` uses the same `Guard` calls and `RecipientLimits` as `CreateRecipientDto`; country rule shared through `RecipientCountries.ThrowIfUnrecognized` / `Normalize`, also used by creation | `UpdateRecipientDtoTests.AddressTooLong`, `WhitespaceOnlyTextAndUnnormalizedCountryAreAccepted`, `CountryUnrecognized`; API test stores `" Globex  Kft "` etc. verbatim and `" us "` as `US`; creation tests still pass | Met | None |
| AC5: Foreign status re-derived from the new country; cannot be supplied | No foreign-status property on `UpdateRecipientDto`; `RecipientDetailsDto.FromModel` derives `IsForeign` from `Country != "HU"` | API happy-path test changes HU to US and asserts `IsForeign = true`; rerun passed | Met | None |
| AC6: Code, ID, and UTC creation time unchanged; `Code` or unknown property ignored by normal JSON binding | `UpdateRecipientDto` has no `Code`, `Id`, or `CreatedAt`; `MapTo` does not touch them; normal binding | Happy-path test asserts `Id`, `Code = "ACME"`, `CreatedAt` after a clock advance; `UpdateRecipient_IgnoresSuppliedCode` sends `Code = "GLOBEX"` (serialized by runtime type) and asserts `ACME`; rerun passed | Met | F002 |
| AC7: Invalid details rejected with HTTP 400, existing error format and stable codes; stored recipient unchanged | `dto.Validate()` before lookup, mapping, and save; existing `DataValidationException` handling | `UpdateRecipient_RejectsInvalidDetailsWithoutChangingRecipient` asserts 400, `Recipient.InvalidCountry`, and unchanged GET result; DTO unit tests cover `FieldMissing` / too-long codes; rerun passed | Met | None |
| AC8: Malformed route ID returns 400; well-formed unknown ID (incl. zero/negative) returns 404 with `Recipient.NotFound` | `[FromRoute] int id`; `NotFoundException(ErrorCodes.Recipient.NotFound, ...)` on missing lookup, as in `GetRecipient` | `UpdateRecipient_UnknownId_ReturnsNotFound` (0, 123456); `UpdateRecipient_MalformedId_ReturnsBadRequest`; negative IDs covered for the identical binding and lookup by `RecipientDetailsTests` (GET); rerun passed | Met | F001 |
| AC9: Retrieving a delivery note that references the updated recipient shows updated details | No production change; US-002 delivery-note retrieval loads the current recipient | `UpdateRecipient_IsShownOnExistingDeliveryNote` asserts the note's `Recipient` equals the PUT response; rerun passed | Met | None |
| AC10: Endpoint described in generated OpenAPI with its status codes; schema annotations follow the US-001 reduced contract | `WithTags`, `WithName("UpdateRecipient")`, `WithSummary`, `WithDescription`, `AllowAnonymous`, `Accepts<UpdateRecipientDto>`, `Produces` 200/400/404 | Generated document inspected: operationId `UpdateRecipient`, integer path `id`, required JSON body `UpdateRecipientDto`, responses 200 (`RecipientDetailsDtoEnvelope`), 400 and 404 (`MedusaErrorResponse`); `UpdateRecipientDto` schema has the same shape as `CreateRecipientDto` (no per-property descriptions, as accepted for US-001) | Met | None |
| Scope: delivered through API, `UpdateRecipient` use case, and persistence, following `CreateRecipient` patterns and logging | `[AutoRegister]` use case with `IRepository<Recipient> repository`, `IUnitOfWork`, `TechnicalUseCaseLogger<UpdateRecipient>`, wrapped in `logger.RunAsync` | Covered by API tests above | Met | None |
| Agreed decision: no `Id` in body; last write wins; no PATCH | DTO has no `Id`; no concurrency token; only `MapPut` added | Code inspection | Met | None |
| Implementation default: validate before lookup | `dto.Validate()` precedes `repository.GetAsync` | Code inspection (not separately tested; not required by the agreed test boundary) | Met | None |

## 5. Verification performed

All commands ran from `service/` against the reviewed head with a clean working tree.

| Command | Result |
|---|---|
| `dotnet build DeliveryNoteService.sln` | Build succeeded, 0 warnings, 0 errors (`EnforceCodeStyleInBuild` is `true` in `service/src/Directory.Build.props` and `service/test/Directory.Build.props`) |
| `dotnet format DeliveryNoteService.sln --verify-no-changes --include <the nine changed C# files>` | Exit code 0 |
| `dotnet test DeliveryNoteService.sln --filter "Database!=PostgreSQL"` | ApiTests: 81 passed, 0 failed; UnitTests: 190 passed, 0 failed |
| `dotnet test DeliveryNoteService.sln --no-build --filter "FullyQualifiedName~UpdateRecipientDtoTests\|FullyQualifiedName~RecipientUpdateTests"` | UnitTests: 8 passed; ApiTests: 7 passed (confirms the new tests are discovered and run) |
| `dotnet tool restore`, then with `ASPNETCORE_ENVIRONMENT=Development`: `dotnet swagger tofile --output <temporary-directory>/openapi-us003.json src/WebApp/bin/Debug/net8.0/DeliveryNoteService.WebApp.dll v1` | Document written; PUT operation inspected with PowerShell (results in AC10 row) |

The nine changed C# files: `src/Core/UseCases/Recipients/CreateRecipientDto.cs`,
`src/Core/UseCases/Recipients/RecipientCountries.cs`,
`src/Core/UseCases/Recipients/UpdateRecipient.cs`,
`src/Core/UseCases/Recipients/UpdateRecipientDto.cs`,
`src/WebApp/RestApi/RecipientEndpoints.cs`, `test/ApiTests/DtoFactories/DtoFactory.cs`,
`test/ApiTests/RecipientUpdateTests.cs`, `test/ApiTests/TestSuite/RecipientRequests.cs`,
`test/UnitTests/DtoValidation/UpdateRecipientDtoTests.cs`.

Not run: PostgreSQL API tests (`Database=PostgreSQL`, Testcontainers), because
`service/test/CLAUDE.md` requires explicit developer approval.

## 6. Code-guideline assessment

Applied guidance: repository `CLAUDE.md`, `service/test/CLAUDE.md`, `CONTEXT.md`;
the .NET policy (C# style, .NET testing with `component-testing`, ASP.NET Core); the
Web API policy (OpenAPI endpoint metadata, version endpoint); and `clean-code`.
Repository proportionality rules (simple hobby project) take precedence over generic
guidance.

- Repository conventions: the use case is named for the business operation, uses
  `repository` for the single repository, keeps the validation, lookup, mapping,
  persistence, and response steps separated by blank lines, calls `dto.Validate()`
  directly, and uses the `TechnicalUseCaseLogger` / `logger.RunAsync` pattern. The
  DTO declares explicit `init` properties, preserves supplied strings, does not
  reject non-positive IDs, and uses a block-bodied `MapTo` that returns the same
  instance. No `ToModel` is needed because an update always maps onto an existing
  model. `ErrorCodes.Recipient.NotFound` and `InvalidCountry` are reused.
- Reuse: the country rule was extracted into `RecipientCountries.Normalize` and
  `ThrowIfUnrecognized` and adopted by both DTOs, with the same error code and
  message. The nine per-field `Guard` calls remain duplicated between the create and
  update DTOs; this is justified because `Guard` derives the reported field name
  from `CallerArgumentExpression`, so a parameter-based helper would change the error
  data. The duplication is small and acceptable at this scale.
- C# style: braces, multiline parentheses (use case constructor and `RunAsync`
  parameters, `MapPut` handler), and naming follow the guide; no XML documentation
  was added. Formatter verification and code-style build enforcement pass.
- ASP.NET Core: the endpoint lives in the existing route module, every application
  parameter has an explicit binding source, and response metadata matches the
  handler's possible results (200 envelope, 400 validation or malformed ID, 404
  not found).
- OpenAPI: purpose, parameters, request body, success and error responses, and
  anonymous access are described; schema detail matches the reduced contract that
  AC10 references. The version endpoint contract is unaffected by this story.
- Tests (`component-testing`, `service/test/CLAUDE.md`): DTO rules are tested
  through `Validate()` unit tests with representative cases rather than a full
  matrix; the API tests are a small set of use-case-level component tests through
  the real `WebApplicationFactory` host with SQLite, arranged with DTO instances and
  `with` expressions. Tests follow Arrange-Act-Assert with blank-line separation,
  and scenario values stay visible at the call sites. New request factories
  (`RecipientRequests.ValidUpdate`, `DtoFactory.UpdateRecipient`) sit next to the
  existing creation factories; no new shared infrastructure was introduced, and the
  existing host, `ResourceEndpoint` wrappers, and error assertions are reused, so
  no extracted-infrastructure scope check applies. The private `UpdateWithCodeDto`
  subtype is a minimal way to send an unknown `Code` property without building JSON
  by hand, consistent with the test-design rule.
- Clean-code score: 9/10. Remaining small items are F001 and F002.

## 7. Architecture assessment

- The change follows the existing layering: endpoint in `WebApp/RestApi`, use case
  and DTO in `Core/UseCases/Recipients`, persistence through `IRepository<Recipient>`
  and `IUnitOfWork`. No new abstractions, migrations, or configuration were added.
- Data integrity: validation precedes lookup and mutation, and save happens only
  after successful mapping, so invalid requests leave stored data unchanged (tested).
  Code, ID, and creation time cannot be changed because the DTO does not carry them.
- Concurrency: last write wins, as agreed; no concurrency token.
- External contract: the new `PUT /api/recipients/{id}` operation and
  `UpdateRecipientDto` schema are introduced by the story itself. The existing
  `POST`, `GET` contracts and `CreateRecipientDto` behavior are unchanged; the
  internal `RecipientCountries.Contains` method was replaced, with no other
  consumers.
- No unnecessary parallel design: the use case mirrors `CreateRecipient` and
  `GetRecipient`, and the delivery-note relationship reuses US-002 retrieval.

## 8. Recorded deviations

Story deviations: none. All differences from legacy behavior (200 instead of 201
with the recipient in the body, no body `Id`, no caller-supplied foreign flag,
field validation, immutable code) are agreed story decisions.

Plan and task-level differences, recorded for traceability:

- `RecipientCountries.Contains` was replaced by `Normalize` and
  `ThrowIfUnrecognized` (acknowledged in the T001 progress notes; internal only).
- The T001 suggestion of "a small shared helper that both DTOs call" was applied to
  the country rule only; per-field `Guard` calls stay in each DTO (acknowledged in
  T001, reason: `CallerArgumentExpression` field names).
- Code preservation with a supplied `Code` and creation-time preservation are
  asserted in separate tests rather than one (see F002).
- T002 required no production change; its test characterizes existing US-002
  behavior (acknowledged in T002).

Repository-required additions: none beyond the story.

## 9. Unverified items and remaining risks

- PostgreSQL API tests were not run (Testcontainers requires explicit developer
  approval). The update path uses only the existing repository lookup and save
  without new mappings or migrations, so the PostgreSQL-specific risk is low.
- Manual verification through Swagger UI or a running service was not performed;
  it is part of human acceptance.
