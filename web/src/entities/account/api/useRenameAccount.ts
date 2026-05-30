import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { accountsApi } from './accountsApi'

export function useRenameAccount() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, name }: { id: string; name: string }) => accountsApi.rename(id, name),
    onSuccess: (_, vars) => {
      qc.invalidateQueries({ queryKey: ['accounts'] })
      qc.invalidateQueries({ queryKey: ['accounts', vars.id] })
      qc.invalidateQueries({ queryKey: ['dashboard'] })
      qc.invalidateQueries({ queryKey: ['transactions'] })
    }
  })
}
