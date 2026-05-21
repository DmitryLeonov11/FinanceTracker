import { z } from 'zod'
import { CurrencySchema } from '@/shared/api/schemas'

export const TransactionTypeSchema = z.enum(['Income', 'Expense', 'Transfer'])
export type TransactionType = z.infer<typeof TransactionTypeSchema>

export const TransactionSchema = z.object({
  id: z.string().uuid(),
  accountId: z.string().uuid(),
  counterAccountId: z.string().uuid().nullable(),
  categoryId: z.string().uuid().nullable(),
  type: TransactionTypeSchema,
  amount: z.number(),
  currency: CurrencySchema,
  occurredAt: z.string(),
  note: z.string().nullable()
})
export type Transaction = z.infer<typeof TransactionSchema>

export const PagedTransactionsSchema = z.object({
  items: z.array(TransactionSchema),
  total: z.number().int(),
  page: z.number().int(),
  pageSize: z.number().int(),
  hasMore: z.boolean()
})
export type PagedTransactions = z.infer<typeof PagedTransactionsSchema>

export const AddTransactionCommandSchema = z.object({
  accountId: z.string().uuid('Выберите счёт'),
  categoryId: z.string().uuid().nullable().optional(),
  type: z.enum(['Income', 'Expense']),
  amount: z.number().positive('Сумма должна быть больше 0'),
  occurredAt: z.string(),
  note: z.string().max(500, 'Не более 500 символов').nullable().optional()
})
export type AddTransactionCommand = z.infer<typeof AddTransactionCommandSchema>

export interface TransactionFilters {
  from?: string
  to?: string
  accountIds?: string[]
  types?: TransactionType[]
  search?: string
  page: number
  pageSize: number
}

export const EditTransactionCommandSchema = z.object({
  amount: z.number().positive('Сумма должна быть больше 0'),
  occurredAt: z.string(),
  categoryId: z.string().uuid().nullable().optional(),
  note: z.string().max(500, 'Не более 500 символов').nullable().optional()
})
export type EditTransactionCommand = z.infer<typeof EditTransactionCommandSchema>

export const RecordTransferCommandSchema = z.object({
  sourceAccountId: z.string().uuid('Выберите счёт-источник'),
  destinationAccountId: z.string().uuid('Выберите счёт-получатель'),
  amount: z.number().positive('Сумма должна быть больше 0'),
  occurredAt: z.string(),
  note: z.string().max(500, 'Не более 500 символов').nullable().optional()
}).refine((v) => v.sourceAccountId !== v.destinationAccountId, {
  message: 'Счёт-источник и счёт-получатель должны различаться',
  path: ['destinationAccountId']
})
export type RecordTransferCommand = z.infer<typeof RecordTransferCommandSchema>

export const TransferResultSchema = z.object({
  transferGroupId: z.string().uuid(),
  outgoingId: z.string().uuid(),
  incomingId: z.string().uuid(),
  sourceAccountId: z.string().uuid(),
  destinationAccountId: z.string().uuid(),
  amount: z.number(),
  currency: CurrencySchema,
  occurredAt: z.string(),
  note: z.string().nullable()
})
export type TransferResult = z.infer<typeof TransferResultSchema>
