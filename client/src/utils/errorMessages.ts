/**
 * Error code to user-friendly message mapping.
 * Maps all backend error codes (from ErrorCodes.cs) to actionable, user-friendly messages.
 *
 * Key Design Principles:
 * - Messages are actionable (tell users HOW to fix the issue)
 * - Messages are user-friendly (avoid technical jargon)
 * - Messages guide users to resolve validation/processing failures
 *
 * Error Codes (from ADR-0008, PRD Section 20.2):
 * - UNCLOSED_TAGS: Stack-based tag validation failure
 * - MALFORMED_TAGS: Overlapping/mismatched tags
 * - MISSING_TOTAL: Required <total> tag absent in expense content
 * - EMPTY_TEXT: Empty or whitespace-only input
 * - INVALID_REQUEST: General validation failure
 * - MISSING_TAXRATE: Tax rate required but not provided
 * - INTERNAL_ERROR: Unhandled server error
 */

export const ERROR_MESSAGES: Record<string, string> = {
  UNCLOSED_TAGS:
    'Your text contains unclosed or overlapping tags. Please ensure all tags are properly closed (e.g., <tag>content</tag>).',

  MALFORMED_TAGS:
    'Your text contains malformed tags. Please check for overlapping tags (e.g., <a><b></a></b> is invalid).',

  MISSING_TOTAL:
    'Expense content is missing a total amount. Please include a <total> tag with the expense total (including tax).',

  EMPTY_TEXT:
    'Please enter some text to parse before submitting.',

  INVALID_REQUEST:
    'Your request is invalid. Please check your input and try again.',

  MISSING_TAXRATE:
    'Tax rate is required. Please provide a tax rate in your request or configure a default.',

  INTERNAL_ERROR:
    'An unexpected error occurred. Please try again or contact support.',

  // Network error (from parseClient.ts)
  NETWORK_ERROR:
    'Unable to reach the parsing service. Please check your connection and try again.',

  // Fallback for unknown error codes
  UNKNOWN_ERROR:
    'An unknown error occurred. Please try again or contact support.',
};

/**
 * Get user-friendly error message for a given error code.
 * Returns a fallback message if the error code is not recognized.
 *
 * @param errorCode - Error code from backend (e.g., "UNCLOSED_TAGS", "MISSING_TOTAL")
 * @returns User-friendly, actionable error message
 */
export function getErrorMessage(errorCode: string): string {
  return ERROR_MESSAGES[errorCode] || ERROR_MESSAGES.UNKNOWN_ERROR;
}
