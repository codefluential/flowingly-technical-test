import { test, expect } from '@playwright/test';

/**
 * Smoke Tests - Playwright Setup Verification
 *
 * Purpose: Validate that Playwright infrastructure is working correctly before
 * writing comprehensive E2E tests. These tests verify basic app functionality:
 * - App loads successfully
 * - Core UI elements are visible
 * - Basic interaction is possible
 *
 * These tests serve as a foundation for task_045, task_046, and task_047 which
 * will implement comprehensive workflow testing using sample email fixtures.
 */
test.describe('Smoke Tests', () => {
  test('app loads successfully', async ({ page }) => {
    // Navigate to app
    await page.goto('/');

    // Verify page has loaded by checking for the main heading
    // (Adjust selector based on actual app structure)
    const heading = page.getByRole('heading', { level: 1 });
    await expect(heading).toBeVisible();
  });

  test('parse button is visible', async ({ page }) => {
    await page.goto('/');

    // Verify parse button exists and is visible
    const parseButton = page.getByRole('button', { name: /parse/i });
    await expect(parseButton).toBeVisible();
  });

  test('textarea is present for input', async ({ page }) => {
    await page.goto('/');

    // Verify input textarea is available
    const textarea = page.getByRole('textbox');
    await expect(textarea).toBeVisible();
  });
});
