import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { render, screen, waitFor } from '@testing-library/angular';
import { tableRowTexts } from '../../../testing/table';
import { DeliveryNoteDetails } from '../delivery-notes-api';
import { DeliveryNotesPage } from './delivery-notes-page';

describe('DeliveryNotesPage', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('lists the newest delivery notes from the backend', async () => {
    await renderDeliveryNotesPage();

    await answerDeliveryNotesRequest([
      deliveryNote({
        noteNbr: 'DN-000002',
        recipient: { code: 'BUDA01', name: 'Buda Logistics' },
        category: 'Parts',
        takeoverPlace: 'Győr',
        shipDate: '2026-11-20',
        estimatedArrivalDate: '2026-11-22',
        noteStatusId: 'Pending',
        isUrgent: true,
      }),
      deliveryNote({
        noteNbr: 'DN-000001',
        recipient: { code: 'WIEN01', name: 'Wien Handel' },
        category: 'Tools',
        takeoverPlace: 'Budapest',
        shipDate: '2026-11-19',
        estimatedArrivalDate: '2026-11-21',
        noteStatusId: 'Approved',
        isUrgent: false,
      }),
    ]);

    expect(tableRowTexts()).toEqual([
      [
        'Note number',
        'Recipient',
        'Category',
        'Takeover place',
        'Ship date',
        'Estimated arrival',
        'Status',
        'Urgent',
      ],
      [
        'DN-000002',
        'BUDA01 – Buda Logistics',
        'Parts',
        'Győr',
        '2026-11-20',
        '2026-11-22',
        'Pending',
        'Urgent',
      ],
      [
        'DN-000001',
        'WIEN01 – Wien Handel',
        'Tools',
        'Budapest',
        '2026-11-19',
        '2026-11-21',
        'Approved',
        '',
      ],
    ]);
  });

  it('shows an empty-state message when there are no delivery notes', async () => {
    await renderDeliveryNotesPage();

    await answerDeliveryNotesRequest([]);

    expect(screen.getByText('No delivery notes yet.')).toBeTruthy();
    expect(screen.queryByRole('table')).toBeNull();
  });
});

async function renderDeliveryNotesPage(): Promise<void> {
  await render(DeliveryNotesPage, {
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
}

/** Expects the newest-first request for the first page and answers it with the notes. */
async function answerDeliveryNotesRequest(notes: DeliveryNoteDetails[]): Promise<void> {
  const request = await waitFor(() =>
    TestBed.inject(HttpTestingController).expectOne(
      '/api/delivery-notes?page=1&limit=10&sortBy=Id&sortDir=Desc',
    ),
  );
  request.flush({
    content: notes,
    paging: { page: 1, limit: 10, totalCount: notes.length, totalPages: notes.length > 0 ? 1 : 0 },
  });
  await TestBed.inject(ApplicationRef).whenStable();
}

let nextNoteId = 1;

/** A note with the listed values; fields the list does not show get fixed values. */
function deliveryNote(listed: {
  noteNbr: string;
  recipient: { code: string; name: string };
  category: string;
  takeoverPlace: string;
  shipDate: string;
  estimatedArrivalDate: string;
  noteStatusId: DeliveryNoteDetails['noteStatusId'];
  isUrgent: boolean;
}): DeliveryNoteDetails {
  const id = nextNoteId++;
  return {
    id,
    noteNbr: listed.noteNbr,
    createdDate: '2026-11-18T10:00:00Z',
    noteStatusId: listed.noteStatusId,
    recipientId: id,
    recipient: { id, ...listed.recipient },
    category: listed.category,
    takeoverPlace: listed.takeoverPlace,
    isUrgent: listed.isUrgent,
    deadline: {
      takeoverDate: listed.shipDate,
      shipDate: listed.shipDate,
      estimatedArrivalDate: listed.estimatedArrivalDate,
    },
  };
}
