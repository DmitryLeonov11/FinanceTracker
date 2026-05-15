import { http } from '@/shared/api/http'
import { AccountSchema, type Account, type CreateAccountCommand } from '../model/schemas'

export const accountsApi = {
  async create(cmd: CreateAccountCommand): Promise<Account> {
    const { data } = await http.post('/accounts', cmd)
    return AccountSchema.parse(data)
  }
}
