import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { render, screen, waitFor } from '@testing-library/angular';
import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('routes to each page from the toolbar links', async () => {
    await render(App, {
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    });

    clickToolbarLink('Recipients');
    await answerEmptyListRequest('/api/recipients?page=1&limit=10&sortBy=Id&sortDir=Desc');

    expect(screen.getByRole('heading', { level: 1, name: 'Recipients' })).toBeTruthy();

    clickToolbarLink('Delivery notes');
    await answerEmptyListRequest('/api/delivery-notes?page=1&limit=10&sortBy=Id&sortDir=Desc');

    expect(screen.getByRole('heading', { level: 1, name: 'Delivery notes' })).toBeTruthy();
  });
});

function clickToolbarLink(name: string): void {
  screen.getByRole('link', { name }).click();
}

/** Waits for the lazily loaded page's first list request and answers it with no items. */
async function answerEmptyListRequest(url: string): Promise<void> {
  const request = await waitFor(() => TestBed.inject(HttpTestingController).expectOne(url));
  request.flush({ content: [], paging: { page: 1, limit: 10, totalCount: 0, totalPages: 0 } });
  await TestBed.inject(ApplicationRef).whenStable();
}
