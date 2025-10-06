import ExpenseView from './ExpenseView';
import OtherView from './OtherView';
import type { ParseResponse } from '../types/api';

interface ResponseDisplayProps {
  response: ParseResponse;
}

/**
 * Discriminated union handler for ParseResponse.
 * Renders ExpenseView OR OtherView based on classification (never both).
 * Implements type-safe response handling per ADR-0007.
 */
function ResponseDisplay({ response }: ResponseDisplayProps) {
  // Type-safe classification handling with discriminated union
  if (response.classification === 'expense') {
    return <ExpenseView expense={response.expense} meta={response.meta} />;
  } else {
    return <OtherView other={response.other} meta={response.meta} />;
  }
}

export default ResponseDisplay;
