# US-003: Human acceptance 04

- Review type: human acceptance
- Acceptance: approved
- Approval date: 2026-11-11 (+01:00)
- Authoritative story: `docs/backlog/US-003-update-recipients.md`
- Plan: `docs/backlog/US-003-update-recipients/README.md`
- Approval statements: "Human review approved", followed by "Just close the user
  story as completed."
- Accepted agent review report: `docs/backlog/US-003-update-recipients/review-03.md`
  (PASS, complete coverage, committed in `70cf486`)
- Accepted reviewed implementation commit: `4cf7419ac1607a3ec5125ab1308a8b540e3cfa3c`
- Current implementation head before the completion commit:
  `f8de9b2` ("Human review", by the developer)
- Completion updates: story `In review` to `Done`; plan `active` to `complete`

## Unreviewed change accepted by the developer

Commit `f8de9b2` landed after review 03 and was not covered by an agent review.
It changes implementation and tests:

- `CreateRecipient` no longer turns a database unique-key conflict into
  400 `Recipient.AlreadyExists`. A concurrent duplicate now returns 409 with the
  common `UniqueKeyConflict` error code. Sequential duplicates still return 400.
- `RecipientConcurrencyTests` (the only PostgreSQL test) was deleted.

After being told this, the developer explicitly instructed that the story be
closed without a further agent review. US-001 AC11 was updated in the
completion commit to record the narrowed behavior. The change does not affect
the US-003 update endpoint or its acceptance criteria.

## Completion state

- Tasks T001 and T002 are done.
- Human review and manual verification are confirmed by the developer's approval.
- Non-blocking findings F008 and F009 from review 02 remain open by the
  developer's choice.
- No new tests were run for this record.
