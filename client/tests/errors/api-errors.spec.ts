import { test, expect } from '@playwright/test';

/**
 * E2E Tests: API Error Display and Interaction
 *
 * Tests that API errors are properly displayed and interactive in the UI:
 * 1. Error code is displayed prominently
 * 2. Correlation ID is shown for support reference
 * 3. Error banner is dismissible via close button
 * 4. Error banner is dismissible via ESC key
 * 5. Error clears on new valid submission
 *
 * Verification Points:
 * - Error banner UI components render correctly
 * - User can dismiss errors via multiple methods
 * - Error state clears appropriately
 *
 * Related: ADR-0007 (Response Contract), ADR-0010 (Test Strategy)
 */

test.describe('API Error Display and Interaction', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('displays error code in error banner', async ({ page }) => {
    // Given: Text that will trigger UNCLOSED_TAGS error
    const unclosedTagContent = '<total>120.50';

    // When: Submit text
    await page.getByRole('textbox').fill(unclosedTagContent);
    await page.getByRole('button', { name: /parse/i }).click();

    // Then: Error code should be visible in error banner
    const errorBanner = page.getByTestId('error-banner');
    await expect(errorBanner).toBeVisible();

    const errorCode = errorBanner.locator('.error-code');
    await expect(errorCode).toBeVisible();
    await expect(errorCode).toContainText('Error: UNCLOSED_TAGS');
  });

  test('displays correlation ID in error banner', async ({ page }) => {
    // Given: Text that will trigger an error
    const invalidContent = '<vendor>Coffee Shop</vendor>';

    // When: Submit text
    await page.getByRole('textbox').fill(invalidContent);
    await page.getByRole('button', { name: /parse/i }).click();

    // Then: Correlation ID should be visible
    const errorBanner = page.getByTestId('error-banner');
    await expect(errorBanner).toBeVisible();

    const correlationId = errorBanner.locator('.correlation-id');
    await expect(correlationId).toBeVisible();
    await expect(correlationId).toContainText('Support Reference:');

    // Verify correlation ID value is present (not just the label)
    const correlationText = await correlationId.textContent();
    expect(correlationText).toMatch(/Support Reference:\s+[A-Za-z0-9-]+/);
  });

  test('dismisses error banner via close button', async ({ page }) => {
    // Given: Error is displayed
    const unclosedTagContent = '<total>120.50';
    await page.getByRole('textbox').fill(unclosedTagContent);
    await page.getByRole('button', { name: /parse/i }).click();

    const errorBanner = page.getByTestId('error-banner');
    await expect(errorBanner).toBeVisible();

    // When: Click dismiss button
    const dismissButton = page.getByTestId('dismiss-error-button');
    await expect(dismissButton).toBeVisible();
    await dismissButton.click();

    // Then: Error banner should be hidden
    await expect(errorBanner).not.toBeVisible();
  });

  test('dismisses error banner via ESC key', async ({ page }) => {
    // Given: Error is displayed
    const missingTotalContent = '<vendor>Starbucks</vendor>';
    await page.getByRole('textbox').fill(missingTotalContent);
    await page.getByRole('button', { name: /parse/i }).click();

    const errorBanner = page.getByTestId('error-banner');
    await expect(errorBanner).toBeVisible();

    // When: Press ESC key
    await page.keyboard.press('Escape');

    // Then: Error banner should be hidden
    await expect(errorBanner).not.toBeVisible();
  });

  test('error clears on new valid submission', async ({ page }) => {
    // Given: Error is displayed from invalid submission
    const unclosedTagContent = '<total>120.50';
    await page.getByRole('textbox').fill(unclosedTagContent);
    await page.getByRole('button', { name: /parse/i }).click();

    const errorBanner = page.getByTestId('error-banner');
    await expect(errorBanner).toBeVisible();

    // When: Submit valid content
    const validContent = 'Hi Yvaine, Please create an expense claim for the below. Relevant details are:\n<expense><cost_centre>DEV002</cost_centre><total>1024.01</total><payment_method>personal card</payment_method></expense>';
    await page.getByRole('textbox').fill(validContent);
    await page.getByRole('button', { name: /parse/i }).click();

    // Then: Wait for result display to appear (this indicates success)
    const resultDisplay = page.getByTestId('result-display');
    await expect(resultDisplay).toBeVisible();

    // Error banner should be hidden after successful submission
    await expect(errorBanner).not.toBeVisible();
  });

  test('displays user-friendly error message', async ({ page }) => {
    // Given: Text that will trigger MISSING_TOTAL error
    const missingTotalContent = '<vendor>Coffee Shop</vendor> <cost_centre>DEV002</cost_centre>';

    // When: Submit text
    await page.getByRole('textbox').fill(missingTotalContent);
    await page.getByRole('button', { name: /parse/i }).click();

    // Then: Error message should be user-friendly (not technical)
    const errorBanner = page.getByTestId('error-banner');
    await expect(errorBanner).toBeVisible();

    const errorMessage = errorBanner.locator('.error-message');
    await expect(errorMessage).toBeVisible();

    // Verify message is actionable and mentions the fix
    const messageText = await errorMessage.textContent();
    expect(messageText?.toLowerCase()).toMatch(/total|missing|required/);
    expect(messageText?.toLowerCase()).toMatch(/include|please|add/);
  });

  test('error banner has proper ARIA attributes for accessibility', async ({ page }) => {
    // Given: Text that will trigger an error
    const unclosedTagContent = '<total>120.50';

    // When: Submit text
    await page.getByRole('textbox').fill(unclosedTagContent);
    await page.getByRole('button', { name: /parse/i }).click();

    // Then: Error banner should have proper accessibility attributes
    const errorBanner = page.getByTestId('error-banner');
    await expect(errorBanner).toBeVisible();

    // Verify ARIA role and live region attributes
    await expect(errorBanner).toHaveAttribute('role', 'alert');
    await expect(errorBanner).toHaveAttribute('aria-live', 'polite');
    await expect(errorBanner).toHaveAttribute('aria-atomic', 'true');
  });

  test('multiple errors are handled sequentially', async ({ page }) => {
    // Given: First error is displayed
    const unclosedTagContent = '<total>120.50';
    await page.getByRole('textbox').fill(unclosedTagContent);
    await page.getByRole('button', { name: /parse/i }).click();

    const errorBanner = page.getByTestId('error-banner');
    await expect(errorBanner).toBeVisible();

    // Verify first error code
    let errorCode = errorBanner.locator('.error-code');
    await expect(errorCode).toContainText('UNCLOSED_TAGS');

    // When: Dismiss first error and trigger a different error
    await page.keyboard.press('Escape');
    await expect(errorBanner).not.toBeVisible();

    const missingTotalContent = '<vendor>Coffee Shop</vendor>';
    await page.getByRole('textbox').fill(missingTotalContent);
    await page.getByRole('button', { name: /parse/i }).click();

    // Then: New error should be displayed with different error code
    await expect(errorBanner).toBeVisible();
    errorCode = errorBanner.locator('.error-code');
    await expect(errorCode).toContainText('MISSING_TOTAL');
  });
});
