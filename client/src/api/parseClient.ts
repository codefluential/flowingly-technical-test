// Fetch-based API client for Flowingly Parsing Service
// POST /api/v1/parse endpoint with type-safe error handling

import type { ParseRequest, ParseResponse, ErrorResponse } from '../types/api';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api/v1';

/**
 * Custom error class for API errors with correlation ID support
 */
export class ApiError extends Error {
  code: string;
  correlationId?: string;

  constructor(
    message: string,
    code: string,
    correlationId?: string
  ) {
    super(message);
    this.name = 'ApiError';
    this.code = code;
    this.correlationId = correlationId;
  }
}

/**
 * Parse text using the Flowingly Parsing Service
 * @param text - Raw text to parse (email body, etc.)
 * @param taxRate - Optional tax rate (defaults to 0.15 / NZ GST if omitted)
 * @returns ParseResponse - Discriminated union (expense XOR other)
 * @throws ApiError - API errors with code and correlation ID
 * @throws ApiError - Network errors (fetch failures)
 */
export async function parseText(
  text: string,
  taxRate?: number
): Promise<ParseResponse> {
  const request: ParseRequest = { text, taxRate };

  try {
    const response = await fetch(`${API_BASE_URL}/parse`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const errorData: ErrorResponse = await response.json();
      throw new ApiError(
        errorData.message,
        errorData.errorCode,
        errorData.correlationId
      );
    }

    return await response.json();
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }
    // Network error or fetch failure
    throw new ApiError(
      'Network error: Unable to reach parsing service',
      'NETWORK_ERROR'
    );
  }
}
