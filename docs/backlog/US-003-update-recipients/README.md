# US-003: Update recipients

- Source: DeliveryNoteService/docs/backlog/US-003-update-recipients.md
- Refinement source: conversation: US-003 refinement, 2026-11-11
- Lifecycle: active
- Created: 2026-11-11
- Updated: 2026-11-11
- Branch: feature/US-003-update-recipients

## Approach

Deliver the complete `PUT /api/recipients/{id}` path first: the update DTO with
its validation, the `UpdateRecipient` use case, the endpoint with OpenAPI
metadata, and the DTO unit tests and HTTP tests that prove it. Validation and
not-found handling belong in this first increment because an update without them
would corrupt or misreport persisted data. It reuses existing patterns
(`CreateRecipient`, `GetRecipient`, `CreateRecipientDto`) and needs no new
infrastructure, so it fits one prompt.

The second increment proves the cross-story effect: a delivery note shows the
recipient's updated details. Delivery-note retrieval already loads the current
recipient (US-002), so this task is expected to be test-only; if the test passes
immediately, record that it characterizes existing behavior.

## Ordered tasks

| Order | Task | Status | Depends on | Increment |
| --- | --- | --- | --- | --- |
| 1 | [T001: Update an existing recipient](tasks/T001-update-existing-recipient.md) | todo | None | Skeleton |
| 2 | [T002: Show updated recipient on delivery notes](tasks/T002-show-updated-recipient-on-notes.md) | todo | T001 | Extension |

The task file owns its status and progress. Keep the status above synchronized
with the task file. T002 cannot proceed in parallel with T001.

## Acceptance coverage

| Source criterion | Tasks |
| --- | --- |
| AC1: 200 with updated recipient in envelope | T001 |
| AC2: update immediately visible through GET-by-ID and list | T001 |
| AC3: full replacement of five required fields | T001 |
| AC4: US-001 field rules applied unchanged | T001 |
| AC5: foreign status re-derived from country | T001 |
| AC6: code, ID, creation time unchanged; unknown properties ignored | T001 |
| AC7: invalid details return 400 and leave recipient unchanged | T001 |
| AC8: malformed ID 400; unknown ID (incl. zero/negative) 404 | T001 |
| AC9: delivery note shows updated recipient details | T002 |
| AC10: endpoint described in OpenAPI with status codes | T001 |

## External prerequisites

- US-001 recipient creation, retrieval, and listing: Done and merged in master.
- US-002 delivery-note creation and retrieval: Done and merged in master.

## Assumptions and open questions

No open questions. Agreed decisions are recorded in the source story.
Implementation defaults from the story apply: validate the body before the
recipient lookup, no `UpdatedAt` timestamp, anonymous access, no caching.

## Resume notes

- Reservation: attempt `32402555-339b-47a4-983a-8756f1531705`, remote `origin`,
  branch `feature/US-003-update-recipients`, marker
  `bd3a0644e92f9a6b55f5cc3d0298597b1557b3eb`.
- Routine API verification uses SQLite with `--filter "Database!=PostgreSQL"`;
  Testcontainers runs need explicit developer approval.
