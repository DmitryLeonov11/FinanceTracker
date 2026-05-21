import { toast } from 'vue-sonner'
import { ApiError } from '@/shared/api/errors'
import { queryClient } from '@/app/plugins/vueQuery'
import { loadQueue, removeFromQueue, updateInQueue } from './queue'
import { runners } from './runners'
import { isNetworkError, isPermanent } from './networkError'
import { pendingIds, refreshPendingIds, isReplaying } from './state'

const MAX_ATTEMPTS = 5
const MAX_BACKOFF_MS = 60_000

export async function replayQueue(): Promise<void> {
  if (typeof navigator !== 'undefined' && !navigator.onLine) return
  if (isReplaying.value) return
  isReplaying.value = true

  try {
    const queue = await loadQueue()
    const now = Date.now()

    for (const op of queue) {
      if (op.retryAfter && now < op.retryAfter) continue

      try {
        // typescript: discriminated union
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        await (runners as any)[op.type](op)
        await removeFromQueue(op.id)
        pendingIds.value.delete(op.id)
        refreshPendingIds()
      } catch (err) {
        if (isNetworkError(err)) {
          // we just lost the network mid-replay — stop and wait for next online
          return
        }

        if (isPermanent(err)) {
          await removeFromQueue(op.id)
          pendingIds.value.delete(op.id)
          refreshPendingIds()
          const message = err instanceof ApiError ? err.message : 'неизвестная ошибка'
          toast.error(`Операция отклонена: ${message}`)
          continue
        }

        // 5xx / 408 / 429: exponential backoff
        const updated = {
          ...op,
          attempts: op.attempts + 1,
          lastError: err instanceof Error ? err.message : String(err),
          retryAfter: Date.now() + Math.min(MAX_BACKOFF_MS, 1000 * 2 ** (op.attempts + 1))
        }

        if (updated.attempts > MAX_ATTEMPTS) {
          await removeFromQueue(op.id)
          pendingIds.value.delete(op.id)
          refreshPendingIds()
          toast.error('Операция не отправлена после нескольких попыток — отменена')
        } else {
          await updateInQueue(updated)
        }
      }
    }

    // refresh server state after successful flush
    queryClient.invalidateQueries({ queryKey: ['transactions'] })
    queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    queryClient.invalidateQueries({ queryKey: ['accounts'] })
  } finally {
    isReplaying.value = false
  }
}
