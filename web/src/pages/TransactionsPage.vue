<script setup lang="ts">
import { ref, computed, watch } from 'vue'

import { Button } from '@/shared/ui/primitives'
import TransactionFiltersBar from '@/widgets/TransactionFiltersBar/TransactionFiltersBar.vue'
import TransactionsTable from '@/widgets/TransactionsTable/TransactionsTable.vue'
import RecordTransactionDialog from '@/features/transaction/record-transaction/RecordTransactionDialog.vue'

import { useTransactions } from '@/entities/transaction/api/useTransactions'
import { useAccounts } from '@/entities/account/api/useAccounts'
import { useRealtimeInvalidation } from '@/shared/composables/useRealtimeInvalidation'

import type { Transaction, TransactionFilters, TransactionType } from '@/entities/transaction/model/schemas'
import { rangeForPreset, type PeriodPreset } from '@/widgets/TransactionFiltersBar/periods'

useRealtimeInvalidation()

const PAGE_SIZE = 50

const period = ref<PeriodPreset>('month')
const accountIds = ref<string[]>([])
const types = ref<TransactionType[]>([])
const search = ref('')
const page = ref(1)

// reset page when any other filter changes
watch([period, accountIds, types, search], () => {
  page.value = 1
})

const debouncedSearch = ref('')
let searchTimer: ReturnType<typeof setTimeout> | null = null
watch(search, (v) => {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    debouncedSearch.value = v
  }, 250)
})

const filters = computed<TransactionFilters>(() => {
  const { from, to } = rangeForPreset(period.value)
  return {
    from,
    to,
    accountIds: accountIds.value.length ? accountIds.value : undefined,
    types: types.value.length ? types.value : undefined,
    search: debouncedSearch.value.trim() || undefined,
    page: page.value,
    pageSize: PAGE_SIZE
  }
})

const { data, isPending, isFetching } = useTransactions(filters)
const { data: accountsData } = useAccounts()

// Accumulate pages — TanStack returns the current page; we keep prior pages locally
const accumulated = ref<Transaction[]>([])
watch(
  [data, page],
  ([d, p]) => {
    if (!d) return
    if (p === 1) accumulated.value = [...d.items]
    else {
      const existing = new Set(accumulated.value.map((t) => t.id))
      const fresh = d.items.filter((t) => !existing.has(t.id))
      accumulated.value = [...accumulated.value, ...fresh]
    }
  },
  { immediate: true }
)

const recordOpen = ref(false)
</script>

<template>
  <div class="space-y-5">
    <header class="flex items-center justify-between gap-3 flex-wrap">
      <div>
        <h1 class="text-[22px] font-semibold tracking-[-0.01em]">Операции</h1>
        <p class="text-[13px] text-fg-muted mt-0.5">
          <span v-if="data">{{ data.total }} {{ data.total === 1 ? 'запись' : data.total < 5 ? 'записи' : 'записей' }} в выбранном периоде</span>
        </p>
      </div>
      <Button icon-left="plus" @click="recordOpen = true">Новая операция</Button>
    </header>

    <TransactionFiltersBar
      :period="period"
      :account-ids="accountIds"
      :types="types"
      :search="search"
      @update:period="(v) => (period = v)"
      @update:account-ids="(v) => (accountIds = v)"
      @update:types="(v) => (types = v)"
      @update:search="(v) => (search = v)"
    />

    <TransactionsTable
      :transactions="accumulated"
      :accounts="accountsData"
      :is-loading="isPending"
      :is-fetching="isFetching"
      :total="data?.total ?? 0"
      :has-more="data?.hasMore ?? false"
      @load-more="page = page + 1"
      @create="recordOpen = true"
    />

    <RecordTransactionDialog v-model:open="recordOpen" />
  </div>
</template>
