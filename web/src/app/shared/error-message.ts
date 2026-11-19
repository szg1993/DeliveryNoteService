import { HttpErrorResponse } from '@angular/common/http';

/** The backend's error body. Field errors name the field in `data.Field`. */
interface BackendError {
  errorCode?: string;
  data?: { Field?: string };
}

const fallbackMessage = 'Something went wrong. Please try again.';

/** The backend error code, or `undefined` when the response has none, such as a network failure. */
export function backendErrorCode(error: HttpErrorResponse): string | undefined {
  return backendError(error)?.errorCode;
}

/**
 * A readable English explanation of a failed request. `recipientCode` is the submitted code,
 * quoted by the duplicate-code message; without it that error gets the fallback message.
 */
export function readableErrorMessage(error: HttpErrorResponse, recipientCode?: string): string {
  const body = backendError(error);
  switch (body?.errorCode) {
    case 'Recipient.AlreadyExists':
      return recipientCode
        ? `A recipient with code ${recipientCode} already exists.`
        : fallbackMessage;
    case 'error:DNC::FieldMissing':
    case 'error:DNC::FieldTooLong':
      return body.data?.Field ? invalidFieldMessage(body.data.Field) : fallbackMessage;
    case 'Recipient.InvalidCode':
      return invalidFieldMessage('Code');
    case 'Recipient.InvalidCountry':
      return 'Unknown country code.';
    case 'Recipient.NotFound':
      return 'The selected recipient no longer exists.';
    case 'Note.InvalidInput':
      return 'Some delivery note values are invalid. Check quantities, weights, dimensions and date order.';
    case 'Note.NumberExhausted':
      return 'No more delivery note numbers are available.';
    default:
      return fallbackMessage;
  }
}

function invalidFieldMessage(field: string): string {
  return `The ${field} field is invalid.`;
}

/** The parsed error body, if the backend sent one; network failures carry a `ProgressEvent`. */
function backendError(error: HttpErrorResponse): BackendError | undefined {
  return typeof error.error === 'object' && error.error !== null
    ? (error.error as BackendError)
    : undefined;
}
