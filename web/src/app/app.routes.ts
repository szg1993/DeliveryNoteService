import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'delivery-notes' },
  {
    path: 'delivery-notes',
    title: 'Delivery notes',
    loadComponent: () =>
      import('./delivery-notes/delivery-notes-page/delivery-notes-page').then(
        (m) => m.DeliveryNotesPage,
      ),
  },
  {
    path: 'delivery-notes/new',
    title: 'New delivery note',
    loadComponent: () =>
      import('./delivery-notes/create-delivery-note-page/create-delivery-note-page').then(
        (m) => m.CreateDeliveryNotePage,
      ),
  },
  {
    path: 'recipients',
    title: 'Recipients',
    loadComponent: () =>
      import('./recipients/recipients-page/recipients-page').then((m) => m.RecipientsPage),
  },
];
