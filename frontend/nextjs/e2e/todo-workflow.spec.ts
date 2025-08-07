import { test, expect } from '@playwright/test';
const TEST_USER_EMAIL = 'test1@test.com';
const TEST_USER_PASSWORD = 'Test01@test';

test.describe('Todo Application Workflow', () => {
  // Login before each test
  test.beforeEach(async ({ page }) => {
    // Navigate to the login page
    await page.goto('/login');

    // Check that we're on the login page
    await expect(page.getByText('Sign in to your account')).toBeVisible();

    // Fill in login credentials and submit
    await page.getByRole('textbox', { name: 'Email address' }).fill(TEST_USER_EMAIL);
    await page.getByRole('textbox', { name: 'Password' }).fill(TEST_USER_PASSWORD);
    await page.getByRole('button', { name: 'Sign in' }).click();

    // Wait for navigation to dashboard after login
    await page.waitForURL('/dashboard');
  });

  test('should allow user to create, view, edit, and delete todos', async ({ page }) => {
    // Verify we're on the dashboard
    await expect(page.getByText('TODO List')).toBeVisible();

    // Create a new todo
    expect(page.getByRole('button', { name: 'Add TODO' })).toBeVisible();
    await page.getByRole('button', { name: 'Add TODO' }).click();

    // Should be redirected to the edit form for a new todo
    await page.waitForURL('/dashboard/todos/*');

    // await expect(page.url()).toContain('/dashboard/todos/new?edit=1');

    // Fill in the todo details
    await expect(page.getByText('Add TODO')).toBeVisible();
    await page.getByPlaceholder('Enter todo title').fill('Buy groceries');
    await page.getByRole('button', { name: 'Add' }).click();

    // Wait for navigation back to the todos list
    await page.waitForURL('/dashboard/todos/*');

    // Navigate back to the todo list
    await expect(page.getByRole('button', { name: 'Back to List' })).toBeVisible();
    await page.getByRole('button', { name: 'Back to List' }).click();

    await page.waitForURL('/dashboard/todos/*');

    // Verify the todo was added to the list
    const todoItem = page.getByText('Buy groceries');
    await expect(todoItem).toBeVisible();

    // Create another todo
    await expect(page.getByText('Add TODO')).toBeVisible();
    await page.getByRole('button', { name: 'Add TODO' }).click();
    await page.getByPlaceholder('Enter todo title').fill('Complete homework');
    await page.getByRole('button', { name: 'Add' }).click();
    await page.waitForURL('/dashboard/todos/*');
    await expect(page.getByRole('button', { name: 'Back to List' })).toBeVisible();
    await page.getByRole('button', { name: 'Back to List' }).click();

    // Verify both todos exist
    await expect(page.getByRole('heading', { name: 'TODO List' })).toBeVisible();
    await expect(page.getByText('Buy groceries')).toBeVisible();
    await expect(page.getByText('Complete homework')).toBeVisible();

    // Edit a todo
    await page.getByRole('listitem')
      .filter({ hasText: 'Buy groceries' })
      .getByRole('button').first().click();

    // Update the todo
    await page.getByRole('textbox').clear();
    await page.getByRole('textbox').fill('Buy organic groceries');
    await page.getByRole('button', { name: 'Save' }).click();
    await page.waitForURL('/dashboard/todos/*');
    await expect(page.getByRole('button', { name: 'Back to List' })).toBeVisible();
    await page.getByRole('button', { name: 'Back to List' }).click();

    // Verify the todo was updated
    await expect(page.getByText('Buy organic groceries')).toBeVisible();

    // Delete a todo
    await page.getByRole('listitem')
      .filter({ hasText: 'Complete homework' })
      .getByRole('button', { name: 'Delete' })
      .click();

    // Verify the todo was deleted
    await expect(page.getByText('Complete homework')).not.toBeVisible();
    await expect(page.getByText('Buy organic groceries')).toBeVisible();
  });

  test('should validate empty inputs', async ({ page }) => {
    // Create a new todo
    await expect(page.getByText('Add TODO')).toBeVisible();
    await page.getByRole('button', { name: 'Add TODO' }).click();

    await page.waitForURL('/dashboard/todos/*');

    // Try to save with empty title
    await page.getByRole('button', { name: 'Add' }).click();

    // Should still be on the edit page
    await expect(page.url()).toContain('edit=1');

    // Fill in the title and save
    await expect(page.getByText('Add TODO')).toBeVisible();
    await page.getByPlaceholder('Enter TODO title').fill('Valid todo');
    await page.getByRole('button', { name: 'Add' }).click();

    // Should navigate away after saving
    await page.waitForURL('/dashboard/todos/*');

    await expect(page.getByRole('button', { name: 'Back to List' })).toBeVisible();
    await page.getByRole('button', { name: 'Back to List' }).click();

    await page.waitForURL('/dashboard/todos/*');

    // Verify the todo was added to the list
    const todoItem = page.getByText('Valid todo');
    await expect(todoItem).toBeVisible();
  });

  test('should allow user to sign out', async ({ page }) => {
    // Sign out
    await page.getByRole('button', { name: /logout/i }).click();

    // Verify redirect to login page
    await page.waitForURL('/login');
    await expect(page.getByText('Sign in to your account')).toBeVisible();

    // Verify we can't access dashboard after logout
    await page.goto('/dashboard/todos');
    await expect(page.url()).toContain('/login');
  });
});
