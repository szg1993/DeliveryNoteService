import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { render, screen, within } from '@testing-library/angular';
import { RecipientDetails } from '../recipients-api';
import { RecipientsPage } from './recipients-page';

describe('RecipientsPage', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('lists the newest recipients from the backend', async () => {
    await renderRecipientsPage();

    await answerRecipientsRequest({ page: 1 }, [
      recipient({
        code: 'BUDA01',
        name: 'Buda Logistics',
        country: 'HU',
        zip: '1011',
        city: 'Budapest',
        address: 'Fő utca 1.',
        isForeign: false,
      }),
      recipient({
        code: 'WIEN01',
        name: 'Wien Handel',
        country: 'AT',
        zip: '1010',
        city: 'Wien',
        address: 'Ring 5',
        isForeign: true,
      }),
    ]);

    expect(tableRowTexts()).toEqual([
      ['Code', 'Name', 'Country', 'ZIP', 'City', 'Address', 'Foreign'],
      ['BUDA01', 'Buda Logistics', 'HU', '1011', 'Budapest', 'Fő utca 1.', 'No'],
      ['WIEN01', 'Wien Handel', 'AT', '1010', 'Wien', 'Ring 5', 'Yes'],
    ]);
  });

  it('shows an empty-state message when there are no recipients', async () => {
    await renderRecipientsPage();

    await answerRecipientsRequest({ page: 1 }, []);

    expect(screen.getByText('No recipients yet.')).toBeTruthy();
    expect(screen.queryAllByRole('row')).toEqual([]);
  });
});

async function renderRecipientsPage(): Promise<void> {
  await render(RecipientsPage, {
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
}

/** Expects the newest-first list request for the page and answers it with the recipients. */
async function answerRecipientsRequest(
  { page }: { page: number },
  recipients: RecipientDetails[],
): Promise<void> {
  TestBed.inject(HttpTestingController)
    .expectOne(`/api/recipients?page=${page}&limit=10&sortBy=Id&sortDir=Desc`)
    .flush({
      content: recipients,
      paging: {
        page,
        limit: 10,
        totalCount: recipients.length,
        totalPages: recipients.length > 0 ? 1 : 0,
      },
    });
  await TestBed.inject(ApplicationRef).whenStable();
}

let nextRecipientId = 1;

function recipient(details: Omit<RecipientDetails, 'id' | 'createdAt'>): RecipientDetails {
  return { id: nextRecipientId++, createdAt: '2026-11-17T10:00:00Z', ...details };
}

function tableRowTexts(): string[][] {
  return screen
    .queryAllByRole('row')
    .map((row) =>
      [...within(row).queryAllByRole('columnheader'), ...within(row).queryAllByRole('cell')].map(
        (cell) => cell.textContent?.trim() ?? '',
      ),
    );
}
