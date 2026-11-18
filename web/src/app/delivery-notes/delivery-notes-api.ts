import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResponse } from '../shared/paged-response';

export type NoteStatus = 'Pending' | 'Approved' | 'Rejected';

/** Dates in `yyyy-MM-dd`, as the backend returns them. */
export interface NoteDeadline {
  takeoverDate: string;
  shipDate: string;
  estimatedArrivalDate: string;
}

export interface NoteRecipient {
  id: number;
  code: string;
  name: string;
}

/**
 * The delivery note fields the pages use. The backend returns more, such as contact,
 * package, and line details.
 */
export interface DeliveryNoteDetails {
  id: number;
  noteNbr: string;
  createdDate: string;
  noteStatusId: NoteStatus;
  recipientId: number;
  recipient: NoteRecipient;
  category: string;
  takeoverPlace: string;
  isUrgent: boolean;
  deadline: NoteDeadline;
}

const pageSize = 10;

@Service()
export class DeliveryNotesApi {
  private readonly http = inject(HttpClient);

  /** Returns one page of delivery notes, newest first. */
  getDeliveryNotes(page: number): Observable<PagedResponse<DeliveryNoteDetails>> {
    return this.http.get<PagedResponse<DeliveryNoteDetails>>('/api/delivery-notes', {
      params: { page, limit: pageSize, sortBy: 'Id', sortDir: 'Desc' },
    });
  }
}
