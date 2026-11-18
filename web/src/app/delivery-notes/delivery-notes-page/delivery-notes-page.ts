import { Component, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { DeliveryNotesApi } from '../delivery-notes-api';

@Component({
  imports: [MatCardModule, MatChipsModule, MatTableModule],
  selector: 'app-delivery-notes-page',
  styleUrl: './delivery-notes-page.scss',
  templateUrl: './delivery-notes-page.html',
})
export class DeliveryNotesPage {
  private readonly api = inject(DeliveryNotesApi);

  private readonly notesPage = rxResource({
    stream: () => this.api.getDeliveryNotes(1),
  });

  protected readonly loadedPage = computed(() =>
    this.notesPage.hasValue() ? this.notesPage.value() : undefined,
  );
  protected readonly columns = [
    'noteNbr',
    'recipient',
    'category',
    'takeoverPlace',
    'shipDate',
    'estimatedArrivalDate',
    'status',
    'urgent',
  ];
}
