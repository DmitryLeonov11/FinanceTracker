import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { budgetsApi } from './budgetsApi'
import type { CreateBudgetCommand } from '../model/schemas'

export function useCreateBudget() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (cmd: CreateBudgetCommand) => budgetsApi.create(cmd),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['budgets'] })
    }
  })
}
