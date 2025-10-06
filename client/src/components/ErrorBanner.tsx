import { useEffect } from 'react';
import type { ApiError } from '../api/parseClient';
import { getErrorMessage } from '../utils/errorMessages';

interface ErrorBannerProps {
  error: ApiError;
  onDismiss: () => void;
}

/**
 * ErrorBanner component for displaying comprehensive error information.
 *
 * Features:
 * - User-friendly error messages mapped from error codes
 * - Correlation ID display for support reference
 * - Keyboard dismissible (ESC key)
 * - Accessible (ARIA attributes, role='alert')
 * - Prominent visual styling (red/warning color scheme)
 *
 * Design Principles (from ADR-0008, Task_043):
 * - Error messages are actionable and guide users to fix issues
 * - Correlation ID is prominently labeled "Support Reference"
 * - ARIA live region ensures screen reader announcements
 * - ESC key and close button for dismissal
 */
export default function ErrorBanner({ error, onDismiss }: ErrorBannerProps) {
  // Get user-friendly error message from error code
  const userMessage = getErrorMessage(error.code);

  // Keyboard dismissal (ESC key)
  useEffect(() => {
    const handleEscKey = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onDismiss();
      }
    };

    window.addEventListener('keydown', handleEscKey);
    return () => {
      window.removeEventListener('keydown', handleEscKey);
    };
  }, [onDismiss]);

  return (
    <div
      className="error-banner"
      role="alert"
      aria-live="polite"
      aria-atomic="true"
      data-testid="error-banner"
    >
      <div className="error-content">
        <div className="error-header">
          <strong className="error-code">Error: {error.code}</strong>
        </div>
        <p className="error-message">{userMessage}</p>
        {error.correlationId && (
          <p className="correlation-id">
            <strong>Support Reference:</strong> {error.correlationId}
          </p>
        )}
        {!error.correlationId && (
          <p className="correlation-id">
            <strong>Support Reference:</strong> N/A
          </p>
        )}
      </div>
      <button
        onClick={onDismiss}
        className="dismiss-button"
        aria-label="Dismiss error (press ESC to dismiss)"
        data-testid="dismiss-error-button"
        type="button"
      >
        ×
      </button>
    </div>
  );
}
