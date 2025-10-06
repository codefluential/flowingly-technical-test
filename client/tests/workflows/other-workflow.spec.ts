import { test, expect } from '@playwright/test';
import { readFileSync } from 'fs';
import { resolve, dirname } from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

/**
 * E2E Happy Path Tests - Other/Unprocessed Workflow
 *
 * Tests the complete user workflow for non-expense (other) classification:
 * 1. User submits non-expense content (e.g., reservation request) via UI
 * 2. Backend parses and classifies as 'other'
 * 3. UI displays raw tags and classification
 * 4. Correlation ID present for traceability
 *
 * Uses sample email fixtures from test brief (task_044).
 */

test.describe('Other/Unprocessed Workflow - Happy Path E2E', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the application
    await page.goto('/');

    // Verify page loaded correctly
    await expect(page.locator('h1')).toContainText('Flowingly Parsing Service');
  });

  test('submits reservation request (sample-email-2) and verifies classified as expense', async ({ page }) => {
    // GIVEN: Sample email 2 content loaded (reservation with vendor+total tags)
    // NOTE: The content router classifies this as EXPENSE because it has vendor+total tags
    // This demonstrates that any content with expense-like tags gets processed as expense
    const sampleEmail2Path = resolve(__dirname, '../fixtures/sample-email-2-other.txt');
    const sampleEmail2 = readFileSync(sampleEmail2Path, 'utf-8');

    // WHEN: User submits content with vendor+total tags
    await page.fill('[data-testid="content-input"]', sampleEmail2);
    await page.click('[data-testid="submit-button"]');

    // Wait for response to load
    await page.waitForSelector('[data-testid="result-display"]', { timeout: 10000 });

    // THEN: Response classification is 'expense' (not 'other')
    // Because vendor+total tags trigger expense processing
    const classificationBadge = page.locator('[data-testid="classification-badge"]');
    await expect(classificationBadge).toHaveText('Expense');

    // AND: Expense data is displayed
    await expect(page.locator('[data-testid="vendor"]')).toHaveText('Viaduct Steakhouse');
    await expect(page.locator('[data-testid="total-incl-tax"]')).toContainText('5.00');

    // AND: Correlation ID is present
    const correlationId = await page.locator('[data-testid="correlation-id"]').textContent();
    expect(correlationId).toBeTruthy();
    expect(correlationId).toMatch(/^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$/i);
  });

  test('submits generic non-expense content and verifies other classification', async ({ page }) => {
    // GIVEN: Generic non-expense content WITHOUT expense tags (no vendor/total/payment_method)
    const nonExpenseContent = `Hi Yvaine,
Can you please book a meeting room?
<location>Conference Center</location>
<date>2025-10-15</date>
<attendees>10 people</attendees>`;

    // WHEN: User submits content
    await page.fill('[data-testid="content-input"]', nonExpenseContent);
    await page.click('[data-testid="submit-button"]');

    // Wait for response
    await page.waitForSelector('[data-testid="result-display"]', { timeout: 10000 });

    // THEN: Classification is 'other'
    await expect(page.locator('[data-testid="classification-badge"]')).toHaveText('Other');

    // AND: Raw tags displayed
    const rawTags = page.locator('[data-testid="raw-tags"]');
    await expect(rawTags).toBeVisible();
    await expect(rawTags).toContainText('location');
    await expect(rawTags).toContainText('Conference Center');
    await expect(rawTags).toContainText('date');
    await expect(rawTags).toContainText('2025-10-15');
    await expect(rawTags).toContainText('attendees');
    await expect(rawTags).toContainText('10 people');

    // AND: Correlation ID present
    const correlationId = await page.locator('[data-testid="correlation-id"]').textContent();
    expect(correlationId).toBeTruthy();
    expect(correlationId).toMatch(/^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$/i);

    // AND: Expense result NOT displayed (XOR)
    await expect(page.locator('[data-testid="expense-result"]')).not.toBeVisible();
  });

  test('verifies other content note is displayed', async ({ page }) => {
    // GIVEN: Non-expense content (no expense-related tags)
    const content = `<location>Restaurant ABC</location> <capacity>8</capacity> people dinner reservation`;

    // WHEN: User submits content
    await page.fill('[data-testid="content-input"]', content);
    await page.click('[data-testid="submit-button"]');

    // Wait for response
    await page.waitForSelector('[data-testid="result-display"]', { timeout: 10000 });

    // THEN: Other view displays informative note
    const otherView = page.locator('[data-testid="other-result"]');
    await expect(otherView).toContainText('This content was classified as non-expense and stored for future processing');

    // AND: Classification badge is 'Other'
    await expect(page.locator('[data-testid="classification-badge"]')).toHaveText('Other');
  });

  test('verifies raw tags are preserved correctly in other response', async ({ page }) => {
    // GIVEN: Content with multiple non-expense tags
    const multiTagContent = `
      Reservation request:
      <location>The Grand Hotel</location>
      <date>2025-11-01</date>
      <time>7:00 PM</time>
      <guests>15</guests>
      <contact>john@example.com</contact>
    `;

    // WHEN: User submits content
    await page.fill('[data-testid="content-input"]', multiTagContent);
    await page.click('[data-testid="submit-button"]');

    // Wait for response
    await page.waitForSelector('[data-testid="result-display"]', { timeout: 10000 });

    // THEN: All tags are preserved in raw form
    const rawTags = page.locator('[data-testid="raw-tags"]');
    await expect(rawTags).toBeVisible();

    // Verify each tag key-value pair
    await expect(rawTags).toContainText('location');
    await expect(rawTags).toContainText('The Grand Hotel');
    await expect(rawTags).toContainText('date');
    await expect(rawTags).toContainText('2025-11-01');
    await expect(rawTags).toContainText('time');
    await expect(rawTags).toContainText('7:00 PM');
    await expect(rawTags).toContainText('guests');
    await expect(rawTags).toContainText('15');

    // AND: Classification is 'other'
    await expect(page.locator('[data-testid="classification-badge"]')).toHaveText('Other');

    // AND: Correlation ID present
    const correlationId = await page.locator('[data-testid="correlation-id"]').textContent();
    expect(correlationId).toBeTruthy();
  });
});
