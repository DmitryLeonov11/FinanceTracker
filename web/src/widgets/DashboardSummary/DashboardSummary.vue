<script setup lang="ts">
import { computed } from 'vue'
import { Card, Skeleton } from '@/shared/ui/primitives'
import Money from '@/entities/money/ui/Money.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import { useDashboardBalance } from '@/entities/dashboard/api/useDashboardBalance'
import { CURRENCY_LABELS, type CurrencyCode } from '@/shared/config/currencies'
import { useFlash } from '@/shared/composables/useFlash'
import { cn } from '@/shared/lib/cn'

const { data, isPending, isError, refetch } = useDashboardBalance()

const accountsCount = computed(() => data.value?.accounts.length ?? 0)

const primaryCurrency = computed<CurrencyCode>(() => data.value?.displayCurrency ?? 'BYN')

const primaryTotal = computed(() => {
  const all = data.value?.balancesByCurrency ?? []
  return all.find((b) => b.currency === primaryCurrency.value)?.total ?? 0
})

const otherCurrencies = computed(
  () => (data.value?.balancesByCurrency ?? []).filter((b) => b.currency !== primaryCurrency.value)
)

const flash = useFlash(primaryTotal)
</script>

<template>
  <section>
    <Card
      variant="elevated"
      padding="lg"
      :class="
        cn(
          'overflow-hidden transition-shadow',
          flash === 'up' && 'flash-up',
          flash === 'down' && 'flash-down'
        )
      "
    >
      <div v-if="isPending" class="space-y-3">
        <Skeleton width="220" height="18" />
        <Skeleton width="320" height="56" />
        <Skeleton width="160" height="14" />
      </div>

      <div v-else-if="isError" class="flex items-center justify-between">
        <div>
          <p class="text-[13px] text-fg-muted">Не удалось загрузить сводку</p>
          <button class="text-accent hover:text-accent-hover text-[13px] mt-1" @click="refetch()">
            Повторить
          </button>
        </div>
      </div>

      <div v-else>
        <div class="flex items-start justify-between gap-4">
          <div>
            <p class="text-[13px] text-fg-muted uppercase tracking-wider font-medium">
              Чистая стоимость
            </p>
            <div class="mt-2">
              <Money
                :amount="primaryTotal"
                :currency="primaryCurrency"
                size="2xl"
                animate
                :duration="600"
              />
            </div>
            <p class="mt-2 text-[13px] text-fg-subtle">
              {{ CURRENCY_LABELS[primaryCurrency] }} ·
              <span v-if="accountsCount === 0">нет счетов</span>
              <span v-else>
                на {{ accountsCount }}
                {{ accountsCount === 1 ? 'счёте' : accountsCount < 5 ? 'счетах' : 'счетах' }}
              </span>
            </p>
          </div>

          <div
            class="hidden sm:flex shrink-0 size-12 rounded-lg bg-accent-soft text-accent-soft-fg items-center justify-center"
          >
            <Icon name="trendingUp" :size="22" />
          </div>
        </div>

        <div v-if="otherCurrencies.length" class="mt-6 pt-5 border-t border-border">
          <p class="text-[12px] uppercase tracking-wider font-medium text-fg-subtle mb-3">
            В других валютах
          </p>
          <div class="flex flex-wrap gap-x-8 gap-y-3">
            <div v-for="b in otherCurrencies" :key="b.currency" class="flex flex-col">
              <span class="text-[11px] text-fg-subtle uppercase tracking-wider">{{ b.currency }}</span>
              <Money
                :amount="b.total"
                :currency="b.currency"
                size="lg"
                :fade-decimals="false"
                animate
              />
            </div>
          </div>
        </div>
      </div>
    </Card>
  </section>
</template>
