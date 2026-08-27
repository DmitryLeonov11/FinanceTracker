import { ApiError } from '@/shared/api/errors'

// status === 0 — так http.ts помечает случаи, когда axios вообще не достучался до сервера
// (нет error.response). Это повод положить операцию в очередь и повторить позже.
export function isNetworkError(err: unknown): boolean {
  if (err instanceof ApiError) return err.status === 0
  if (err instanceof TypeError) return true
  return false
}

// Ошибки, после которых операцию бессмысленно повторять — сразу убираем из очереди.
// 4xx, кроме 408 (таймаут), 425 (too early) и 429 (rate limit) — те временные.
export function isPermanent(err: unknown): boolean {
  if (!(err instanceof ApiError)) return false
  if (err.status < 400 || err.status >= 500) return false
  return err.status !== 408 && err.status !== 425 && err.status !== 429
}
