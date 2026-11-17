import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatTableModule } from '@angular/material/table';
import { RecipientsApi } from '../recipients-api';

@Component({
  imports: [MatTableModule],
  selector: 'app-recipients-page',
  templateUrl: './recipients-page.html',
})
export class RecipientsPage {
  private readonly api = inject(RecipientsApi);

  private readonly page = signal(1);
  private readonly recipientsPage = rxResource({
    params: () => this.page(),
    stream: ({ params: page }) => this.api.getRecipients(page),
  });

  protected readonly recipients = computed(() => this.recipientsPage.value()?.content ?? []);
  protected readonly noRecipients = computed(
    () => this.recipientsPage.value()?.content.length === 0,
  );
  protected readonly columns = ['code', 'name', 'country', 'zip', 'city', 'address', 'isForeign'];
}
