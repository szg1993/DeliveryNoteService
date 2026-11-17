import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { fireEvent, render, screen, within } from '@testing-library/angular';
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
    expect(screen.queryByRole('navigation', { name: 'Recipient pages' })).toBeNull();
  });

  it('requests the following page when next is selected', async () => {
    await renderRecipientsPage();
    await answerRecipientsRequest({ page: 1, totalPages: 3 }, [recipientWithCode('FIRST1')]);

    fireEvent.click(pagingButton('Next page'));
    await answerRecipientsRequest({ page: 2, totalPages: 3 }, [recipientWithCode('SECOND')]);

    expect(listedCodes()).toEqual(['SECOND']);
  });

  it('requests the preceding page when previous is selected', async () => {
    await renderRecipientsPage();
    await answerRecipientsRequest({ page: 1, totalPages: 3 }, [recipientWithCode('FIRST1')]);
    fireEvent.click(pagingButton('Next page'));
    await answerRecipientsRequest({ page: 2, totalPages: 3 }, [recipientWithCode('SECOND')]);

    fireEvent.click(pagingButton('Previous page'));
    await answerRecipientsRequest({ page: 1, totalPages: 3 }, [recipientWithCode('FIRST1')]);

    expect(listedCodes()).toEqual(['FIRST1']);
  });

  it('shows the current page and the total pages', async () => {
    await renderRecipientsPage();

    await answerRecipientsRequest({ page: 1, totalPages: 3 }, [recipientWithCode('FIRST1')]);

    expect(screen.getByText('Page 1 of 3')).toBeTruthy();
  });

  it('disables previous on the first page', async () => {
    await renderRecipientsPage();

    await answerRecipientsRequest({ page: 1, totalPages: 2 }, [recipientWithCode('FIRST1')]);

    expect(pagingButton('Previous page').disabled).toBe(true);
    expect(pagingButton('Next page').disabled).toBe(false);
  });

  it('disables next on the last page', async () => {
    await renderRecipientsPage();
    await answerRecipientsRequest({ page: 1, totalPages: 2 }, [recipientWithCode('FIRST1')]);

    fireEvent.click(pagingButton('Next page'));
    await answerRecipientsRequest({ page: 2, totalPages: 2 }, [recipientWithCode('SECOND')]);

    expect(pagingButton('Previous page').disabled).toBe(false);
    expect(pagingButton('Next page').disabled).toBe(true);
  });
});

async function renderRecipientsPage(): Promise<void> {
  await render(RecipientsPage, {
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
}

/** Expects the newest-first list request for the page and answers it with the recipients. */
async function answerRecipientsRequest(
  { page, totalPages }: { page: number; totalPages?: number },
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
        totalPages: totalPages ?? (recipients.length > 0 ? 1 : 0),
      },
    });
  await TestBed.inject(ApplicationRef).whenStable();
}

let nextRecipientId = 1;

function recipient(details: Omit<RecipientDetails, 'id' | 'createdAt'>): RecipientDetails {
  return { id: nextRecipientId++, createdAt: '2026-11-17T10:00:00Z', ...details };
}

/** A recipient for scenarios where only its code identifies the row. */
function recipientWithCode(code: string): RecipientDetails {
  return recipient({
    code,
    name: 'Buda Logistics',
    country: 'HU',
    zip: '1011',
    city: 'Budapest',
    address: 'Fő utca 1.',
    isForeign: false,
  });
}

/** Codes of the listed recipients, from the first table column. */
function listedCodes(): string[] {
  return tableRowTexts()
    .slice(1)
    .map(([code]) => code);
}

function pagingButton(name: 'Previous page' | 'Next page'): HTMLButtonElement {
  return screen.getByRole<HTMLButtonElement>('button', { name });
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
