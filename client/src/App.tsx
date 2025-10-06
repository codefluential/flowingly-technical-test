import { useState, useEffect } from 'react';
import { parseText, ApiError } from './api/parseClient';
import type { ParseResponse } from './types/api';
import ParseForm from './components/ParseForm';
import ResponseDisplay from './components/ResponseDisplay';
import ErrorBanner from './components/ErrorBanner';
import './App.css';

type TextSize = 'normal' | 'large' | 'xlarge';

function App() {
  const [inputText, setInputText] = useState('');
  const [result, setResult] = useState<ParseResponse | null>(null);
  const [error, setError] = useState<ApiError | null>(null);
  const [loading, setLoading] = useState(false);

  // Accessibility controls with localStorage persistence
  const [highContrast, setHighContrast] = useState(
    localStorage.getItem('highContrast') === 'true'
  );
  const [textSize, setTextSize] = useState<TextSize>(
    (localStorage.getItem('textSize') as TextSize) || 'normal'
  );

  // Persist high-contrast mode
  useEffect(() => {
    localStorage.setItem('highContrast', String(highContrast));
    document.body.classList.toggle('high-contrast', highContrast);
  }, [highContrast]);

  // Persist text size
  useEffect(() => {
    localStorage.setItem('textSize', textSize);
    // Remove all text size classes first
    document.body.classList.remove('text-large', 'text-xlarge');
    // Add appropriate class
    if (textSize === 'large') {
      document.body.classList.add('text-large');
    } else if (textSize === 'xlarge') {
      document.body.classList.add('text-xlarge');
    }
  }, [textSize]);

  const handleSubmit = async (text: string) => {
    setError(null);
    setResult(null);
    setLoading(true);

    try {
      const response = await parseText(text);
      setResult(response);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err);
      } else {
        setError(new ApiError('Unknown error occurred', 'UNKNOWN_ERROR'));
      }
    } finally {
      setLoading(false);
    }
  };

  const handleClear = () => {
    setInputText('');
    setResult(null);
    setError(null);
  };

  return (
    <div className="app-container">
      <header>
        <h1>Flowingly Parsing Service</h1>
        <p>Extract structured expense data from free-form text</p>
      </header>

      {/* Accessibility Controls */}
      <div className="accessibility-controls">
        <button
          onClick={() => setHighContrast(!highContrast)}
          className={highContrast ? 'active' : ''}
          aria-pressed={highContrast}
          aria-label="Toggle high contrast mode"
        >
          {highContrast ? '✓ ' : ''}High Contrast
        </button>
        <button
          onClick={() => setTextSize('normal')}
          className={textSize === 'normal' ? 'active' : ''}
          aria-pressed={textSize === 'normal'}
          aria-label="Set text size to normal"
        >
          {textSize === 'normal' ? '✓ ' : ''}Normal Text
        </button>
        <button
          onClick={() => setTextSize('large')}
          className={textSize === 'large' ? 'active' : ''}
          aria-pressed={textSize === 'large'}
          aria-label="Set text size to large"
        >
          {textSize === 'large' ? '✓ ' : ''}Large Text
        </button>
        <button
          onClick={() => setTextSize('xlarge')}
          className={textSize === 'xlarge' ? 'active' : ''}
          aria-pressed={textSize === 'xlarge'}
          aria-label="Set text size to extra large"
        >
          {textSize === 'xlarge' ? '✓ ' : ''}X-Large Text
        </button>
      </div>

      <main>
        <ParseForm
          value={inputText}
          onChange={setInputText}
          onSubmit={() => handleSubmit(inputText)}
          onClear={handleClear}
          loading={loading}
          disabled={loading}
        />

        {error && (
          <ErrorBanner
            error={error}
            onDismiss={() => setError(null)}
          />
        )}

        {result && <ResponseDisplay response={result} />}
      </main>

      <footer>
        <p>Flowingly Parsing Service - Extract expenses from free-form text</p>
      </footer>
    </div>
  );
}

export default App;
