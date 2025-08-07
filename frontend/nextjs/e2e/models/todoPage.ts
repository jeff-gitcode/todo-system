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
    await this.page.goto('/dashboard/todos');
  }

  async expectLoaded() {
    await expect(this.todoListTitle).toBeVisible();
    await expect(this.addTodoButton).toBeVisible();
  }

  async getTodoItem(title: string) {
    return this.page.getByText(title).first();
  }

  async getListItem(title: string) {
    return this.page.getByRole('listitem');
  }

  async clickEditTodo(title: string) {
    const listItem = await this.getListItem(title);
    const todoRow = listItem.filter({ hasText: title });
    await todoRow.getByRole('button').first().click();
  }

  async clickDeleteTodo(title: string) {
    const listItem = await this.getListItem(title);
    const todoRow = listItem.filter({ hasText: title });
    await todoRow.getByRole('button').nth(1).click();
  }

  async expectTodoExists(title: string) {
    const todoItem = await this.getTodoItem(title);
    await expect(todoItem).toBeVisible();
  }

  async expectTodoNotExists(title: string) {
    const todoItem = await this.getTodoItem(title);
    await expect(await todoItem.isVisible()).toBeFalsy();
  }
}