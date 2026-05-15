import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { transactionsApi } from './transactionsApi'
import type { AddTransactionCommand } from '../model/schemas'

export function useAddTransaction() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (cmd: AddTransactionCommand) => transactionsApi.create(cmd),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['transactions'] })
      qc.invalidateQueries({ queryKey: ['dashboard'] })
      qc.invalidateQueries({ queryKey: ['accounts'] })
    }
  })
}
