# T001: Update an existing recipient

- Parent: [US-003](../README.md)
- Status: todo
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

- [ ] A valid update of an existing recipient returns 200 with the updated
      `Envelope<RecipientDetailsDto>`; GET-by-ID and the list return the new details.
- [ ] Name, ZIP, city, and address are stored exactly as supplied; country is
      trimmed and uppercased; foreign status follows the new country.
- [ ] Code, ID, and `CreatedAt` are unchanged, including when the request body
      contains a `Code` property (ignored by normal binding).
- [ ] Missing, empty, or too-long fields and an unrecognized country are rejected
      with 400 using the existing validation error codes, and the stored recipient
      is unchanged.
- [ ] A well-formed unknown ID, including zero or negative, returns 404 with
      `Recipient.NotFound`; a malformed ID returns 400.
- [ ] The generated OpenAPI document lists the PUT operation with 200, 400, and 404.

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

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
