import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { transactionsApi } from './transactionsApi'
import type { EditTransactionCommand } from '../model/schemas'

export function useEditTransaction() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, cmd }: { id: string; cmd: EditTransactionCommand }) =>
      transactionsApi.update(id, cmd),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['transactions'] })
      qc.invalidateQueries({ queryKey: ['dashboard'] })
      qc.invalidateQueries({ queryKey: ['accounts'] })
    }
  })
}
