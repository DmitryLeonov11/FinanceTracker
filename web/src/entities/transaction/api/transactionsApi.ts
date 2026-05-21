import { http } from '@/shared/api/http'
import {
  PagedTransactionsSchema,
  TransactionSchema,
  TransferResultSchema,
  type AddTransactionCommand,
  type EditTransactionCommand,
  type PagedTransactions,
  type RecordTransferCommand,
  type Transaction,
  type TransactionFilters,
  type TransferResult
} from '../model/schemas'

export const transactionsApi = {
  async list(filters: TransactionFilters): Promise<PagedTransactions> {
    const params = new URLSearchParams()
    if (filters.from) params.set('from', filters.from)
    if (filters.to) params.set('to', filters.to)
    for (const id of filters.accountIds ?? []) params.append('accountId', id)
    for (const t of filters.types ?? []) params.append('type', t)
    if (filters.search) params.set('search', filters.search)
    params.set('page', String(filters.page))
    params.set('pageSize', String(filters.pageSize))

    const { data } = await http.get(`/transactions?${params.toString()}`)
    return PagedTransactionsSchema.parse(data)
  },

  async create(cmd: AddTransactionCommand, idempotencyKey?: string): Promise<Transaction> {
    const config = idempotencyKey
      ? { headers: { 'Idempotency-Key': idempotencyKey } }
      : undefined
    const { data } = await http.post('/transactions', cmd, config)
    return TransactionSchema.parse(data)
  },

  async update(id: string, cmd: EditTransactionCommand, idempotencyKey?: string): Promise<Transaction> {
    const config = idempotencyKey ? { headers: { 'Idempotency-Key': idempotencyKey } } : undefined
    const { data } = await http.patch(`/transactions/${id}`, cmd, config)
    return TransactionSchema.parse(data)
  },

  async remove(id: string, idempotencyKey?: string): Promise<void> {
    const config = idempotencyKey ? { headers: { 'Idempotency-Key': idempotencyKey } } : undefined
    await http.delete(`/transactions/${id}`, config)
  },

  async transfer(cmd: RecordTransferCommand, idempotencyKey?: string): Promise<TransferResult> {
    const config = idempotencyKey ? { headers: { 'Idempotency-Key': idempotencyKey } } : undefined
    const { data } = await http.post('/transactions/transfer', cmd, config)
    return TransferResultSchema.parse(data)
  }
}
