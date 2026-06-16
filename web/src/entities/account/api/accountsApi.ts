import { http } from '@/shared/api/http'
import {
  AccountSchema,
  AccountBalanceHistorySchema,
  type Account,
  type AccountBalanceHistory,
  type CreateAccountCommand
} from '../model/schemas'

export const accountsApi = {
  async create(cmd: CreateAccountCommand): Promise<Account> {
    const { data } = await http.post('/accounts', cmd)
    return AccountSchema.parse(data)
  },

  async getById(id: string): Promise<Account> {
    const { data } = await http.get(`/accounts/${id}`)
    return AccountSchema.parse(data)
  },

  async getBalanceHistory(id: string, days = 30): Promise<AccountBalanceHistory> {
    const { data } = await http.get(`/accounts/${id}/balance-history`, { params: { days } })
    return AccountBalanceHistorySchema.parse(data)
  },

  async rename(id: string, name: string): Promise<Account> {
    const { data } = await http.patch(`/accounts/${id}`, { name })
    return AccountSchema.parse(data)
  },

  async archive(id: string): Promise<void> {
    await http.delete(`/accounts/${id}`)
  }
}
