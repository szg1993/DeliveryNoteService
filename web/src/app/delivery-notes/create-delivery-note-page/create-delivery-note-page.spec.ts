import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { fireEvent, screen, waitFor, within } from '@testing-library/angular';
import { answerEmptyListRequest } from '../../../testing/http';
import { routes } from '../../app.routes';
import { RecipientDetails } from '../../recipients/recipients-api';
import { DeliveryNoteDetails, NewDeliveryNote } from '../delivery-notes-api';

describe('CreateDeliveryNotePage', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('creates a note with two lines as entered and returns to the list', async () => {
    await openCreationPage();
    await answerRecipientsRequest([
      recipient(1, 'BUDA01', 'Buda Logistics'),
      recipient(7, 'WIEN01', 'Wien Handel'),
    ]);

    await fillValidNote();
    fillFields(section('Header'), { 'Takeover place': ' Győr ' });
    checkFlag('Urgent');
    checkFlag('Own cost');
    fillFields(line(1), { 'Part code': 'P-100', 'Comment (optional)': 'Fragile' });
    await addLine();
    fillFields(line(2), { ...validLine, 'Part code': 'P-200', Quantity: '2.5' });
    await submitForm();
    answerCreateRequest(
      {
        ekaerNbr: null,
        takeoverPlace: ' Győr ',
        category: 'Parts',
        comments: null,
        isUrgent: true,
        isCrackable: false,
        isDangerous: false,
        isOwnCost: true,
        recipientId: 7,
        contactInfo: {
          assignTo: 'Anna Kovács',
          assignToPhone: '+36 1 234 5678',
          contact: 'Béla Nagy',
          contactPhone: '+36 1 876 5432',
        },
        packageDetail: {
          tareWgt: 12.5,
          tareWgtUm: 'kg',
          pkgQty: 3,
          pkgScale: 'box',
          pkgSizeX: 1.2,
          pkgSizeY: 0.8,
          pkgSizeZ: 0.5,
          pkgSizeUm: 'm',
        },
        deadline: {
          takeoverDate: '2026-11-20',
          shipDate: '2026-11-21',
          estimatedArrivalDate: '2026-11-23',
        },
        noteLines: [
          {
            partCode: 'P-100',
            partDesc: 'Gear',
            partCmt: 'Fragile',
            partQty: 4,
            partQtyUm: 'pcs',
            partWgt: 1.75,
            partWgtUm: 'kg',
          },
          {
            partCode: 'P-200',
            partDesc: 'Gear',
            partCmt: null,
            partQty: 2.5,
            partQtyUm: 'pcs',
            partWgt: 1.75,
            partWgtUm: 'kg',
          },
        ],
      },
      'DN-000042',
    );
    await answerEmptyListRequest('/api/delivery-notes?page=1&limit=10&sortBy=Id&sortDir=Desc');

    expect(await screen.findByText('Delivery note DN-000042 created')).toBeTruthy();
    expect(screen.getByRole('heading', { level: 1, name: 'Delivery notes' })).toBeTruthy();
    expect(TestBed.inject(Router).url).toBe('/delivery-notes');
  });

  it('shows an inline error and sends nothing when a required value is missing', async () => {
    await openCreationPage();
    await answerRecipientsRequest([recipient(7, 'WIEN01', 'Wien Handel')]);
    await fillValidNote();
    fillFields(section('Header'), { Category: '' });

    await submitForm();

    expect(await screen.findByText('Category is required.')).toBeTruthy();
    TestBed.inject(HttpTestingController).expectNone({
      method: 'POST',
      url: '/api/delivery-notes',
    });
  });

  it('shows the date-order error and sends nothing when shipping precedes takeover', async () => {
    await openCreationPage();
    await answerRecipientsRequest([recipient(7, 'WIEN01', 'Wien Handel')]);
    await fillValidNote();
    fillFields(section('Deadlines'), { 'Takeover date': '11/22/2026', 'Ship date': '11/21/2026' });

    await submitForm();

    expect(
      await screen.findByText('Ship date must not be earlier than takeover date.'),
    ).toBeTruthy();
    TestBed.inject(HttpTestingController).expectNone({
      method: 'POST',
      url: '/api/delivery-notes',
    });
  });
});

async function openCreationPage(): Promise<void> {
  TestBed.configureTestingModule({
    providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
  });
  const harness = await RouterTestingHarness.create();
  await harness.navigateByUrl('/delivery-notes/new');
}

/** Expects the request for every recipient and answers it with the given ones. */
async function answerRecipientsRequest(recipients: RecipientDetails[]): Promise<void> {
  const request = await waitFor(() =>
    TestBed.inject(HttpTestingController).expectOne('/api/recipients?limit=0'),
  );
  request.flush({
    content: recipients,
    paging: { page: 1, limit: 0, totalCount: recipients.length, totalPages: 1 },
  });
  await TestBed.inject(ApplicationRef).whenStable();
}

/** Expects one POST with exactly the given body and answers it with 201 and the note number. */
function answerCreateRequest(body: NewDeliveryNote, noteNbr: string): void {
  const request = TestBed.inject(HttpTestingController).expectOne({
    method: 'POST',
    url: '/api/delivery-notes',
  });
  expect(request.request.body).toEqual(body);
  request.flush({ content: createdNote(noteNbr) }, { status: 201, statusText: 'Created' });
}

const validContact = {
  'Assigned to': 'Anna Kovács',
  'Assignee phone': '+36 1 234 5678',
  'Contact person': 'Béla Nagy',
  'Contact phone': '+36 1 876 5432',
};

const validPackage = {
  'Tare weight': '12.5',
  'Tare weight unit': 'kg',
  'Package quantity': '3',
  'Package scale': 'box',
  'Length (X)': '1.2',
  'Width (Y)': '0.8',
  'Height (Z)': '0.5',
  'Size unit': 'm',
};

const validLine = {
  'Part code': 'P-1',
  Description: 'Gear',
  Quantity: '4',
  'Quantity unit': 'pcs',
  Weight: '1.75',
  'Weight unit': 'kg',
};

/** Chooses the only recipient and fills every required field with a value that passes. */
async function fillValidNote(): Promise<void> {
  await chooseRecipient('WIEN01 – Wien Handel');
  fillFields(section('Header'), { Category: 'Parts', 'Takeover place': 'Győr' });
  fillFields(section('Contact'), validContact);
  fillFields(section('Package'), validPackage);
  fillFields(section('Deadlines'), {
    'Takeover date': '11/20/2026',
    'Ship date': '11/21/2026',
    'Estimated arrival date': '11/23/2026',
  });
  fillFields(line(1), validLine);
}

function section(title: string): HTMLElement {
  return screen.getByRole('region', { name: title });
}

/** The input labelled exactly `label`, ignoring the required marker. */
function input(container: HTMLElement, label: string): HTMLInputElement {
  const escaped = label.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  return within(container).getByLabelText<HTMLInputElement>(
    new RegExp(`^\\s*${escaped}\\s*\\*?\\s*$`),
  );
}

/** Types each value into the input with that label inside the container. */
function fillFields(container: HTMLElement, values: Record<string, string>): void {
  for (const [label, value] of Object.entries(values)) {
    fireEvent.input(input(container, label), { target: { value } });
  }
}

function line(position: number): HTMLElement {
  return screen.getByRole('group', { name: `Line ${position}` });
}

async function chooseRecipient(option: string): Promise<void> {
  fireEvent.click(screen.getByRole('combobox', { name: /Recipient/ }));
  fireEvent.click(await screen.findByRole('option', { name: option }));
}

function checkFlag(name: string): void {
  fireEvent.click(screen.getByRole('checkbox', { name }));
}

async function addLine(): Promise<void> {
  fireEvent.click(screen.getByRole('button', { name: 'Add line' }));
  await TestBed.inject(ApplicationRef).whenStable();
}

async function submitForm(): Promise<void> {
  fireEvent.click(screen.getByRole('button', { name: 'Create delivery note' }));
  await TestBed.inject(ApplicationRef).whenStable();
}

function recipient(id: number, code: string, name: string): RecipientDetails {
  return {
    id,
    code,
    name,
    country: 'HU',
    zip: '1011',
    city: 'Budapest',
    address: 'Fő utca 1.',
    createdAt: '2026-11-17T10:00:00Z',
    isForeign: false,
  };
}

/** The created note the backend returns; only its number is shown. */
function createdNote(noteNbr: string): DeliveryNoteDetails {
  return {
    id: 42,
    noteNbr,
    createdDate: '2026-11-18T10:00:00Z',
    noteStatusId: 'Pending',
    recipientId: 7,
    recipient: { id: 7, code: 'WIEN01', name: 'Wien Handel' },
    category: 'Parts',
    takeoverPlace: ' Győr ',
    isUrgent: true,
    deadline: {
      takeoverDate: '2026-11-20',
      shipDate: '2026-11-21',
      estimatedArrivalDate: '2026-11-23',
    },
  };
}
