import { useQuery } from '@tanstack/vue-query'
import { computed, type MaybeRefOrGetter, toValue } from 'vue'
import { accountsApi } from './accountsApi'

export function useAccountBalanceHistory(
  id: MaybeRefOrGetter<string | undefined>,
  days: MaybeRefOrGetter<number> = 30
) {
  const queryKey = computed(() => ['accounts', toValue(id), 'balance-history', toValue(days)] as const)
  return useQuery({
    queryKey,
    queryFn: () => accountsApi.getBalanceHistory(toValue(id)!, toValue(days)),
    enabled: computed(() => !!toValue(id)),
    staleTime: 60_000
  })
}
