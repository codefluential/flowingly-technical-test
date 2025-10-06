import { test, expect } from '@playwright/test';

/**
 * E2E Tests: Validation Error Scenarios
 *
 * Tests that validation errors are properly displayed in the UI when:
 * 1. Tags are unclosed (e.g., <total>120.50)
 * 2. Expense is missing required <total> tag
 * 3. Empty text is submitted
 *
 * Verification Points:
 * - Error banner is visible
 * - Correct error code is displayed
 * - User-friendly error message is shown
 * - Correlation ID is present
 *
 * Related: ADR-0008 (Parsing & Validation Rules), ADR-0010 (Test Strategy)
 */

test.describe('Validation Error Scenarios', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('displays error when tag is unclosed', async ({ page }) => {
    // Given: Text with unclosed tag (missing closing </total>)
    const unclosedTagContent = 'Hi Yvaine, Expense for <vendor>Coffee Co</vendor> <total>120.50';

    // When: Submit text
    await page.getByRole('textbox').fill(unclosedTagContent);
    await page.getByRole('button', { name: /parse/i }).click();

    // Then: Error banner should appear with UNCLOSED_TAGS error
    const errorBanner = page.getByTestId('error-banner');
    await expect(errorBanner).toBeVisible();

    // Verify error code is displayed
    const errorCode = errorBanner.locator('.error-code');
    await expect(errorCode).toContainText('UNCLOSED_TAGS');

    // Verify user-friendly error message mentions "unclosed" or "tags"
    const errorMessage = errorBanner.locator('.error-message');
    await expect(errorMessage).toBeVisible();
    const messageText = await errorMessage.textContent();
    expect(messageText?.toLowerCase()).toMatch(/unclosed|tags/);

    // Verify correlation ID is present
    const correlationId = errorBanner.locator('.correlation-id');
    await expect(correlationId).toBeVisible();
    await expect(correlationId).toContainText('Support Reference:');
  });

  test('displays error when total tag is missing', async ({ page }) => {
    // Given: Expense content without required <total> tag
    const missingTotalContent = 'Hi team, Expense for <vendor>Office Supplies Ltd</vendor> <cost_centre>DEV002</cost_centre>';

    // When: Submit text
    await page.getByRole('textbox').fill(missingTotalContent);
    await page.getByRole('button', { name: /parse/i }).click();

    // Then: Error banner should appear with MISSING_TOTAL error
    const errorBanner = page.getByTestId('error-banner');
    await expect(errorBanner).toBeVisible();

    // Verify error code is displayed
    const errorCode = errorBanner.locator('.error-code');
    await expect(errorCode).toContainText('MISSING_TOTAL');

    // Verify user-friendly error message mentions "total" or "required"
    const errorMessage = errorBanner.locator('.error-message');
    await expect(errorMessage).toBeVisible();
    const messageText = await errorMessage.textContent();
    expect(messageText?.toLowerCase()).toMatch(/total|required/);

    // Verify correlation ID is present
    const correlationId = errorBanner.locator('.correlation-id');
    await expect(correlationId).toBeVisible();
    await expect(correlationId).toContainText('Support Reference:');
  });

  test('displays error when empty text is submitted', async ({ page }) => {
    // Given: Empty textarea
    const emptyContent = '';

    // When: Try to submit empty text
    await page.getByRole('textbox').fill(emptyContent);

    // Then: Parse button should be disabled (client-side validation)
    const parseButton = page.getByRole('button', { name: /parse/i });
    await expect(parseButton).toBeDisabled();
  });

  test('displays error when only whitespace is submitted', async ({ page }) => {
    // Given: Only whitespace
    const whitespaceContent = '   \n   \t   ';

    // When: Try to submit whitespace text
    await page.getByRole('textbox').fill(whitespaceContent);

    // Then: Parse button should be disabled (client-side validation)
    const parseButton = page.getByRole('button', { name: /parse/i });
    await expect(parseButton).toBeDisabled();
  });

  test('displays error when tags overlap', async ({ page }) => {
    // Given: Text with overlapping tags (closing order violation)
    const overlappingContent = 'Please process <expense><vendor>Mojo Coffee</vendor><total>150.00</expense></total>';

    // When: Submit text
    await page.getByRole('textbox').fill(overlappingContent);
    await page.getByRole('button', { name: /parse/i }).click();

    // Then: Error banner should appear with UNCLOSED_TAGS error
    const errorBanner = page.getByTestId('error-banner');
    await expect(errorBanner).toBeVisible();

    // Verify error code is displayed (overlapping tags are caught as UNCLOSED_TAGS)
    const errorCode = errorBanner.locator('.error-code');
    await expect(errorCode).toContainText('UNCLOSED_TAGS');

    // Verify user-friendly error message is present
    const errorMessage = errorBanner.locator('.error-message');
    await expect(errorMessage).toBeVisible();

    // Verify correlation ID is present
    const correlationId = errorBanner.locator('.correlation-id');
    await expect(correlationId).toBeVisible();
  });
});
