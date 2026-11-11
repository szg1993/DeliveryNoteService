# T001: Update an existing recipient

- Parent: [US-003](../README.md)
- Status: done
- Depends on: None (US-001 merged)
- Increment: Skeleton

## Outcome

A caller can replace an existing recipient's name, country, ZIP, city, and address
through `PUT /api/recipients/{id}` and immediately read the updated recipient,
while invalid input and unknown IDs are rejected without changing stored data.

## Scope

- `service/src/Core/UseCases/Recipients/UpdateRecipientDto.cs`: properties
  `Name`, `Country`, `Zip`, `City`, `Address` with public `init` setters and
  `[Description]` text in the style of `CreateRecipientDto`. No `Id`, `Code`, or
  foreign flag. `Validate()` applies the same rules as creation for these five
  fields; reuse the existing checks instead of copying them (for example, a small
  shared helper that both DTOs call). `MapTo(Recipient model)` assigns the four
  supplied-as-is fields and the trimmed, uppercased country, and returns the model
  using a block body. It does not touch `Code`, `Id`, or `CreatedAt`.
- `service/src/Core/UseCases/Recipients/UpdateRecipient.cs`: `[AutoRegister]` use
  case with `IRepository<Recipient> repository`, `IUnitOfWork`, and
  `TechnicalUseCaseLogger<UpdateRecipient>`, wrapped in `logger.RunAsync`. Order:
  `dto.Validate()`, lookup by ID (`NotFoundException` with
  `ErrorCodes.Recipient.NotFound`, as in `GetRecipient`), `dto.MapTo(recipient)`,
  save, return `RecipientDetailsDto.FromModel(recipient).Envelop()`. Last write
  wins; no concurrency token.
- `service/src/WebApp/RestApi/RecipientEndpoints.cs`: map
  `PUT /api/recipients/{id}` with `[FromRoute] int id` and
  `[FromBody] UpdateRecipientDto`, returning 200 with the envelope. Add tag, name
  `UpdateRecipient`, summary, description, `AllowAnonymous`, `Accepts`, and
  `Produces` for 200, 400, and 404, consistent with the existing recipient
  endpoints.
- Tests: `service/test/UnitTests/DtoValidation/UpdateRecipientDtoTests.cs` with a
  few representative `Validate()` cases; HTTP tests in a new
  `service/test/ApiTests/RecipientUpdateTests.cs`. Add a small request factory
  next to `RecipientRequests` if it removes duplication.

Out of scope: changing codes, PATCH, optimistic concurrency, `UpdatedAt`,
delivery-note assertions (T002).

## Acceptance criteria

- [x] A valid update of an existing recipient returns 200 with the updated
      `Envelope<RecipientDetailsDto>`; GET-by-ID and the list return the new details.
- [x] Name, ZIP, city, and address are stored exactly as supplied; country is
      trimmed and uppercased; foreign status follows the new country.
- [x] Code, ID, and `CreatedAt` are unchanged, including when the request body
      contains a `Code` property (ignored by normal binding).
- [x] Missing, empty, or too-long fields and an unrecognized country are rejected
      with 400 using the existing validation error codes, and the stored recipient
      is unchanged.
- [x] A well-formed unknown ID, including zero or negative, returns 404 with
      `Recipient.NotFound`; a malformed ID returns 400.
- [x] The generated OpenAPI document lists the PUT operation with 200, 400, and 404.

## Verification

Follow TDD and `service/test/CLAUDE.md`:

- Unit tests calling `UpdateRecipientDto.Validate()`: one valid case and a few
  representative rejections (missing field, too long, invalid country). Do not
  repeat the full creation matrix.
- HTTP tests (SQLite): successful update observed through GET-by-ID, with code and
  `CreatedAt` preserved while a `Code` is supplied and a country change flips
  `IsForeign`; one rejected update leaving the recipient unchanged; unknown ID
  returns 404.
- Run the unit tests and `dotnet test` for the API tests with
  `--filter "Database!=PostgreSQL"`; existing tests still pass.
- Run the repository formatter on changed files and verify formatting.

## Progress

### 2026-11-11

Implemented `PUT /api/recipients/{id}` on `feature/US-003-update-recipients`.

- Added `UpdateRecipientDto` (five `init` properties, `Validate()`, block-bodied
  `MapTo(Recipient)` that leaves code, ID, and `CreatedAt` untouched),
  `UpdateRecipient` use case (`TechnicalUseCaseLogger` + `logger.RunAsync`,
  validation → lookup → mapping → save → response), and the endpoint with
  summary, description, `Accepts`, and 200/400/404 `Produces` metadata.
- Reuse: country recognition and normalization moved into
  `RecipientCountries.ThrowIfUnrecognized` / `Normalize`, now used by both
  `CreateRecipientDto` and `UpdateRecipientDto` (the error code and message are
  unchanged; `RecipientCountries.Contains` had no other consumer and was
  replaced). The per-field `Guard` calls stay in each DTO and share
  `RecipientLimits`: `Guard` derives the reported field name from the caller's
  argument expression, so a parameter-based helper would change the error data.
- TDD: the first HTTP test (successful update visible through PUT, GET-by-ID, and
  list, code and creation time preserved, country normalized, `IsForeign`
  re-derived) failed to compile without the DTO, then passed after the DTO, use
  case, and endpoint. Six of the eight `UpdateRecipientDtoTests` cases failed
  against an empty `Validate()`, then passed. The ignored-`Code`, rejected-update,
  unknown-ID, and malformed-ID HTTP tests passed when written, because the
  earlier slices already implemented that behavior; each one would fail if its
  rule regressed.
- Tests: `test/UnitTests/DtoValidation/UpdateRecipientDtoTests.cs` (8 cases) and
  `test/ApiTests/RecipientUpdateTests.cs` (6 cases). Added
  `DtoFactory.UpdateRecipient()` and `RecipientRequests.ValidUpdate()` next to
  the existing creation factories. The existing host, `Api.Recipients(id)`
  resource endpoint (`PutAsync`/`PutRawAsync`), and error assertions were reused
  without changes; no new shared infrastructure was needed.

Refactoring assessment (clean-code): the shared country rule was the only
justified extraction. The use case mirrors `CreateRecipient`/`GetRecipient`.
Unit tests use `with` expressions instead of adding `With*` extension methods for
a second DTO type; the existing test instructions allow this, and new
extensions would duplicate the creation helpers for six call sites. No further
refactoring was justified.

Verification:

- `dotnet format DeliveryNoteService.sln --include <changed files>` followed by
  `--verify-no-changes`: exit 0.
- `dotnet test DeliveryNoteService.sln --filter "Database!=PostgreSQL"`: ApiTests
  80/80 passed, UnitTests 190/190 passed.
- Generated OpenAPI (`dotnet swagger tofile`, v1): `PUT /api/recipients/{id}`
  has operationId `UpdateRecipient`, an `application/json` request body, and
  responses 200, 400, 404.

Remaining work: none for T001. T002 (delivery-note test) is next.
