// TypeScript interfaces for Flowingly Parsing Service API contracts
// Aligns with C# DTOs from ADR-0007 (Classification-Specific Response)

// Request Contract
export interface ParseRequest {
  text: string;
  taxRate?: number;
}

// Response Contracts (Discriminated Union: Expense XOR Other)
export type ParseResponse = ExpenseResponse | OtherResponse;

export interface ExpenseResponse {
  classification: 'expense';
  expense: ExpenseDto;
  meta: Metadata;
}

export interface OtherResponse {
  classification: 'other';
  other: OtherDto;
  meta: Metadata;
}

// DTOs
export interface ExpenseDto {
  vendor: string;
  description: string;
  total: number;
  totalExclTax: number;
  salesTax: number;
  costCentre: string;
  date: string;
  time: string | null;
  taxRate: number;
}

export interface OtherDto {
  rawTags: Record<string, string>;
}

// Metadata
export interface Metadata {
  correlationId: string;
  processingTimeMs: number;
  warnings: string[];
}

// Error Response (matches backend C# ErrorResponse contract)
export interface ErrorResponse {
  CorrelationId: string;
  ErrorCode: string;
  Message: string;
  Details?: Record<string, string> | null;
}
