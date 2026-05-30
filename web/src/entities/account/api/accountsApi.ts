import { http } from '@/shared/api/http'
import { AccountSchema, type Account, type CreateAccountCommand } from '../model/schemas'

export const accountsApi = {
  async create(cmd: CreateAccountCommand): Promise<Account> {
    const { data } = await http.post('/accounts', cmd)
    return AccountSchema.parse(data)
  },

  async getById(id: string): Promise<Account> {
    const { data } = await http.get(`/accounts/${id}`)
    return AccountSchema.parse(data)
  },

  async rename(id: string, name: string): Promise<Account> {
    const { data } = await http.patch(`/accounts/${id}`, { name })
    return AccountSchema.parse(data)
  },

  async archive(id: string): Promise<void> {
    await http.delete(`/accounts/${id}`)
  }
}
