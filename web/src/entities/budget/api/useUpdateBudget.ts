import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { budgetsApi } from './budgetsApi'
import type { UpdateBudgetCommand } from '../model/schemas'

export function useUpdateBudget() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, cmd }: { id: string; cmd: UpdateBudgetCommand }) => budgetsApi.update(id, cmd),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['budgets'] })
    }
  })
}
