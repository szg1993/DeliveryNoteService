import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { screen } from '@testing-library/angular';
import { routes } from './app.routes';

describe('app routes', () => {
  it('shows the Recipients page at /recipients when navigating to the start route', async () => {
    TestBed.configureTestingModule({
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    });
    const harness = await RouterTestingHarness.create();

    await harness.navigateByUrl('/');

    expect(await screen.findByRole('heading', { name: 'Recipients' })).toBeTruthy();
    expect(TestBed.inject(Router).url).toBe('/recipients');
  });
});
