import { ApiError } from '@/shared/api/errors'

/**
 * Status === 0 is what our http.ts sets when axios can't reach the server
 * (no `error.response`). Treat as "queue and retry later".
 */
export function isNetworkError(err: unknown): boolean {
  if (err instanceof ApiError) return err.status === 0
  if (err instanceof TypeError) return true  // fetch-level network failure
  return false
}

/**
 * Errors that mean "this op will never succeed — drop it from the queue".
 * 4xx except 408 (timeout) / 425 (too early) / 429 (rate limit).
 */
export function isPermanent(err: unknown): boolean {
  if (!(err instanceof ApiError)) return false
  if (err.status < 400 || err.status >= 500) return false
  return err.status !== 408 && err.status !== 425 && err.status !== 429
}
