import { z } from 'zod'
import { SUPPORTED_CURRENCIES } from '@/shared/config/currencies'

export const CurrencySchema = z.enum(SUPPORTED_CURRENCIES)
export type Currency = z.infer<typeof CurrencySchema>

export const ProblemDetailsSchema = z.object({
  type: z.string().optional(),
  title: z.string().optional(),
  status: z.number().optional(),
  detail: z.string().optional(),
  instance: z.string().optional(),
  errors: z.record(z.string(), z.array(z.string())).optional()
})
export type ProblemDetails = z.infer<typeof ProblemDetailsSchema>
