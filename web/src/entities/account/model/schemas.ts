import { z } from 'zod'
import { CurrencySchema } from '@/shared/api/schemas'

export const AccountTypeSchema = z.enum(['Cash', 'Bank', 'Card', 'Crypto', 'Other'])
export type AccountType = z.infer<typeof AccountTypeSchema>

export const AccountSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  type: AccountTypeSchema,
  currency: CurrencySchema,
  balance: z.number(),
  isArchived: z.boolean(),
  createdAt: z.string()
})
export type Account = z.infer<typeof AccountSchema>

export const BalancePointSchema = z.object({
  date: z.string(),
  balance: z.number()
})
export type BalancePoint = z.infer<typeof BalancePointSchema>

export const AccountBalanceHistorySchema = z.object({
  accountId: z.string().uuid(),
  currency: CurrencySchema,
  points: z.array(BalancePointSchema)
})
export type AccountBalanceHistory = z.infer<typeof AccountBalanceHistorySchema>

export const CreateAccountCommandSchema = z.object({
  name: z.string().min(1, 'Название обязательно').max(100),
  type: AccountTypeSchema,
  currency: CurrencySchema,
  initialBalance: z.number().min(0, 'Не может быть отрицательным')
})
export type CreateAccountCommand = z.infer<typeof CreateAccountCommandSchema>
