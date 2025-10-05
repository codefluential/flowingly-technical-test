import type { ApiError } from '../api/parseClient';

interface ErrorBannerProps {
  error: ApiError;
  onDismiss: () => void;
}

export default function ErrorBanner({ error, onDismiss }: ErrorBannerProps) {
  return (
    <div className="error-banner" role="alert">
      <div className="error-content">
        <strong>Error {error.code}:</strong> {error.message}
        {error.correlationId && (
          <p className="correlation-id">
            Correlation ID: {error.correlationId}
          </p>
        )}
      </div>
      <button
        onClick={onDismiss}
        aria-label="Dismiss error"
      >
        ×
      </button>
    </div>
  );
}
