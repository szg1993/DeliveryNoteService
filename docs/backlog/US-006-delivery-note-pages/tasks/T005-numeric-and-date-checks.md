# T005: Check numbers and date order before submitting

- Parent: [US-006](../README.md)
- Status: done
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

- [x] A ship date earlier than the takeover date shows the date-order error and
      sends no request.
- [x] An estimated arrival date earlier than the ship date shows the date-order
      error and sends no request.
- [x] Zero or negative quantities and negative weights or dimensions show inline
      errors and send no request; zero weights and dimensions are accepted.
- [x] Unparseable typed date text shows "Enter a valid date.".

## Verification

TDD: agreed test 6, a ship date earlier than the takeover date shows the
date-order error and sends no request. Keep test 4 green. Check numeric minimums
and the invalid-date message manually. Then run lint, test, build, and format check
in `web/`.

## Progress

2026-11-18: Implemented and verified.

- Schema rules in `CreateDeliveryNotePage`: `requiredPositive` (required plus a
  `validate` rule, "{Label} must be greater than 0.") for package and line quantities;
  `requiredNotNegative` (required plus built-in `min(0)`, "{Label} must be 0 or more.")
  for tare weight, line weights, and the three dimensions; `notEarlierThan` (built-in
  `minDate` with the earlier date as a logic function, equal dates pass, nothing checked
  until both are set) for "Ship date must not be earlier than takeover date." and
  "Estimated arrival date must not be earlier than ship date.", each on the later date.
- Agreed test 6 in `create-delivery-note-page.spec.ts` (ship 11/21 before takeover 11/22).
  It failed first (no message, a POST was sent), then passed. Test 4 stays green.

Invalid-date outcome (normal framework behavior, verified with a temporary probe test,
then removed): Signal Forms' CVA interop reads the `MatDatepickerInput`'s
`NG_VALIDATORS` as parse errors, so unparseable text yields a `matDatepickerParse` error
without a message, which also blocks submission; the model becomes `null`, so
"required" fires as well. Because `minDate` publishes `min` to the datepicker's `min`
input, the datepicker's own `matDatepickerMin` validator adds a second, message-less
date-order error. `dateErrorMessages` (a `computed` per date field) therefore shows only
"Enter a valid date." when the parse error is present and otherwise only errors that
carry a message.

Manual check (2026-11-18, browser pane, `ng serve --port 4300 --proxy-config` pointing
`/api` at a local Node stub on port 5299; the real backend was never contacted):
tare weight 0, width 0, height 0 showed no error; package quantity 0 showed "Package
quantity must be greater than 0."; length -1 "Length must be 0 or more."; line quantity
-2 "Quantity must be greater than 0."; line weight -1 "Weight must be 0 or more.";
takeover and ship both 11/22/2026 showed no error; arrival 11/21/2026 showed "Estimated
arrival date must not be earlier than ship date." (after the fix, with no empty
duplicate); takeover "abc" showed only "Enter a valid date."; submitting sent no POST
(stub log showed only GETs). Observation: numeric errors appeared on blur, while the
datepicker fields' errors appeared once the form was submitted.

Verification in `web/`: `npm run lint` passed; `npm test` 19/19 passed (5 files);
`npm run build` succeeded; `npm run format:check` passed after `prettier --write` on
the changed files.

Refactoring assessment (clean-code): the new rules follow T004's small schema helpers
and reuse `requiredValue`; built-in `min` and `minDate` are used where they fit, with a
custom `validate` only for the exclusive minimum. `notEarlierThan` was reduced from four
arguments (two labels) to the two paths plus the complete message, so each rule reads
on one line group and the visible text is searchable. The three date fields share
`dateErrorMessages` instead of per-template filtering. In tests, test 6 reuses
`fillValidNote()`, `fillFields`, `section`, and `submitForm`; only the two
distinguishing dates are visible. Its "expect no POST" assertion repeats test 5's two
lines; two consumers in one file did not justify a helper. No shared test
infrastructure changed: the helpers stay local to the creation spec (stateless,
per-test TestBed), and no other spec exercises this form.

Remaining: none for T005.
