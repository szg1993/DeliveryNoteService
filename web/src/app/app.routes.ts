import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'recipients' },
  {
    path: 'recipients',
    title: 'Recipients',
    loadComponent: () =>
      import('./recipients/recipients-page/recipients-page').then((m) => m.RecipientsPage),
  },
];
