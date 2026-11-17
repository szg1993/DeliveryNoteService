import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { backendErrorCode, readableErrorMessage } from '../error-message';

export interface ErrorDialogData {
  message: string;
  errorCode?: string;
}

@Component({
  imports: [MatButtonModule, MatDialogModule],
  selector: 'app-error-dialog',
  styleUrl: './error-dialog.scss',
  templateUrl: './error-dialog.html',
})
export class ErrorDialog {
  protected readonly data = inject<ErrorDialogData>(MAT_DIALOG_DATA);
}

/** Explains a failed request in a modal dialog; the returned observable emits once it closes. */
export function openErrorDialog(
  dialog: MatDialog,
  error: HttpErrorResponse,
  recipientCode?: string,
): Observable<unknown> {
  const data: ErrorDialogData = {
    message: readableErrorMessage(error, recipientCode),
    errorCode: backendErrorCode(error),
  };
  return dialog.open(ErrorDialog, { data }).afterClosed();
}
