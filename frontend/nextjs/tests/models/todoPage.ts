// frontend/nextjs/e2e/models/todoPage.ts
import { Page, Locator, expect } from '@playwright/test';

export class TodoPage {
  readonly page: Page;
  readonly todoListTitle: Locator;
  readonly addTodoButton: Locator;
  readonly todoItems: Locator;
  
  constructor(page: Page) {
    this.page = page;
    this.todoListTitle = page.getByText('TODO List');
    this.addTodoButton = page.getByRole('button', { name: 'Add TODO' });
    this.todoItems = page.locator('li');
  }
  
  async goto() {
    await this.page.goto('/todos');
  }
  
  async expectLoaded() {
    await expect(this.todoListTitle).toBeVisible();
    await expect(this.addTodoButton).toBeVisible();
  }
  
  async getTodoItem(title: string) {
    return this.page.getByText(title).first();
  }
  
  async clickEditTodo(title: string) {
    const todoRow = await this.getTodoItem(title).locator('..').locator('..');
    await todoRow.getByRole('button', { name: 'Edit' }).click();
  }
  
  async clickDeleteTodo(title: string) {
    const todoRow = await this.getTodoItem(title).locator('..').locator('..');
    await todoRow.getByRole('button', { name: 'Delete' }).click();
  }
  
  async expectTodoExists(title: string) {
    await expect(this.getTodoItem(title)).toBeVisible();
  }
  
  async expectTodoNotExists(title: string) {
    await expect(this.getTodoItem(title)).toHaveCount(0);
  }
}