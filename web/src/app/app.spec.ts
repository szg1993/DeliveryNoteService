import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import { answerEmptyListRequest } from '../testing/http';
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
