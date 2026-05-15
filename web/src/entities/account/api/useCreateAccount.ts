import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { accountsApi } from './accountsApi'
import type { CreateAccountCommand } from '../model/schemas'

export function useCreateAccount() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (cmd: CreateAccountCommand) => accountsApi.create(cmd),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['dashboard', 'balance'] })
      qc.invalidateQueries({ queryKey: ['accounts'] })
    }
  })
}
