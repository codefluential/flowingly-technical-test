import ClassificationBadge from './ClassificationBadge';
import type { ExpenseData, ResponseMeta } from '../types/api';
import './ExpenseView.css';

interface ExpenseViewProps {
  expense: ExpenseData;
  meta: ResponseMeta;
}

/**
 * Displays expense data with all 10 fields including tax breakdown.
 * Shows classification badge, correlation ID, and warnings if present.
 */
function ExpenseView({ expense, meta }: ExpenseViewProps) {
  return (
    <div className="expense-view" data-testid="result-display">
      <div className="expense-view__header">
        <h2>Parsed Expense</h2>
        <ClassificationBadge type="expense" />
      </div>

      <dl className="expense-view__data" data-testid="expense-result">
        <dt>Vendor:</dt>
        <dd data-testid="vendor">{expense.vendor}</dd>

        <dt>Description:</dt>
        <dd data-testid="description">{expense.description || 'N/A'}</dd>

        <dt>Total (incl. tax):</dt>
        <dd className="expense-view__highlight" data-testid="total-incl-tax">
          ${expense.total.toFixed(2)}
        </dd>

        <dt>Total (excl. tax):</dt>
        <dd data-testid="total-excl-tax">${expense.totalExclTax.toFixed(2)}</dd>

        <dt>Sales Tax ({(expense.taxRate * 100).toFixed(0)}%):</dt>
        <dd data-testid="sales-tax">${expense.salesTax.toFixed(2)}</dd>

        <dt>Tax Rate:</dt>
        <dd data-testid="tax-rate">{expense.taxRate} ({(expense.taxRate * 100).toFixed(0)}%)</dd>

        <dt>Cost Centre:</dt>
        <dd data-testid="cost-centre">{expense.costCentre}</dd>

        {expense.date && (
          <>
            <dt>Date:</dt>
            <dd>{expense.date}</dd>
          </>
        )}

        {expense.time && (
          <>
            <dt>Time:</dt>
            <dd>{expense.time}</dd>
          </>
        )}
      </dl>

      {meta.warnings && meta.warnings.length > 0 && (
        <div className="expense-view__warnings" role="alert">
          <strong>Warnings:</strong>
          <ul>
            {meta.warnings.map((warning: string, index: number) => (
              <li key={index}>{warning}</li>
            ))}
          </ul>
        </div>
      )}

      <div className="expense-view__footer">
        <p>
          <strong>Correlation ID:</strong> <span data-testid="correlation-id">{meta.correlationId}</span>
        </p>
        {meta.processingTimeMs > 0 && (
          <p className="expense-view__timing">
            Processed in {meta.processingTimeMs}ms
          </p>
        )}
      </div>
    </div>
  );
}

export default ExpenseView;
