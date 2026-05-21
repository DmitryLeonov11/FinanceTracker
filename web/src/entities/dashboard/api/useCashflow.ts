import { useQuery, keepPreviousData } from '@tanstack/vue-query'
import { computed, type Ref } from 'vue'
import { http } from '@/shared/api/http'
import { CashflowSchema } from '../model/cashflowSchemas'
import type { CurrencyCode } from '@/shared/config/currencies'

interface UseCashflowParams {
  days: Ref<number>
  currency?: Ref<CurrencyCode | undefined>
}

export function useCashflow({ days, currency }: UseCashflowParams) {
  const queryKey = computed(() => ['dashboard', 'cashflow', { days: days.value, currency: currency?.value }] as const)

  return useQuery({
    queryKey,
    queryFn: async () => {
      const params = new URLSearchParams()
      params.set('days', String(days.value))
      if (currency?.value) params.set('currency', currency.value)
      const { data } = await http.get(`/dashboard/cashflow?${params.toString()}`)
      return CashflowSchema.parse(data)
    },
    placeholderData: keepPreviousData,
    staleTime: 60_000
  })
}
