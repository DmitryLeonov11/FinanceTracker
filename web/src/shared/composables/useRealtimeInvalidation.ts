import { onMounted, onUnmounted } from 'vue'
import { useQueryClient } from '@tanstack/vue-query'
import { realtime } from '@/shared/api/signalr'

export function useRealtimeInvalidation() {
  const qc = useQueryClient()
  let off: (() => void) | null = null

  onMounted(() => {
    off = realtime.on(({ eventName }) => {
      switch (eventName) {
        case 'account.created':
        case 'account.balance-changed':
        case 'account.renamed':
        case 'account.archived':
          qc.invalidateQueries({ queryKey: ['dashboard'] })
          qc.invalidateQueries({ queryKey: ['accounts'] })
          break
        case 'transaction.created':
        case 'transaction.updated':
        case 'transaction.deleted':
          qc.invalidateQueries({ queryKey: ['dashboard'] })
          qc.invalidateQueries({ queryKey: ['transactions'] })
          qc.invalidateQueries({ queryKey: ['accounts'] })
          qc.invalidateQueries({ queryKey: ['budgets'] })
          break
        case 'budget.created':
        case 'budget.updated':
        case 'budget.closed':
        case 'budget.threshold-reached':
          qc.invalidateQueries({ queryKey: ['budgets'] })
          break
      }
    })
  })

  onUnmounted(() => {
    off?.()
  })
}
