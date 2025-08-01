// frontend/nextjs/e2e/todo-workflow.spec.ts
import { test, expect } from '@playwright/test';
import { TodoPage } from './models/todoPage';
import { TodoDetailPage } from './models/todoDetailPage';
import { TodoAddPage } from './models/todoAddPage';

test.describe('Todo Application E2E', () => {
  test.describe.configure({ mode: 'serial' });

  let todoPage: TodoPage;
  let todoDetailPage: TodoDetailPage;
  let todoAddPage: TodoAddPage;

  // Generate a unique todo title for test isolation
  const uniqueTodoTitle = `E2E Test Todo ${Date.now()}`;
  let createdTodoId: string;

  test.beforeEach(async ({ page }) => {
    todoPage = new TodoPage(page);
    todoDetailPage = new TodoDetailPage(page);
    todoAddPage = new TodoAddPage(page);
  });

  test('should display the todo list page', async () => {
    // Arrange
    await todoPage.goto();

    // Act - page load

    // Assert
    await todoPage.expectLoaded();
  });

  test('should allow adding a new todo', async ({ page }) => {
    // Arrange
    await todoPage.goto();
    await todoPage.addTodoButton.click();

    // Act
    await todoAddPage.expectLoaded();
    await todoAddPage.addTodo(uniqueTodoTitle);

    // Assert
    await todoDetailPage.expectLoaded();
    await todoDetailPage.expectTitle(uniqueTodoTitle);

    // Store the ID for future tests
    const idText = await todoDetailPage.idField.textContent();
    createdTodoId = idText?.split('ID: ')[1].trim() || '';
    expect(createdTodoId).toBeTruthy();
  });

  test('should navigate back to list and show the new todo', async () => {
    // Arrange
    await todoDetailPage.goto(createdTodoId);

    // Act
    await todoDetailPage.clickBackToList();

    // Assert
    await todoPage.expectLoaded();
    await todoPage.expectTodoExists(uniqueTodoTitle);
  });

  test('should edit a todo', async () => {
    // Arrange
    await todoPage.goto();
    await todoPage.clickEditTodo(uniqueTodoTitle);

    // Act
    const updatedTitle = `${uniqueTodoTitle} - Updated`;
    await todoDetailPage.updateTitle(updatedTitle);

    // Assert
    await todoDetailPage.expectLoaded();
    await todoDetailPage.expectTitle(updatedTitle);

    // Return to list and verify update is visible
    await todoDetailPage.clickBackToList();
    await todoPage.expectTodoExists(updatedTitle);
  });

  test('should delete a todo', async ({ page }) => {
    // Arrange
    await todoPage.goto();
    const updatedTitle = `${uniqueTodoTitle} - Updated`;

    // Act
    await todoPage.clickDeleteTodo(updatedTitle);

    // Assert - wait for deletion to complete
    await page.waitForTimeout(500); // Short wait for UI update
    await todoPage.expectTodoNotExists(updatedTitle);
  });
});