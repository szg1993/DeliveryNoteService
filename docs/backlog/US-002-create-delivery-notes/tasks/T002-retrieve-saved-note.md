# T002: Retrieve a saved delivery note

- Parent: [US-002](../README.md)
- Status: todo
- Depends on: T001
- Increment: Extension

## Outcome

A caller can follow the creation response's Location URI to retrieve all saved
note details and current recipient details through GET.

## Scope

- Add GetNote under service/src/Core/UseCases/Notes and
  GET /api/delivery-notes/{id} in the capability route module. Reuse the T001
  aggregate/response DTO, existing repository query facilities, standard envelope,
  TechnicalUseCaseLogger<T>, and cancellation conventions.
- Load the note with contact/package/deadline/line records and its current
  recipient; do not return unloaded navigation properties or build a historical
  recipient snapshot. Preserve submitted line order using explicit persisted
  order if T001's mapping does not already guarantee it.
- Add the Location header to POST, pointing to the new GET resource. Describe it
  in POST's OpenAPI metadata in the same change.
- Add detail errors and complete the POST-to-GET workflow in the existing HTTP
  test language. Do not add listing, update, status transitions, user attribution,
  recipient mutation, or child routes to make a test possible.

## Acceptance criteria

- [ ] Following Location after successful creation returns 200 with the expected
  standard envelope and the same note ID, number, Pending status, and creation
  timestamp.
- [ ] A new GET request returns the normalized, persisted header/flags, complete
  contact/package/deadline details, and every line, including repeated part codes,
  optional text, decimal values, server child IDs, and original line order.
- [ ] All three deadlines round-trip as the same yyyy-MM-dd calendar dates using
  DateOnly response properties, without offsets or timezone conversion.
- [ ] Identical valid POSTs create separate notes; both can be retrieved through
  their distinct Location URIs with their own IDs/numbers and complete details.
- [ ] GET exposes the referenced recipient's current details using the existing
  recipient DTO. No recipient snapshot is persisted on the note. Reference
  loading is verified using the recipient created through HTTP; no recipient
  update endpoint is required or added by this task.
- [ ] Unknown integer note IDs, including zero and negative values, return 404
  with a stable Note error code. Malformed/out-of-range integer route input
  returns 400 using the current HTTP error convention.
- [ ] Creation remains anonymous and GET is anonymous. Reads do not change the
  stored note or assign new identifiers/timestamps. Anonymous GET returns full
  contact/current recipient details; access control is explicitly deferred.
- [ ] For representative failed validation and missing-recipient requests in a
  fresh identity-controlled test, GET confirms that no note was created at the
  next candidate ID before any subsequent valid creation is made.
- [ ] Generated OpenAPI documents GET's route parameter, response fields,
  authentication, 200/400/404, and POST's new Location response header.

## Verification

Use TDD at the agreed HTTP boundary: create the recipient, POST a distinctive full
note, then GET via Location and compare the complete saved details. Include
multiple lines whose values and order are deliberately different, positive
fractional quantities, nondefault handling flags, optional text, and equal/past
dates. Verify optional blank-text normalization and UTC timestamp consistency
through a fresh HTTP request rather than a tracked EF entity.

Exercise missing/malformed identifiers and rejected-creation readback. Inspect
the live generated OpenAPI in a test host with Swagger enabled before startup.
Run focused creation/detail/documentation and recipient tests, excluding
Database=PostgreSQL. Format this task's changed C# files and verify formatting
after the last edit. Record exact commands and red/green evidence. No container
execution is part of routine verification.

## Progress

No implementation started. Record dated work, verification evidence, and remaining
work here during implementation.
