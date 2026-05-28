import { http } from '@/shared/api/http'
import { z } from 'zod'
import {
  BudgetSchema,
  BudgetWithProgressSchema,
  type Budget,
  type BudgetWithProgress,
  type CreateBudgetCommand,
  type UpdateBudgetCommand
} from '../model/schemas'

const BudgetListSchema = z.array(BudgetWithProgressSchema)

export const budgetsApi = {
  async list(includeClosed = false): Promise<BudgetWithProgress[]> {
    const { data } = await http.get('/budgets', { params: { includeClosed } })
    return BudgetListSchema.parse(data)
  },
  async create(cmd: CreateBudgetCommand): Promise<Budget> {
    const { data } = await http.post('/budgets', cmd)
    return BudgetSchema.parse(data)
  },
  async update(id: string, cmd: UpdateBudgetCommand): Promise<Budget> {
    const { data } = await http.patch(`/budgets/${id}`, cmd)
    return BudgetSchema.parse(data)
  },
  async close(id: string): Promise<void> {
    await http.delete(`/budgets/${id}`)
  }
}
