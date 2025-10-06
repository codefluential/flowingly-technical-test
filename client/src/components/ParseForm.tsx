import { useEffect, useRef } from 'react';
import LoadingSpinner from './LoadingSpinner';

interface ParseFormProps {
  value: string;
  onChange: (value: string) => void;
  onSubmit: () => void;
  onClear: () => void;
  loading: boolean;
  disabled: boolean;
}

/**
 * Enhanced parse form with accessibility features:
 * - ARIA labels for screen readers
 * - Auto-focus on mount
 * - Keyboard shortcuts (Ctrl+Enter to submit)
 * - Loading spinner during async operations
 */
export default function ParseForm({
  value,
  onChange,
  onSubmit,
  onClear,
  loading,
  disabled,
}: ParseFormProps) {
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Auto-focus textarea on mount
  useEffect(() => {
    if (textareaRef.current) {
      textareaRef.current.focus();
    }
  }, []);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (value.trim() && !loading) {
      onSubmit();
    }
  };

  // Keyboard shortcut: Ctrl+Enter or Cmd+Enter to submit
  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
      e.preventDefault();
      if (value.trim() && !loading) {
        onSubmit();
      }
    }
  };

  return (
    <form onSubmit={handleSubmit} className="parse-form" aria-label="Parse expense text">
      <label htmlFor="input-text">
        Enter text to parse:
        <span className="visually-hidden">
          Paste email body or text with expense tags. Press Ctrl+Enter to submit.
        </span>
      </label>
      <textarea
        ref={textareaRef}
        id="input-text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="Paste email content here... (Ctrl+Enter to submit)"
        rows={10}
        disabled={disabled}
        aria-label="Expense text input"
        aria-required="true"
        aria-describedby="input-help"
      />
      <span id="input-help" className="visually-hidden">
        Enter expense content with XML tags or inline tags. Press Ctrl+Enter to parse.
      </span>

      <div className="form-actions">
        <button
          type="submit"
          disabled={disabled || !value.trim() || loading}
          aria-label={loading ? 'Parsing in progress' : 'Parse text'}
        >
          {loading ? 'Parsing...' : 'Parse'}
        </button>
        <button
          type="button"
          onClick={onClear}
          disabled={disabled || loading}
          aria-label="Clear input and results"
        >
          Clear
        </button>
      </div>

      {loading && (
        <div className="parse-form__loading">
          <LoadingSpinner size="medium" message="Parsing your content..." />
        </div>
      )}
    </form>
  );
}
