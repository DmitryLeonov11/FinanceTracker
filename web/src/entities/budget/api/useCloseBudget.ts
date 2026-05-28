import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { budgetsApi } from './budgetsApi'

export function useCloseBudget() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => budgetsApi.close(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['budgets'] })
    }
  })
}
