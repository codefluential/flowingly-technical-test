import ClassificationBadge from './ClassificationBadge';
import type { OtherData, ResponseMeta } from '../types/api';
import './OtherView.css';

interface OtherViewProps {
  other: OtherData;
  meta: ResponseMeta;
}

/**
 * Displays non-expense content with raw tags and note.
 * Used for reservation requests or other unprocessed content.
 */
function OtherView({ other, meta }: OtherViewProps) {
  const hasRawTags = other.rawTags && Object.keys(other.rawTags).length > 0;

  return (
    <div className="other-view" data-testid="result-display">
      <div className="other-view__header">
        <h2>Other Content</h2>
        <ClassificationBadge type="other" />
      </div>

      <p className="other-view__note">
        This content was classified as non-expense and stored for future processing.
      </p>

      {hasRawTags && (
        <div className="other-view__tags" data-testid="raw-tags">
          <h3>Extracted Tags:</h3>
          <dl>
            {Object.entries(other.rawTags).map(([key, value]) => (
              <div key={key} className="other-view__tag-item">
                <dt>{key}:</dt>
                <dd>{value}</dd>
              </div>
            ))}
          </dl>
        </div>
      )}

      {meta.warnings && meta.warnings.length > 0 && (
        <div className="other-view__warnings" role="alert">
          <strong>Warnings:</strong>
          <ul>
            {meta.warnings.map((warning: string, index: number) => (
              <li key={index}>{warning}</li>
            ))}
          </ul>
        </div>
      )}

      <div className="other-view__footer">
        <p>
          <strong>Correlation ID:</strong> <span data-testid="correlation-id">{meta.correlationId}</span>
        </p>
        {meta.processingTimeMs > 0 && (
          <p className="other-view__timing">
            Processed in {meta.processingTimeMs}ms
          </p>
        )}
      </div>
    </div>
  );
}

export default OtherView;
