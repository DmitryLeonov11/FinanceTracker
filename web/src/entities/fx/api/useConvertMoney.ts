import { useQuery, keepPreviousData } from '@tanstack/vue-query'
import { computed, type Ref } from 'vue'
import { fxApi } from './fxApi'
import type { CurrencyCode } from '@/shared/config/currencies'

interface UseConvertMoneyParams {
  amount: Ref<number>
  from: Ref<CurrencyCode | null | undefined>
  to: Ref<CurrencyCode | null | undefined>
  enabled?: Ref<boolean>
}

export function useConvertMoney({ amount, from, to, enabled }: UseConvertMoneyParams) {
  const isEnabled = computed(
    () =>
      (enabled?.value ?? true) &&
      !!from.value &&
      !!to.value &&
      from.value !== to.value &&
      amount.value > 0 &&
      Number.isFinite(amount.value)
  )

  const queryKey = computed(() => ['fx', 'convert', amount.value, from.value, to.value] as const)

  return useQuery({
    queryKey,
    queryFn: () => fxApi.convert(amount.value, from.value!, to.value!),
    enabled: isEnabled,
    placeholderData: keepPreviousData,
    staleTime: 60_000
  })
}
