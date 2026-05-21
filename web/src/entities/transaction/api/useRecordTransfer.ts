import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { transactionsApi } from './transactionsApi'
import type { RecordTransferCommand } from '../model/schemas'

export function useRecordTransfer() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (cmd: RecordTransferCommand) => transactionsApi.transfer(cmd),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['transactions'] })
      qc.invalidateQueries({ queryKey: ['dashboard'] })
      qc.invalidateQueries({ queryKey: ['accounts'] })
    }
  })
}
