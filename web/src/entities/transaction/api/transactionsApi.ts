import { http } from '@/shared/api/http'
import {
  PagedTransactionsSchema,
  TransactionSchema,
  type AddTransactionCommand,
  type PagedTransactions,
  type Transaction,
  type TransactionFilters
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
  }
}
