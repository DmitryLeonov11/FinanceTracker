import { z } from 'zod'
import { CurrencySchema } from '@/shared/api/schemas'

export const CurrencyBalanceSchema = z.object({
  currency: CurrencySchema,
  total: z.number(),
  accountCount: z.number().int()
})
export type CurrencyBalance = z.infer<typeof CurrencyBalanceSchema>

export const AccountBalanceSchema = z.object({
  accountId: z.string().uuid(),
  name: z.string(),
  currency: CurrencySchema,
  balance: z.number()
})
export type AccountBalance = z.infer<typeof AccountBalanceSchema>

export const DashboardBalanceSchema = z.object({
  displayCurrency: CurrencySchema,
  balancesByCurrency: z.array(CurrencyBalanceSchema),
  accounts: z.array(AccountBalanceSchema)
})
export type DashboardBalance = z.infer<typeof DashboardBalanceSchema>
