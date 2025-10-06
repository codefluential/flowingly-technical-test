import './ClassificationBadge.css';

interface ClassificationBadgeProps {
  type: 'expense' | 'other';
}

/**
 * Color-coded badge indicating content classification (expense vs other).
 * Green for expense, blue for other content.
 */
function ClassificationBadge({ type }: ClassificationBadgeProps) {
  const label = type === 'expense' ? 'Expense' : 'Other';
  const ariaLabel = `Classification: ${label}`;

  return (
    <span
      className={`classification-badge classification-badge--${type}`}
      data-testid="classification-badge"
      aria-label={ariaLabel}
      role="status"
    >
      {label}
    </span>
  );
}

export default ClassificationBadge;
