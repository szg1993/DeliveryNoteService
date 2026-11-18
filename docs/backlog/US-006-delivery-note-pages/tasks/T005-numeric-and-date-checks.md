# T005: Check numbers and date order before submitting

- Parent: [US-006](../README.md)
- Status: todo
- Depends on: T004
- Increment: Edge case

## Outcome

The form catches invalid quantities, weights, dimensions, dates, and date order
inline before sending a request, because the backend reports these only as the
field-less `Note.InvalidInput`.

## Scope

In the creation page's Signal Forms schema:

- Package quantity and line quantities must be greater than 0.
- Tare weight, line weights, and the three dimensions must be 0 or more.
- Date order: takeover date ≤ ship date ≤ estimated arrival date; equal dates are
  allowed. Show the error inline on the later date of a violated pair.
- Typed date text the native adapter cannot parse shows "Enter a valid date."
  inline. Verify how the datepicker's parse error surfaces through Signal Forms and
  use normal framework behavior.

Any of these errors blocks submission without a request. The backend remains
authoritative.

## Acceptance criteria

- [ ] A ship date earlier than the takeover date shows the date-order error and
      sends no request.
- [ ] An estimated arrival date earlier than the ship date shows the date-order
      error and sends no request.
- [ ] Zero or negative quantities and negative weights or dimensions show inline
      errors and send no request; zero weights and dimensions are accepted.
- [ ] Unparseable typed date text shows "Enter a valid date.".

## Verification

TDD: agreed test 6, a ship date earlier than the takeover date shows the
date-order error and sends no request. Keep test 4 green. Check numeric minimums
and the invalid-date message manually. Then run lint, test, build, and format check
in `web/`.

## Progress

No implementation started. Record dated work, verification evidence and remaining work here during implementation.
