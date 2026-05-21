import { z } from 'zod'
import { CurrencySchema } from '@/shared/api/schemas'

export const CashflowPointSchema = z.object({
  date: z.string(), // ISO date YYYY-MM-DD from .NET DateOnly
  income: z.number(),
  expense: z.number()
})
export type CashflowPoint = z.infer<typeof CashflowPointSchema>

export const CashflowSchema = z.object({
  currency: CurrencySchema,
  from: z.string(),
  to: z.string(),
  totalIncome: z.number(),
  totalExpense: z.number(),
  net: z.number(),
  points: z.array(CashflowPointSchema)
})
export type Cashflow = z.infer<typeof CashflowSchema>
