import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { catchError, Observable, switchMap, throwError } from 'rxjs';
import { openErrorDialog } from '../../shared/error-dialog/error-dialog';
import { CreateRecipientForm } from '../create-recipient-form/create-recipient-form';
import { PagedResponse, RecipientDetails, RecipientsApi } from '../recipients-api';

@Component({
  imports: [CreateRecipientForm, MatButtonModule, MatProgressBarModule, MatTableModule],
  selector: 'app-recipients-page',
  templateUrl: './recipients-page.html',
})
export class RecipientsPage {
  private readonly api = inject(RecipientsApi);
  private readonly dialog = inject(MatDialog);

  private readonly page = signal(1);
  private readonly recipientsPage = rxResource({
    params: () => this.page(),
    stream: ({ params: page }) => this.loadRecipients(page),
  });

  protected readonly loading = this.recipientsPage.isLoading;
  protected readonly loadFailed = computed(() => this.recipientsPage.status() === 'error');
  protected readonly loadedPage = computed(() =>
    this.recipientsPage.hasValue() ? this.recipientsPage.value() : undefined,
  );
  protected readonly columns = ['code', 'name', 'country', 'zip', 'city', 'address', 'isForeign'];

  protected showFirstPage(): void {
    if (this.page() === 1) {
      this.recipientsPage.reload();
    } else {
      this.page.set(1);
    }
  }

  protected showPreviousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected showNextPage(): void {
    this.page.update((page) => page + 1);
  }

  protected retry(): void {
    this.recipientsPage.reload();
  }

  /** Loads a page; on failure, explains it in the error dialog and fails once that closes. */
  private loadRecipients(page: number): Observable<PagedResponse<RecipientDetails>> {
    return this.api
      .getRecipients(page)
      .pipe(
        catchError((error: HttpErrorResponse) =>
          openErrorDialog(this.dialog, error).pipe(switchMap(() => throwError(() => error))),
        ),
      );
  }
}
