// frontend/nextjs/e2e/models/todoAddPage.ts
import { Page, Locator, expect } from '@playwright/test';

export class TodoAddPage {
  readonly page: Page;
  readonly addTitle: Locator;
  readonly titleInput: Locator;
  readonly addButton: Locator;
  readonly backButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.addTitle = page.getByText('Add TODO');
    this.titleInput = page.getByPlaceholder('Enter TODO title');
    this.addButton = page.getByRole('button', { name: 'Add' });
    this.backButton = page.getByRole('button', { name: 'Back to List' });
  }

  async goto() {
    await this.page.goto('/dashboard/todos/new?edit=1');
  }

  async expectLoaded() {
    await expect(this.addTitle).toBeVisible();
    await expect(this.titleInput).toBeVisible();
    await expect(this.addButton).toBeVisible();
  }

  async addTodo(title: string) {
    await this.titleInput.fill(title);
    await this.addButton.click();
  }

  async clickBackToList() {
    await this.backButton.click();
  }
}