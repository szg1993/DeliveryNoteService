import { screen, within } from '@testing-library/angular';

/** The visible text of every table row, header row first, one entry per cell. */
export function tableRowTexts(): string[][] {
  return screen
    .queryAllByRole('row')
    .map((row) =>
      [...within(row).queryAllByRole('columnheader'), ...within(row).queryAllByRole('cell')].map(
        (cell) => cell.textContent?.trim() ?? '',
      ),
    );
}
