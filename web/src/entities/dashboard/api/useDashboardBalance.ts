import { useQuery } from '@tanstack/vue-query'
import { http } from '@/shared/api/http'
import { DashboardBalanceSchema } from '../model/schemas'

export function useDashboardBalance() {
  return useQuery({
    queryKey: ['dashboard', 'balance'] as const,
    queryFn: async () => {
      const { data } = await http.get('/dashboard/balance')
      return DashboardBalanceSchema.parse(data)
    },
    staleTime: 60_000
  })
}
