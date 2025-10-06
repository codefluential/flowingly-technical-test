/**
 * TypeScript interfaces for Flowingly Parsing Service API contracts.
 * Aligns with C# DTOs from ADR-0007 (Classification-Specific Response).
 *
 * Key Design Principles:
 * - Discriminated union for type-safe response handling (Expense XOR Other)
 * - C# PascalCase → JSON camelCase (ASP.NET default serialization)
 * - C# Guid → string, decimal → number, List<T> → T[], Dictionary<K,V> → Record<K,V>
 */

// ===========================
// REQUEST CONTRACT
// ===========================

/**
 * Parse request payload.
 * Matches C# ParseRequest.cs contract.
 */
export interface ParseRequest {
  /** Required text content to parse (email body, message, etc.) */
  text: string;
  /** Optional tax rate override (e.g., 0.15 for 15% NZ GST). Defaults to 0.15 if omitted. */
  taxRate?: number;
}

// ===========================
// RESPONSE CONTRACTS (DISCRIMINATED UNION)
// ===========================

/**
 * Parse response (Discriminated Union: Expense XOR Other).
 * TypeScript can narrow types based on 'classification' field:
 * - If classification === 'expense', then 'expense' field exists and 'other' doesn't
 * - If classification === 'other', then 'other' field exists and 'expense' doesn't
 */
export type ParseResponse = ExpenseResponse | OtherResponse;

/**
 * Expense classification response.
 * Returned when content is classified as an expense claim.
 */
export interface ExpenseResponse {
  classification: 'expense';
  expense: ExpenseData;
  meta: ResponseMeta;
}

/**
 * Other (non-expense) classification response.
 * Returned when content doesn't match expense patterns.
 */
export interface OtherResponse {
  classification: 'other';
  other: OtherData;
  meta: ResponseMeta;
}

// ===========================
// DATA TRANSFER OBJECTS (DTOs)
// ===========================

/**
 * Structured expense data extracted from parsed content.
 * Matches C# ExpenseData.cs contract (camelCase field names in JSON).
 */
export interface ExpenseData {
  /** Vendor or merchant name (extracted from <vendor> tag or defaulted) */
  vendor: string;
  /** Total amount including tax (tax-inclusive) from <total> tag */
  total: number;
  /** Total amount excluding tax (calculated via tax calculator) */
  totalExclTax: number;
  /** Sales tax amount (GST) calculated from inclusive total */
  salesTax: number;
  /** Cost centre code for expense allocation (defaults to "UNKNOWN" if not provided) */
  costCentre: string;
  /** Optional description or notes about the expense */
  description: string | null;
  /** Payment method used (e.g., "personal card", "company card") */
  paymentMethod: string;
  /** Tax rate applied to this expense (e.g., 0.15 for 15% NZ GST) */
  taxRate: number;
  /** Currency code for the expense (e.g., "NZD", "USD"). Defaults to "NZD". */
  currency: string;
  /** Source of the expense data (e.g., "expense-xml" for XML islands, "inline" for inline tags) */
  source: string;
  /** Date of the expense (if provided in <date> tag) */
  date: string | null;
  /** Time of the expense (if provided and successfully parsed from <time> tag) */
  time: string | null;
}

/**
 * Data for non-expense content that doesn't match known processors.
 * Matches C# OtherData.cs contract.
 */
export interface OtherData {
  /** Dictionary of tag names to their values (e.g., reservation data). Preserved for future processing. */
  rawTags: Record<string, string>;
  /** Human-readable note indicating this content is unprocessed (e.g., "This content is not recognized as an expense claim.") */
  note: string;
}

/**
 * Metadata included in all parse responses for traceability and diagnostics.
 * Matches C# ResponseMeta.cs contract.
 */
export interface ResponseMeta {
  /** Unique correlation identifier for request tracing and logging (Guid serialized to string) */
  correlationId: string;
  /** List of non-critical warnings encountered during parsing */
  warnings: string[];
  /** List of tag names found in the parsed content */
  tagsFound: string[];
  /** Processing time in milliseconds for the request */
  processingTimeMs: number;
}

// ===========================
// ERROR CONTRACTS
// ===========================

/**
 * Standard error response for validation failures and processing errors.
 * Matches C# ErrorResponse.cs contract (camelCase field names in JSON).
 */
export interface ErrorResponse {
  /** Correlation ID for traceability (Guid serialized to string) */
  correlationId: string;
  /** Error code (e.g., UNCLOSED_TAG, INVALID_XML, MISSING_TOTAL) */
  errorCode: string;
  /** Human-readable error message */
  message: string;
  /** Additional error details (optional, e.g., tag positions, validation failures) */
  details?: Record<string, string> | null;
}
