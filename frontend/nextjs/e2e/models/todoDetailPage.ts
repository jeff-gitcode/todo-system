import { Page, Locator, expect } from '@playwright/test';

export class TodoDetailPage {
  readonly page: Page;
  readonly detailTitle: Locator;
  readonly idField: Locator;
  readonly titleField: Locator;
  readonly editButton: Locator;
  readonly backButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.detailTitle = page.getByText('TODO Detail');
    this.idField = page.getByText(/ID:\s*[a-zA-Z0-9-]{4,30}/i);
    this.titleField = page.getByText('Title:').locator('..').locator('strong');
    this.editButton = page.getByRole('button', { name: 'Edit' });
    this.backButton = page.getByRole('button', { name: 'Back to List' });
  }

  async goto(id: string) {
    await this.page.goto(`/dashboard/todos/${id}`);
  }

  async expectLoaded() {
    await expect(this.detailTitle).toBeVisible();
    await expect(this.backButton).toBeVisible();
  }

  async clickEdit() {
    await this.editButton.click();
  }

  async updateTitle(newTitle: string) {
    const input = this.page.getByRole('textbox');
    await input.clear();
    await input.fill(newTitle);
    await this.page.getByRole('button', { name: 'Save' }).click();
  }

  async expectTitle(title: string) {
    await expect(this.page.getByText(`Title: ${title}`)).toBeVisible();
  }

  async clickBackToList() {
    await this.backButton.click();
  }
}