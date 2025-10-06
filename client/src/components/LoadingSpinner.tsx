import './LoadingSpinner.css';

interface LoadingSpinnerProps {
  size?: 'small' | 'medium' | 'large';
  message?: string;
}

/**
 * Accessible loading spinner component with ARIA live region.
 * Provides visual and screen reader feedback during async operations.
 */
function LoadingSpinner({ size = 'medium', message = 'Loading, please wait...' }: LoadingSpinnerProps) {
  return (
    <div
      className={`loading-spinner loading-spinner--${size}`}
      role="status"
      aria-live="polite"
      aria-label={message}
    >
      <div className="loading-spinner__icon" aria-hidden="true">
        <div className="loading-spinner__circle"></div>
      </div>
      <span className="sr-only">{message}</span>
    </div>
  );
}

export default LoadingSpinner;
