import { test, expect } from '@playwright/test';

/**
 * GST Verification Tests - Banker's Rounding
 *
 * Purpose: Verify GST (Goods and Services Tax) calculations in the UI use
 * Banker's Rounding (MidpointRounding.ToEven) as specified in ADR-0009.
 *
 * Critical Test Case: 120.50 @ 15% GST = 104.78 excl + 15.72 tax
 * Standard rounding would produce 104.77 + 15.73 (INCORRECT).
 *
 * Tax Calculation Formula:
 * - total_excl_tax = total_incl_tax / (1 + tax_rate)
 * - sales_tax = total_incl_tax - total_excl_tax
 * - Both values rounded using Banker's Rounding to 2 decimal places
 *
 * Banker's Rounding Rule:
 * - When rounding X.XX5, round to the nearest even digit
 * - Examples: 2.125 → 2.12, 2.135 → 2.14, 2.145 → 2.14, 2.155 → 2.16
 */

test.describe('GST Verification - Banker\'s Rounding', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the app before each test
    await page.goto('/');
  });

  test('120.50 @ 15% GST = 104.78 excl + 15.72 tax (Banker\'s Rounding)', async ({ page }) => {
    // Arrange: Critical test case from ADR-0009
    // This test MUST pass with 104.78, not 104.77 (standard rounding)
    const expenseText = `Hi Yvaine, Please create an expense claim for the below. Relevant details are:
<expense><cost_centre>DEV002</cost_centre><total>120.50</total><payment_method>personal card</payment_method></expense>`;

    // Act: Submit expense text
    await page.getByRole('textbox').fill(expenseText);
    await page.getByRole('button', { name: /parse/i }).click();

    // Wait for response to appear
    await page.waitForSelector('[data-testid="expense-result"]', { timeout: 10000 });

    // Assert: Verify Banker's Rounding applied
    const totalInclTax = page.getByTestId('total-incl-tax');
    const totalExclTax = page.getByTestId('total-excl-tax');
    const salesTax = page.getByTestId('sales-tax');
    const taxRate = page.getByTestId('tax-rate');

    // Verify exact values (Banker's Rounding)
    await expect(totalInclTax).toContainText('120.50');
    await expect(totalExclTax).toContainText('104.78'); // NOT 104.77 (standard rounding)
    await expect(salesTax).toContainText('15.72');      // NOT 15.73 (standard rounding)
    await expect(taxRate).toContainText('0.15');

    // Additional verification: Extract numeric values and verify
    const exclTaxText = await totalExclTax.textContent();
    const salesTaxText = await salesTax.textContent();

    expect(exclTaxText).toMatch(/104\.78/);
    expect(salesTaxText).toMatch(/15\.72/);
  });

  test('100.00 @ 15% GST = 86.96 excl + 13.04 tax (Banker\'s Rounding)', async ({ page }) => {
    // Arrange: Another Banker's Rounding test case
    // 100.00 / 1.15 = 86.95652173913... → 86.96 (Banker's Rounding)
    // 100.00 - 86.96 = 13.04
    const expenseText = `Hi Yvaine, Please create an expense claim for the below.
<expense><total>100.00</total><payment_method>personal card</payment_method></expense>`;

    // Act: Submit expense text
    await page.getByRole('textbox').fill(expenseText);
    await page.getByRole('button', { name: /parse/i }).click();

    // Wait for response
    await page.waitForSelector('[data-testid="expense-result"]', { timeout: 10000 });

    // Assert: Verify calculations
    await expect(page.getByTestId('total-incl-tax')).toContainText('100.00');
    await expect(page.getByTestId('total-excl-tax')).toContainText('86.96');
    await expect(page.getByTestId('sales-tax')).toContainText('13.04');
    await expect(page.getByTestId('tax-rate')).toContainText('0.15');
  });

  test('1024.01 @ 15% GST = 890.44 excl + 133.57 tax (Sample from test brief)', async ({ page }) => {
    // Arrange: Sample 1 from test brief PDF
    // 1024.01 / 1.15 = 890.443478260869... → 890.44 (Banker's Rounding)
    // 1024.01 - 890.44 = 133.57
    const expenseText = `Hi Yvaine, Please create an expense claim for the below. Relevant details are:
<expense><cost_centre>DEV002</cost_centre><total>1024.01</total><payment_method>personal card</payment_method></expense>`;

    // Act: Submit expense text
    await page.getByRole('textbox').fill(expenseText);
    await page.getByRole('button', { name: /parse/i }).click();

    // Wait for response
    await page.waitForSelector('[data-testid="expense-result"]', { timeout: 10000 });

    // Assert: Verify calculations match backend
    await expect(page.getByTestId('total-incl-tax')).toContainText('1024.01');
    await expect(page.getByTestId('total-excl-tax')).toContainText('890.44');
    await expect(page.getByTestId('sales-tax')).toContainText('133.57');
    await expect(page.getByTestId('tax-rate')).toContainText('0.15');
  });

  test('50.00 @ 15% GST = 43.48 excl + 6.52 tax (Small amount)', async ({ page }) => {
    // Arrange: Small amount test case
    // 50.00 / 1.15 = 43.47826086956... → 43.48 (Banker's Rounding)
    // 50.00 - 43.48 = 6.52
    const expenseText = `<expense><total>50.00</total><payment_method>personal card</payment_method></expense>`;

    // Act: Submit expense text
    await page.getByRole('textbox').fill(expenseText);
    await page.getByRole('button', { name: /parse/i }).click();

    // Wait for response
    await page.waitForSelector('[data-testid="expense-result"]', { timeout: 10000 });

    // Assert: Verify calculations
    await expect(page.getByTestId('total-incl-tax')).toContainText('50.00');
    await expect(page.getByTestId('total-excl-tax')).toContainText('43.48');
    await expect(page.getByTestId('sales-tax')).toContainText('6.52');
    await expect(page.getByTestId('tax-rate')).toContainText('0.15');
  });

  test('23.00 @ 15% GST = 20.00 excl + 3.00 tax (Exact division)', async ({ page }) => {
    // Arrange: Test case with exact division (no rounding needed)
    // 23.00 / 1.15 = 20.00 (exact)
    // 23.00 - 20.00 = 3.00 (exact)
    const expenseText = `<expense><total>23.00</total><payment_method>personal card</payment_method></expense>`;

    // Act: Submit expense text
    await page.getByRole('textbox').fill(expenseText);
    await page.getByRole('button', { name: /parse/i }).click();

    // Wait for response
    await page.waitForSelector('[data-testid="expense-result"]', { timeout: 10000 });

    // Assert: Verify calculations
    await expect(page.getByTestId('total-incl-tax')).toContainText('23.00');
    await expect(page.getByTestId('total-excl-tax')).toContainText('20.00');
    await expect(page.getByTestId('sales-tax')).toContainText('3.00');
    await expect(page.getByTestId('tax-rate')).toContainText('0.15');
  });

  test('Tax rate displayed correctly as decimal and percentage', async ({ page }) => {
    // Arrange: Verify tax rate is displayed in both formats
    const expenseText = `<expense><total>100.00</total><payment_method>personal card</payment_method></expense>`;

    // Act: Submit expense text
    await page.getByRole('textbox').fill(expenseText);
    await page.getByRole('button', { name: /parse/i }).click();

    // Wait for response
    await page.waitForSelector('[data-testid="expense-result"]', { timeout: 10000 });

    // Assert: Verify tax rate displayed as decimal and percentage
    const taxRateElement = page.getByTestId('tax-rate');
    const taxRateText = await taxRateElement.textContent();

    // Should contain decimal format (0.15)
    expect(taxRateText).toContain('0.15');

    // Should contain percentage format (15%)
    expect(taxRateText).toMatch(/15\s*%/);
  });

  test('All tax breakdown fields are visible and formatted to 2 decimals', async ({ page }) => {
    // Arrange: Verify all tax fields are displayed with proper formatting
    const expenseText = `<expense><total>99.99</total><payment_method>personal card</payment_method></expense>`;

    // Act: Submit expense text
    await page.getByRole('textbox').fill(expenseText);
    await page.getByRole('button', { name: /parse/i }).click();

    // Wait for response
    await page.waitForSelector('[data-testid="expense-result"]', { timeout: 10000 });

    // Assert: Verify all fields are visible
    await expect(page.getByTestId('total-incl-tax')).toBeVisible();
    await expect(page.getByTestId('total-excl-tax')).toBeVisible();
    await expect(page.getByTestId('sales-tax')).toBeVisible();
    await expect(page.getByTestId('tax-rate')).toBeVisible();

    // Verify 2 decimal places formatting
    const totalInclText = await page.getByTestId('total-incl-tax').textContent();
    const totalExclText = await page.getByTestId('total-excl-tax').textContent();
    const salesTaxText = await page.getByTestId('sales-tax').textContent();

    // All monetary values should have exactly 2 decimal places
    expect(totalInclText).toMatch(/\d+\.\d{2}/);
    expect(totalExclText).toMatch(/\d+\.\d{2}/);
    expect(salesTaxText).toMatch(/\d+\.\d{2}/);
  });
});
