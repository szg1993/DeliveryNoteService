import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { map, Observable } from 'rxjs';

export interface RecipientDetails {
  id: number;
  code: string;
  name: string;
  country: string;
  zip: string;
  city: string;
  address: string;
  createdAt: string;
  isForeign: boolean;
}

/** The details a user enters to register a recipient. */
export interface NewRecipient {
  code: string;
  name: string;
  country: string;
  zip: string;
  city: string;
  address: string;
}

export interface Paging {
  page: number;
  limit: number;
  totalCount: number;
  totalPages: number;
}

export interface PagedResponse<T> {
  content: T[];
  paging: Paging;
}

const pageSize = 10;

@Service()
export class RecipientsApi {
  private readonly http = inject(HttpClient);

  /** Returns one page of recipients, newest first. */
  getRecipients(page: number): Observable<PagedResponse<RecipientDetails>> {
    return this.http.get<PagedResponse<RecipientDetails>>('/api/recipients', {
      params: { page, limit: pageSize, sortBy: 'Id', sortDir: 'Desc' },
    });
  }

  createRecipient(details: NewRecipient): Observable<RecipientDetails> {
    return this.http
      .post<{ content: RecipientDetails }>('/api/recipients', details)
      .pipe(map((response) => response.content));
  }
}
