import { onMounted, onUnmounted } from 'vue'
import { useQueryClient } from '@tanstack/vue-query'
import { realtime } from '@/shared/api/signalr'

/**
 * Subscribes to realtime events from the server hub and invalidates the
 * matching Vue Query keys, triggering automatic refetches.
 *
 * Mount inside a layout or top-level page so a single subscription drives
 * all server-state cache for the user.
 */
export function useRealtimeInvalidation() {
  const qc = useQueryClient()
  let off: (() => void) | null = null

  onMounted(() => {
    off = realtime.on(({ eventName }) => {
      switch (eventName) {
        case 'AccountCreatedEvent':
        case 'BalanceChangedEvent':
        case 'account.created':
        case 'account.balance-changed':
          qc.invalidateQueries({ queryKey: ['dashboard'] })
          qc.invalidateQueries({ queryKey: ['accounts'] })
          break
        case 'TransactionRecordedEvent':
        case 'transaction.created':
          qc.invalidateQueries({ queryKey: ['dashboard'] })
          qc.invalidateQueries({ queryKey: ['transactions'] })
          qc.invalidateQueries({ queryKey: ['accounts'] })
          break
      }
    })
  })

  onUnmounted(() => {
    off?.()
  })
}
