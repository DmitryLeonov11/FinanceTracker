<script setup lang="ts">
import { computed } from 'vue'
import { Card, Skeleton, Button } from '@/shared/ui/primitives'
import TransactionRow from '@/entities/transaction/ui/TransactionRow.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import { fmtDate } from '@/shared/lib/format/date'
import type { Transaction } from '@/entities/transaction/model/schemas'
import type { Account } from '@/entities/account/model/schemas'

interface Props {
  transactions: Transaction[]
  accounts: Account[] | undefined
  isLoading: boolean
  isFetching: boolean
  total: number
  hasMore: boolean
  emptyTitle?: string
  emptyHint?: string
}

const props = defineProps<Props>()
const emit = defineEmits<{ loadMore: []; create: [] }>()

const accountById = computed(() => {
  const map = new Map<string, Account>()
  for (const a of props.accounts ?? []) map.set(a.id, a)
  return map
})

interface DayGroup {
  date: string
  label: string
  items: Transaction[]
}

const groupedByDay = computed<DayGroup[]>(() => {
  const groups = new Map<string, DayGroup>()
  for (const tx of props.transactions) {
    const dayKey = tx.occurredAt.slice(0, 10)
    let g = groups.get(dayKey)
    if (!g) {
      g = { date: dayKey, label: fmtDate(tx.occurredAt), items: [] }
      groups.set(dayKey, g)
    }
    g.items.push(tx)
  }
  return [...groups.values()].sort((a, b) => (a.date < b.date ? 1 : -1))
})
</script>

<template>
  <div>
    <!-- Loading skeleton (first load) -->
    <Card v-if="isLoading && transactions.length === 0" padding="none" class="overflow-hidden">
      <div class="px-4 py-3 border-b border-border">
        <Skeleton width="120" height="14" />
      </div>
      <div v-for="n in 5" :key="n" class="flex items-center gap-3 px-4 h-14 border-b border-border last:border-b-0">
        <Skeleton width="36" height="36" rounded="md" />
        <div class="flex-1 space-y-1.5">
          <Skeleton width="60%" height="14" />
          <Skeleton width="40%" height="11" />
        </div>
        <Skeleton width="80" height="16" />
      </div>
    </Card>

    <!-- Empty state -->
    <Card
      v-else-if="transactions.length === 0"
      variant="outlined"
      padding="lg"
      class="text-center border-dashed"
    >
      <div class="mx-auto size-12 rounded-full bg-surface-hi flex items-center justify-center mb-3">
        <Icon name="swap" :size="20" class="text-fg-subtle" />
      </div>
      <p class="font-medium text-fg">{{ emptyTitle ?? 'Операций пока нет' }}</p>
      <p class="text-[13px] text-fg-muted mt-1 mb-4">
        {{ emptyHint ?? 'Добавьте первую операцию, чтобы начать вести учёт.' }}
      </p>
      <Button icon-left="plus" @click="emit('create')">Новая операция</Button>
    </Card>

    <!-- Grouped list -->
    <div v-else class="space-y-4">
      <Card
        v-for="group in groupedByDay"
        :key="group.date"
        padding="none"
        class="overflow-hidden"
      >
        <div
          class="sticky top-14 z-10 px-4 h-10 flex items-center justify-between bg-surface-hi/95 backdrop-blur border-b border-border"
        >
          <span class="text-[12px] font-semibold uppercase tracking-wider text-fg-muted">
            {{ group.label }}
          </span>
          <span class="text-[11px] text-fg-subtle">
            {{ group.items.length }} {{ group.items.length === 1 ? 'операция' : group.items.length < 5 ? 'операции' : 'операций' }}
          </span>
        </div>
        <div class="divide-y divide-border">
          <TransactionRow
            v-for="tx in group.items"
            :key="tx.id"
            :transaction="tx"
            :account-name="accountById.get(tx.accountId)?.name"
          />
        </div>
      </Card>

      <div v-if="hasMore" class="flex justify-center pt-2">
        <Button intent="secondary" :loading="isFetching" @click="emit('loadMore')">
          Загрузить ещё
        </Button>
      </div>
      <p v-else class="text-center text-[12px] text-fg-subtle pt-2">
        Показано {{ transactions.length }} из {{ total }}
      </p>
    </div>
  </div>
</template>
