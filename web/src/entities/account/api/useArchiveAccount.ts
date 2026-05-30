import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { accountsApi } from './accountsApi'

export function useArchiveAccount() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => accountsApi.archive(id),
    onSuccess: (_, id) => {
      qc.invalidateQueries({ queryKey: ['accounts'] })
      qc.invalidateQueries({ queryKey: ['accounts', id] })
      qc.invalidateQueries({ queryKey: ['dashboard'] })
    }
  })
}
