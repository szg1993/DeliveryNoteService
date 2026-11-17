import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, output, signal } from '@angular/core';
import { form, FormField, FormRoot, maxLength, required } from '@angular/forms/signals';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { firstValueFrom } from 'rxjs';
import { openErrorDialog } from '../../shared/error-dialog/error-dialog';
import { NewRecipient, RecipientDetails, RecipientsApi } from '../recipients-api';

interface RecipientField {
  key: keyof NewRecipient;
  label: string;
  /** The backend's length limit for the value. */
  maxLength: number;
}

const recipientFields: RecipientField[] = [
  { key: 'code', label: 'Code', maxLength: 10 },
  { key: 'name', label: 'Name', maxLength: 200 },
  { key: 'country', label: 'Country', maxLength: 2 },
  { key: 'zip', label: 'ZIP', maxLength: 20 },
  { key: 'city', label: 'City', maxLength: 50 },
  { key: 'address', label: 'Address', maxLength: 200 },
];

const emptyRecipient: NewRecipient = {
  code: '',
  name: '',
  country: '',
  zip: '',
  city: '',
  address: '',
};

@Component({
  imports: [FormField, FormRoot, MatButtonModule, MatFormFieldModule, MatInputModule],
  selector: 'app-create-recipient-form',
  templateUrl: './create-recipient-form.html',
})
export class CreateRecipientForm {
  private readonly api = inject(RecipientsApi);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly created = output<RecipientDetails>();

  protected readonly fields = recipientFields;
  private readonly recipient = signal(emptyRecipient);
  protected readonly recipientForm = form(
    this.recipient,
    (recipient) => {
      for (const field of recipientFields) {
        required(recipient[field.key], { message: `${field.label} is required.` });
        maxLength(recipient[field.key], field.maxLength, {
          message: `${field.label} must be at most ${field.maxLength} characters.`,
        });
      }
    },
    { submission: { action: () => this.createRecipient() } },
  );

  /** On failure, explains the error in the dialog and keeps the entered values for correction. */
  private async createRecipient(): Promise<void> {
    const details = this.recipient();
    try {
      this.confirmCreated(await firstValueFrom(this.api.createRecipient(details)));
    } catch (error) {
      if (!(error instanceof HttpErrorResponse)) {
        throw error;
      }
      openErrorDialog(this.dialog, error, details.code);
    }
  }

  private confirmCreated(created: RecipientDetails): void {
    this.snackBar.open(`Recipient ${created.code} created`, undefined, { duration: 5000 });
    this.recipientForm().reset(emptyRecipient);
    this.created.emit(created);
  }
}
