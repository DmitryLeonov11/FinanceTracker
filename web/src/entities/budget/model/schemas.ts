import { z } from 'zod'
import { CurrencySchema } from '@/shared/api/schemas'

export const BudgetPeriodSchema = z.enum(['Week', 'Month', 'Quarter', 'Year'])
export type BudgetPeriod = z.infer<typeof BudgetPeriodSchema>

export const BUDGET_PERIOD_LABELS: Record<BudgetPeriod, string> = {
  Week: 'Неделя',
  Month: 'Месяц',
  Quarter: 'Квартал',
  Year: 'Год'
}

export const BudgetSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  categoryId: z.string().uuid().nullable(),
  period: BudgetPeriodSchema,
  currency: CurrencySchema,
  limit: z.number(),
  startDate: z.string(),
  endDate: z.string().nullable(),
  rollover: z.boolean(),
  isClosed: z.boolean(),
  createdAt: z.string()
})
export type Budget = z.infer<typeof BudgetSchema>

export const BudgetWithProgressSchema = BudgetSchema.extend({
  spent: z.number(),
  remaining: z.number(),
  progressPercent: z.number(),
  isOverLimit: z.boolean(),
  thresholdReached: z.number().int().nullable(),
  currentWindowFrom: z.string(),
  currentWindowTo: z.string()
})
export type BudgetWithProgress = z.infer<typeof BudgetWithProgressSchema>

export const CreateBudgetCommandSchema = z.object({
  name: z.string().min(1, 'Название обязательно').max(100, 'Не более 100 символов'),
  period: BudgetPeriodSchema,
  currency: CurrencySchema,
  limit: z.number().positive('Лимит должен быть больше 0'),
  startDate: z.string(),
  categoryId: z.string().uuid().nullable().optional(),
  rollover: z.boolean().optional()
})
export type CreateBudgetCommand = z.infer<typeof CreateBudgetCommandSchema>

export const UpdateBudgetCommandSchema = z.object({
  name: z.string().min(1).max(100).optional(),
  limit: z.number().positive().optional()
})
export type UpdateBudgetCommand = z.infer<typeof UpdateBudgetCommandSchema>
