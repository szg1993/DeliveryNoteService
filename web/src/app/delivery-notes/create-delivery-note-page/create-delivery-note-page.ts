import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, Signal, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import {
  applyEach,
  FieldTree,
  form,
  FormField,
  FormRoot,
  maxLength,
  min,
  minDate,
  required,
  SchemaPath,
  validate,
} from '@angular/forms/signals';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { catchError, firstValueFrom, Observable, throwError } from 'rxjs';
import { RecipientDetails, RecipientsApi } from '../../recipients/recipients-api';
import { openErrorDialog } from '../../shared/error-dialog/error-dialog';
import {
  DeliveryNoteDetails,
  DeliveryNotesApi,
  NewDeliveryNote,
  NewNoteLine,
} from '../delivery-notes-api';

/** What the user edits; empty numbers and dates are `null`, empty texts `''`. */
interface NoteValues {
  recipientId: number | null;
  category: string;
  takeoverPlace: string;
  ekaerNbr: string;
  comments: string;
  isUrgent: boolean;
  isCrackable: boolean;
  isDangerous: boolean;
  isOwnCost: boolean;
  contactInfo: {
    assignTo: string;
    assignToPhone: string;
    contact: string;
    contactPhone: string;
  };
  packageDetail: {
    tareWgt: number | null;
    tareWgtUm: string;
    pkgQty: number | null;
    pkgScale: string;
    pkgSizeX: number | null;
    pkgSizeY: number | null;
    pkgSizeZ: number | null;
    pkgSizeUm: string;
  };
  deadline: {
    takeoverDate: Date | null;
    shipDate: Date | null;
    estimatedArrivalDate: Date | null;
  };
  noteLines: LineValues[];
}

interface LineValues {
  partCode: string;
  partDesc: string;
  partCmt: string;
  partQty: number | null;
  partQtyUm: string;
  partWgt: number | null;
  partWgtUm: string;
}

function emptyLine(): LineValues {
  return {
    partCode: '',
    partDesc: '',
    partCmt: '',
    partQty: null,
    partQtyUm: '',
    partWgt: null,
    partWgtUm: '',
  };
}

function emptyNote(): NoteValues {
  return {
    recipientId: null,
    category: '',
    takeoverPlace: '',
    ekaerNbr: '',
    comments: '',
    isUrgent: false,
    isCrackable: false,
    isDangerous: false,
    isOwnCost: false,
    contactInfo: { assignTo: '', assignToPhone: '', contact: '', contactPhone: '' },
    packageDetail: {
      tareWgt: null,
      tareWgtUm: '',
      pkgQty: null,
      pkgScale: '',
      pkgSizeX: null,
      pkgSizeY: null,
      pkgSizeZ: null,
      pkgSizeUm: '',
    },
    deadline: { takeoverDate: null, shipDate: null, estimatedArrivalDate: null },
    noteLines: [emptyLine()],
  };
}

@Component({
  imports: [
    FormField,
    FormRoot,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    RouterLink,
  ],
  providers: [provideNativeDateAdapter()],
  selector: 'app-create-delivery-note-page',
  styleUrl: './create-delivery-note-page.scss',
  templateUrl: './create-delivery-note-page.html',
})
export class CreateDeliveryNotePage {
  private readonly notesApi = inject(DeliveryNotesApi);
  private readonly recipientsApi = inject(RecipientsApi);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);

  private readonly allRecipients = rxResource({
    stream: () => this.loadRecipients(),
  });
  protected readonly recipients = computed(() =>
    this.allRecipients.hasValue() ? this.allRecipients.value() : [],
  );
  protected readonly noRecipients = computed(
    () => this.allRecipients.hasValue() && this.allRecipients.value().length === 0,
  );
  protected readonly recipientsLoadFailed = computed(() => this.allRecipients.status() === 'error');

  private readonly note = signal(emptyNote());
  protected readonly noteForm = form(
    this.note,
    (note) => {
      required(note.recipientId, { message: 'Recipient is required.' });
      requiredText(note.category, 'Category', 20);
      requiredText(note.takeoverPlace, 'Takeover place', 30);
      optionalText(note.ekaerNbr, 'EKAER number', 10);
      optionalText(note.comments, 'Comments', 200);

      requiredText(note.contactInfo.assignTo, 'Assigned to', 200);
      requiredText(note.contactInfo.assignToPhone, 'Assignee phone', 30);
      requiredText(note.contactInfo.contact, 'Contact person', 200);
      requiredText(note.contactInfo.contactPhone, 'Contact phone', 30);

      requiredNotNegative(note.packageDetail.tareWgt, 'Tare weight');
      requiredText(note.packageDetail.tareWgtUm, 'Tare weight unit', 10);
      requiredPositive(note.packageDetail.pkgQty, 'Package quantity');
      requiredText(note.packageDetail.pkgScale, 'Package scale', 10);
      requiredNotNegative(note.packageDetail.pkgSizeX, 'Length');
      requiredNotNegative(note.packageDetail.pkgSizeY, 'Width');
      requiredNotNegative(note.packageDetail.pkgSizeZ, 'Height');
      requiredText(note.packageDetail.pkgSizeUm, 'Size unit', 10);

      requiredValue(note.deadline.takeoverDate, 'Takeover date');
      requiredValue(note.deadline.shipDate, 'Ship date');
      requiredValue(note.deadline.estimatedArrivalDate, 'Estimated arrival date');
      notEarlierThan(
        note.deadline.shipDate,
        note.deadline.takeoverDate,
        'Ship date must not be earlier than takeover date.',
      );
      notEarlierThan(
        note.deadline.estimatedArrivalDate,
        note.deadline.shipDate,
        'Estimated arrival date must not be earlier than ship date.',
      );

      applyEach(note.noteLines, (line) => {
        requiredText(line.partCode, 'Part code', 100);
        requiredText(line.partDesc, 'Description', 200);
        optionalText(line.partCmt, 'Comment', 200);
        requiredPositive(line.partQty, 'Quantity');
        requiredText(line.partQtyUm, 'Quantity unit', 10);
        requiredNotNegative(line.partWgt, 'Weight');
        requiredText(line.partWgtUm, 'Weight unit', 10);
      });
    },
    { submission: { action: () => this.createDeliveryNote() } },
  );

  protected readonly takeoverDateErrors = dateErrorMessages(this.noteForm.deadline.takeoverDate);
  protected readonly shipDateErrors = dateErrorMessages(this.noteForm.deadline.shipDate);
  protected readonly arrivalDateErrors = dateErrorMessages(
    this.noteForm.deadline.estimatedArrivalDate,
  );

  protected addLine(): void {
    this.note.update((note) => ({ ...note, noteLines: [...note.noteLines, emptyLine()] }));
  }

  protected removeLine(index: number): void {
    this.note.update((note) => ({
      ...note,
      noteLines: note.noteLines.filter((_, lineIndex) => lineIndex !== index),
    }));
  }

  /** On failure, explains the error in the dialog; the page offers no Retry. */
  private loadRecipients(): Observable<RecipientDetails[]> {
    return this.recipientsApi.getAllRecipients().pipe(
      catchError((error: HttpErrorResponse) => {
        openErrorDialog(this.dialog, error);
        return throwError(() => error);
      }),
    );
  }

  /** On failure, explains the error in the dialog and keeps the entered values for correction. */
  private async createDeliveryNote(): Promise<void> {
    const details = toNewDeliveryNote(this.note());
    if (!details) {
      return;
    }
    try {
      await this.confirmCreated(await firstValueFrom(this.notesApi.createDeliveryNote(details)));
    } catch (error) {
      if (!(error instanceof HttpErrorResponse)) {
        throw error;
      }
      openErrorDialog(this.dialog, error);
    }
  }

  private async confirmCreated(created: DeliveryNoteDetails): Promise<void> {
    this.snackBar.open(`Delivery note ${created.noteNbr} created`, undefined, { duration: 5000 });
    await this.router.navigate(['/delivery-notes']);
  }
}

function requiredText(path: SchemaPath<string>, label: string, limit: number): void {
  requiredValue(path, label);
  optionalText(path, label, limit);
}

function optionalText(path: SchemaPath<string>, label: string, limit: number): void {
  maxLength(path, limit, { message: `${label} must be at most ${limit} characters.` });
}

function requiredValue<T>(path: SchemaPath<T>, label: string): void {
  required(path, { message: `${label} is required.` });
}

function requiredPositive(path: SchemaPath<number | null>, label: string): void {
  requiredValue(path, label);
  validate(path, ({ value }) => {
    const amount = value();
    return amount !== null && amount <= 0
      ? { kind: 'positive', message: `${label} must be greater than 0.` }
      : undefined;
  });
}

function requiredNotNegative(path: SchemaPath<number | null>, label: string): void {
  requiredValue(path, label);
  min(path, 0, { message: `${label} must be 0 or more.` });
}

/** Equal dates pass; nothing is checked until both dates are set. */
function notEarlierThan(
  later: SchemaPath<Date | null>,
  earlier: SchemaPath<Date | null>,
  message: string,
): void {
  minDate(later, ({ valueOf }) => valueOf(earlier) ?? undefined, { message });
}

/**
 * The datepicker's own validators report errors without a message: its parse error, shown as
 * "Enter a valid date." instead of the "required" error the emptied model also causes, and its
 * min error, which repeats the date-order rule and is left out.
 */
function dateErrorMessages(date: FieldTree<Date | null>): Signal<string[]> {
  return computed(() => {
    const errors = date().errors();
    if (errors.some((error) => error.kind === 'matDatepickerParse')) {
      return ['Enter a valid date.'];
    }
    return errors.flatMap((error) => (error.message ? [error.message] : []));
  });
}

/**
 * The request for validated values. Returns `undefined` without a recipient, which the
 * form's required rule already prevents.
 */
function toNewDeliveryNote(values: NoteValues): NewDeliveryNote | undefined {
  if (values.recipientId === null) {
    return undefined;
  }
  return {
    ekaerNbr: emptyAsNull(values.ekaerNbr),
    takeoverPlace: values.takeoverPlace,
    category: values.category,
    comments: emptyAsNull(values.comments),
    isUrgent: values.isUrgent,
    isCrackable: values.isCrackable,
    isDangerous: values.isDangerous,
    isOwnCost: values.isOwnCost,
    recipientId: values.recipientId,
    contactInfo: values.contactInfo,
    packageDetail: values.packageDetail,
    deadline: {
      takeoverDate: calendarDate(values.deadline.takeoverDate),
      shipDate: calendarDate(values.deadline.shipDate),
      estimatedArrivalDate: calendarDate(values.deadline.estimatedArrivalDate),
    },
    noteLines: values.noteLines.map(toNewNoteLine),
  };
}

/** Copies the fields one by one: Signal Forms tags array items with an identity symbol. */
function toNewNoteLine(line: LineValues): NewNoteLine {
  return {
    partCode: line.partCode,
    partDesc: line.partDesc,
    partCmt: emptyAsNull(line.partCmt),
    partQty: line.partQty,
    partQtyUm: line.partQtyUm,
    partWgt: line.partWgt,
    partWgtUm: line.partWgtUm,
  };
}

function emptyAsNull(text: string): string | null {
  return text === '' ? null : text;
}

/** The local calendar date as `yyyy-MM-dd`, without converting it to UTC. */
function calendarDate(date: Date | null): string | null {
  if (!date) {
    return null;
  }
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}
