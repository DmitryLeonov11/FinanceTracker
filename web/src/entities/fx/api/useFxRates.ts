import { useQuery } from '@tanstack/vue-query'
import { fxApi } from './fxApi'

export function useFxRates() {
  return useQuery({
    queryKey: ['fx', 'rates'] as const,
    queryFn: () => fxApi.list(),
    staleTime: 5 * 60 * 1000
  })
}
