import { HttpTestingController } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { waitFor } from '@testing-library/angular';

/** Waits for a page's list request, such as after lazy loading, and answers it with no items. */
export async function answerEmptyListRequest(url: string): Promise<void> {
  const request = await waitFor(() => TestBed.inject(HttpTestingController).expectOne(url));
  request.flush({ content: [], paging: { page: 1, limit: 10, totalCount: 0, totalPages: 0 } });
  await TestBed.inject(ApplicationRef).whenStable();
}
