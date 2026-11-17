import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { fireEvent, render, screen, waitFor, within } from '@testing-library/angular';
import { NewRecipient, RecipientDetails } from '../recipients-api';
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

  it('creates a recipient with the values as entered and reloads the first page', async () => {
    await renderRecipientsPage();
    await answerRecipientsRequest({ page: 1 }, [recipientWithCode('OLDER1')]);
    const details = { ...validRecipient, code: 'BUDA01', name: ' Buda Logistics ' };

    fillRecipientForm(details);
    fireEvent.click(createButton());
    answerCreateRequest(details);
    await answerRecipientsRequest({ page: 1 }, [recipientWithCode('BUDA01')]);

    expect(await screen.findByText('Recipient BUDA01 created')).toBeTruthy();
    expect(recipientFormValues()).toEqual(emptyRecipientForm);
    expect(listedCodes()).toEqual(['BUDA01']);
  });

  it('disables create and ignores another submit while the request is pending', async () => {
    await renderRecipientsPage();
    await answerRecipientsRequest({ page: 1 }, []);
    fillRecipientForm(validRecipient);

    fireEvent.click(createButton());
    fireEvent.click(createButton());

    await waitFor(() => expect(createButton().disabled).toBe(true));
    answerCreateRequest(validRecipient);
    await answerRecipientsRequest({ page: 1 }, [recipientWithCode('BUDA01')]);
  });

  it('shows an inline error and sends nothing when a required value is missing', async () => {
    await renderRecipientsPage();
    await answerRecipientsRequest({ page: 1 }, []);
    fillRecipientForm({ ...validRecipient, code: '' });

    fireEvent.click(createButton());

    expect(await screen.findByText('Code is required.')).toBeTruthy();
    expectNoCreateRequest();
  });

  it('shows an inline error and sends nothing when a value is too long', async () => {
    await renderRecipientsPage();
    await answerRecipientsRequest({ page: 1 }, []);
    fillRecipientForm({ ...validRecipient, country: 'HUN' });

    fireEvent.click(createButton());

    expect(await screen.findByText('Country must be at most 2 characters.')).toBeTruthy();
    expectNoCreateRequest();
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
  const request = await waitFor(() =>
    TestBed.inject(HttpTestingController).expectOne(
      `/api/recipients?page=${page}&limit=10&sortBy=Id&sortDir=Desc`,
    ),
  );
  request.flush({
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

/** Expects one POST with exactly the entered details and answers it with 201. */
function answerCreateRequest(details: NewRecipient): void {
  const request = TestBed.inject(HttpTestingController).expectOne({
    method: 'POST',
    url: '/api/recipients',
  });
  expect(request.request.body).toEqual(details);
  request.flush(
    { content: recipient({ ...details, isForeign: false }) },
    { status: 201, statusText: 'Created' },
  );
}

function expectNoCreateRequest(): void {
  TestBed.inject(HttpTestingController).expectNone({ method: 'POST', url: '/api/recipients' });
}

/** Details that pass every client-side rule. */
const validRecipient: NewRecipient = {
  code: 'BUDA01',
  name: 'Buda Logistics',
  country: 'HU',
  zip: '1011',
  city: 'Budapest',
  address: 'Fő utca 1.',
};

const recipientFormLabels: Record<keyof NewRecipient, string> = {
  code: 'Code',
  name: 'Name',
  country: 'Country',
  zip: 'ZIP',
  city: 'City',
  address: 'Address',
};

const emptyRecipientForm: NewRecipient = {
  code: '',
  name: '',
  country: '',
  zip: '',
  city: '',
  address: '',
};

function recipientInput(field: keyof NewRecipient): HTMLInputElement {
  return screen.getByLabelText<HTMLInputElement>(recipientFormLabels[field], { exact: false });
}

/** Types the given values into the creation form; omitted fields stay as they are. */
function fillRecipientForm(details: Partial<NewRecipient>): void {
  for (const [field, value] of Object.entries(details) as [keyof NewRecipient, string][]) {
    fireEvent.input(recipientInput(field), { target: { value } });
  }
}

function recipientFormValues(): NewRecipient {
  return {
    code: recipientInput('code').value,
    name: recipientInput('name').value,
    country: recipientInput('country').value,
    zip: recipientInput('zip').value,
    city: recipientInput('city').value,
    address: recipientInput('address').value,
  };
}

function createButton(): HTMLButtonElement {
  return screen.getByRole<HTMLButtonElement>('button', { name: 'Create recipient' });
}

let nextRecipientId = 1;

function recipient(details: Omit<RecipientDetails, 'id' | 'createdAt'>): RecipientDetails {
  return { id: nextRecipientId++, createdAt: '2026-11-17T10:00:00Z', ...details };
}

/** A recipient for scenarios where only its code identifies the row. */
function recipientWithCode(code: string): RecipientDetails {
  return recipient({ ...validRecipient, code, isForeign: false });
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
