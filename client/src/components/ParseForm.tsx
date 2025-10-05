interface ParseFormProps {
  value: string;
  onChange: (value: string) => void;
  onSubmit: () => void;
  onClear: () => void;
  loading: boolean;
  disabled: boolean;
}

export default function ParseForm({
  value,
  onChange,
  onSubmit,
  onClear,
  loading,
  disabled,
}: ParseFormProps) {
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (value.trim()) {
      onSubmit();
    }
  };

  return (
    <form onSubmit={handleSubmit} className="parse-form">
      <label htmlFor="input-text">
        Enter text to parse:
      </label>
      <textarea
        id="input-text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder="Paste email content here..."
        rows={10}
        disabled={disabled}
        aria-label="Text input for parsing"
      />

      <div className="form-actions">
        <button
          type="submit"
          disabled={disabled || !value.trim()}
        >
          {loading ? 'Parsing...' : 'Parse'}
        </button>
        <button
          type="button"
          onClick={onClear}
          disabled={disabled}
        >
          Clear
        </button>
      </div>
    </form>
  );
}
