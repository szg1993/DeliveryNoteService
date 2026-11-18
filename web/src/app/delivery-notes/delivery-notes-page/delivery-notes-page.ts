import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { catchError, Observable, switchMap, throwError } from 'rxjs';
import { openErrorDialog } from '../../shared/error-dialog/error-dialog';
import { PagedResponse } from '../../shared/paged-response';
import { DeliveryNoteDetails, DeliveryNotesApi } from '../delivery-notes-api';

@Component({
  imports: [
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatProgressBarModule,
    MatTableModule,
    RouterLink,
  ],
  selector: 'app-delivery-notes-page',
  styleUrl: './delivery-notes-page.scss',
  templateUrl: './delivery-notes-page.html',
})
export class DeliveryNotesPage {
  private readonly api = inject(DeliveryNotesApi);
  private readonly dialog = inject(MatDialog);

  private readonly page = signal(1);
  private readonly notesPage = rxResource({
    params: () => this.page(),
    stream: ({ params: page }) => this.loadDeliveryNotes(page),
  });

  protected readonly loading = this.notesPage.isLoading;
  protected readonly loadFailed = computed(() => this.notesPage.status() === 'error');
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

  protected showPreviousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected showNextPage(): void {
    this.page.update((page) => page + 1);
  }

  protected retry(): void {
    this.notesPage.reload();
  }

  /** Loads a page; on failure, explains it in the error dialog and fails once that closes. */
  private loadDeliveryNotes(page: number): Observable<PagedResponse<DeliveryNoteDetails>> {
    return this.api
      .getDeliveryNotes(page)
      .pipe(
        catchError((error: HttpErrorResponse) =>
          openErrorDialog(this.dialog, error).pipe(switchMap(() => throwError(() => error))),
        ),
      );
  }
}
