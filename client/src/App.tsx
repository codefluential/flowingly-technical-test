import { useState } from 'react';
import { parseText, ApiError } from './api/parseClient';
import type { ParseResponse } from './types/api';
import ParseForm from './components/ParseForm';
import ResultDisplay from './components/ResultDisplay';
import ErrorBanner from './components/ErrorBanner';
import './App.css';

function App() {
  const [inputText, setInputText] = useState('');
  const [result, setResult] = useState<ParseResponse | null>(null);
  const [error, setError] = useState<ApiError | null>(null);
  const [loading, setLoading] = useState(false);

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

        {result && <ResultDisplay result={result} />}
      </main>

      <footer>
        {result && (
          <p>Correlation ID: {result.meta.correlationId}</p>
        )}
      </footer>
    </div>
  );
}

export default App;
