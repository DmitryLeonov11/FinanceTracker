export type PeriodPreset = 'week' | 'month' | 'quarter' | 'year' | 'all'

export interface DateRange {
  from?: string
  to?: string
}

const startOfDay = (d: Date) => {
  const x = new Date(d)
  x.setHours(0, 0, 0, 0)
  return x
}

export function rangeForPreset(preset: PeriodPreset): DateRange {
  const now = new Date()
  const to = now.toISOString()
  const from = new Date(now)
  switch (preset) {
    case 'week':
      from.setDate(from.getDate() - 7)
      return { from: startOfDay(from).toISOString(), to }
    case 'month':
      from.setMonth(from.getMonth() - 1)
      return { from: startOfDay(from).toISOString(), to }
    case 'quarter':
      from.setMonth(from.getMonth() - 3)
      return { from: startOfDay(from).toISOString(), to }
    case 'year':
      from.setFullYear(from.getFullYear() - 1)
      return { from: startOfDay(from).toISOString(), to }
    case 'all':
      return {}
  }
}

export const PERIOD_LABELS: Record<PeriodPreset, string> = {
  week: '7 дней',
  month: '30 дней',
  quarter: '3 месяца',
  year: 'Год',
  all: 'Всё время'
}

export const PERIOD_ORDER: PeriodPreset[] = ['week', 'month', 'quarter', 'year', 'all']
