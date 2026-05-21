export type CashflowPeriod = '7d' | '30d' | '90d' | 'year'

export const PERIOD_DAYS: Record<CashflowPeriod, number> = {
  '7d': 7,
  '30d': 30,
  '90d': 90,
  year: 365
}

export const PERIOD_LABELS: Record<CashflowPeriod, string> = {
  '7d': '7 дней',
  '30d': '30 дней',
  '90d': '90 дней',
  year: 'Год'
}

export const PERIOD_ORDER: CashflowPeriod[] = ['7d', '30d', '90d', 'year']
