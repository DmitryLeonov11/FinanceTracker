import { z } from 'zod'
import { CurrencySchema } from '@/shared/api/schemas'

export const FxRateSchema = z.object({
  currency: CurrencySchema,
  rateToUsd: z.number(),
  date: z.string() // ISO date (YYYY-MM-DD)
})
export type FxRate = z.infer<typeof FxRateSchema>

export const FxConversionSchema = z.object({
  sourceAmount: z.number(),
  sourceCurrency: CurrencySchema,
  targetAmount: z.number(),
  targetCurrency: CurrencySchema,
  rateApplied: z.number(),
  rateDate: z.string()
})
export type FxConversion = z.infer<typeof FxConversionSchema>
