import type { ParseResponse } from '../types/api';

interface ResultDisplayProps {
  result: ParseResponse;
}

export default function ResultDisplay({ result }: ResultDisplayProps) {
  if (result.classification === 'expense') {
    const { expense } = result;
    return (
      <div className="result-display expense-result" data-testid="result-display">
        <h2>Expense Data</h2>
        <dl>
          <dt>Vendor:</dt>
          <dd>{expense.vendor}</dd>

          <dt>Description:</dt>
          <dd>{expense.description}</dd>

          <dt>Total (incl. tax):</dt>
          <dd>${expense.total.toFixed(2)}</dd>

          <dt>Tax ({(expense.taxRate * 100).toFixed(0)}%):</dt>
          <dd>${expense.salesTax.toFixed(2)}</dd>

          <dt>Total (excl. tax):</dt>
          <dd>${expense.totalExclTax.toFixed(2)}</dd>

          <dt>Cost Centre:</dt>
          <dd>{expense.costCentre}</dd>

          <dt>Date:</dt>
          <dd>{expense.date}</dd>

          {expense.time && (
            <>
              <dt>Time:</dt>
              <dd>{expense.time}</dd>
            </>
          )}
        </dl>
      </div>
    );
  } else {
    const { other } = result;
    return (
      <div className="result-display other-result" data-testid="result-display">
        <h2>Other/Unprocessed Data</h2>
        <p>The following tags were found but did not match expense patterns:</p>
        <dl>
          {Object.entries(other.rawTags).map(([key, value]) => (
            <div key={key}>
              <dt>{key}:</dt>
              <dd>{value}</dd>
            </div>
          ))}
        </dl>
      </div>
    );
  }
}
