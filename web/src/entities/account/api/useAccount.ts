import { useQuery } from '@tanstack/vue-query'
import { computed, type MaybeRefOrGetter, toValue } from 'vue'
import { accountsApi } from './accountsApi'

export function useAccount(id: MaybeRefOrGetter<string | undefined>) {
  const queryKey = computed(() => ['accounts', toValue(id)] as const)
  return useQuery({
    queryKey,
    queryFn: () => accountsApi.getById(toValue(id)!),
    enabled: computed(() => !!toValue(id)),
    staleTime: 30_000
  })
}
