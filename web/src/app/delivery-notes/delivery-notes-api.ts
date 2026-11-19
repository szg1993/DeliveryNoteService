import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { map, Observable } from 'rxjs';
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

/**
 * The details a user enters to create a note, shaped like the backend's `CreateDeliveryNoteDto`.
 * Like the backend contract, numbers and dates may be `null`; the backend rejects missing ones.
 */
export interface NewDeliveryNote {
  ekaerNbr: string | null;
  takeoverPlace: string;
  category: string;
  comments: string | null;
  isUrgent: boolean;
  isCrackable: boolean;
  isDangerous: boolean;
  isOwnCost: boolean;
  recipientId: number;
  contactInfo: NewContactInfo;
  packageDetail: NewPackageDetail;
  deadline: NewDeadline;
  noteLines: NewNoteLine[];
}

/** Dates in `yyyy-MM-dd`. */
export interface NewDeadline {
  takeoverDate: string | null;
  shipDate: string | null;
  estimatedArrivalDate: string | null;
}

export interface NewContactInfo {
  assignTo: string;
  assignToPhone: string;
  contact: string;
  contactPhone: string;
}

export interface NewPackageDetail {
  tareWgt: number | null;
  tareWgtUm: string;
  pkgQty: number | null;
  pkgScale: string;
  pkgSizeX: number | null;
  pkgSizeY: number | null;
  pkgSizeZ: number | null;
  pkgSizeUm: string;
}

export interface NewNoteLine {
  partCode: string;
  partDesc: string;
  partCmt: string | null;
  partQty: number | null;
  partQtyUm: string;
  partWgt: number | null;
  partWgtUm: string;
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

  createDeliveryNote(details: NewDeliveryNote): Observable<DeliveryNoteDetails> {
    return this.http
      .post<{ content: DeliveryNoteDetails }>('/api/delivery-notes', details)
      .pipe(map((response) => response.content));
  }
}
