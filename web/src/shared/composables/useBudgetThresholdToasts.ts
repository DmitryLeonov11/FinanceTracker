import { onMounted, onUnmounted } from 'vue'
import { toast } from 'vue-sonner'
import { realtime } from '@/shared/api/signalr'

interface ThresholdPayload {
  budgetId: string
  name: string
  threshold: number
  spent: number
  limit: number
  currency: string
  progressPercent: number
}

/**
 * Subscribes to realtime `budget.threshold-reached` events and surfaces a
 * toast with the relevant budget summary. Mount once at app-shell level so
 * the toast appears regardless of which route the user is currently on.
 */
export function useBudgetThresholdToasts() {
  let off: (() => void) | null = null

  onMounted(() => {
    off = realtime.on(({ eventName, payload }) => {
      if (eventName !== 'budget.threshold-reached') return
      const p = payload as ThresholdPayload | null
      if (!p) return

      const formatted = formatAmount(p.spent, p.currency) + ' из ' + formatAmount(p.limit, p.currency)
      const description = `${formatted} (${formatPercent(p.progressPercent)}%)`
      const title = `Бюджет «${p.name}»: порог ${p.threshold}%`

      switch (p.threshold) {
        case 100:
          toast.error(title, { description, duration: 8000 })
          break
        case 80:
          toast.warning(title, { description, duration: 6000 })
          break
        case 50:
        default:
          toast.info(title, { description, duration: 5000 })
          break
      }
    })
  })

  onUnmounted(() => {
    off?.()
  })
}

const moneyFormatter = new Intl.NumberFormat('ru-RU', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2
})

function formatAmount(amount: number, currency: string): string {
  return `${moneyFormatter.format(amount)} ${currency}`
}

function formatPercent(percent: number): string {
  if (Number.isInteger(percent)) return String(percent)
  return percent.toFixed(1).replace('.', ',')
}
