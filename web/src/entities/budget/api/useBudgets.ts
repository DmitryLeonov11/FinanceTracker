import { useQuery } from '@tanstack/vue-query'
import { computed, type Ref } from 'vue'
import { budgetsApi } from './budgetsApi'

export function useBudgets(includeClosed: Ref<boolean>) {
  const queryKey = computed(() => ['budgets', { includeClosed: includeClosed.value }] as const)
  return useQuery({
    queryKey,
    queryFn: () => budgetsApi.list(includeClosed.value),
    staleTime: 30_000
  })
}
