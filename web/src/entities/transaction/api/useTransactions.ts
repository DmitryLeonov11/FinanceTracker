import { useQuery, keepPreviousData } from '@tanstack/vue-query'
import { computed, type Ref } from 'vue'
import { transactionsApi } from './transactionsApi'
import type { TransactionFilters } from '../model/schemas'

export function useTransactions(filters: Ref<TransactionFilters>) {
  const queryKey = computed(() => ['transactions', filters.value] as const)

  return useQuery({
    queryKey,
    queryFn: () => transactionsApi.list(filters.value),
    placeholderData: keepPreviousData,
    staleTime: 15_000
  })
}
