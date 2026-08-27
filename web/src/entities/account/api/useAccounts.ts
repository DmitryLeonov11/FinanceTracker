import { useQuery } from '@tanstack/vue-query'
import { http } from '@/shared/api/http'
import { z } from 'zod'
import { AccountSchema } from '../model/schemas'

const AccountListSchema = z.array(AccountSchema)

export function useAccounts() {
  return useQuery({
    queryKey: ['accounts'] as const,
    queryFn: async () => {
      const { data } = await http.get('/accounts')
      return AccountListSchema.parse(data)
    },
    staleTime: 30_000
  })
}
