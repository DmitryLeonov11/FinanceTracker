<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '@/shared/lib/cn'
import { Input } from '@/shared/ui/primitives'
import { useAccounts } from '@/entities/account/api/useAccounts'
import type { TransactionType } from '@/entities/transaction/model/schemas'
import { PERIOD_LABELS, PERIOD_ORDER, type PeriodPreset } from './periods'

interface Props {
  period: PeriodPreset
  accountIds: string[]
  types: TransactionType[]
  search: string
}

const props = defineProps<Props>()
const emit = defineEmits<{
  'update:period': [v: PeriodPreset]
  'update:accountIds': [v: string[]]
  'update:types': [v: TransactionType[]]
  'update:search': [v: string]
}>()

const { data: accountsData } = useAccounts()

const allAccountIds = computed(() => accountsData.value?.map((a) => a.id) ?? [])
const selectedAccountSet = computed(() => new Set(props.accountIds))
const accountsAllSelected = computed(
  () => props.accountIds.length === 0 || props.accountIds.length === allAccountIds.value.length
)

function toggleAccount(id: string) {
  const set = new Set(props.accountIds)
  if (set.has(id)) set.delete(id)
  else set.add(id)
  emit('update:accountIds', [...set])
}

function resetAccounts() {
  emit('update:accountIds', [])
}

const TYPE_LABELS: Record<TransactionType, string> = {
  Income: 'Доход',
  Expense: 'Расход',
  Transfer: 'Перевод'
}

function toggleType(t: TransactionType) {
  const set = new Set(props.types)
  if (set.has(t)) set.delete(t)
  else set.add(t)
  emit('update:types', [...set])
}
</script>

<template>
  <div class="space-y-3">
    <!-- Period chips -->
    <div class="flex flex-wrap items-center gap-2">
      <div class="flex items-center gap-1 bg-surface border border-border rounded-md p-1">
        <button
          v-for="p in PERIOD_ORDER"
          :key="p"
          type="button"
          :class="
            cn(
              'h-7 px-2.5 rounded-[6px] text-[12px] font-medium transition-colors',
              period === p
                ? 'bg-surface-sunk text-fg shadow-xs'
                : 'text-fg-muted hover:text-fg'
            )
          "
          @click="emit('update:period', p)"
        >
          {{ PERIOD_LABELS[p] }}
        </button>
      </div>

      <!-- Type chips -->
      <div class="flex items-center gap-1">
        <button
          v-for="t in (['Income', 'Expense'] as const)"
          :key="t"
          type="button"
          :class="
            cn(
              'h-7 px-2.5 rounded-md border text-[12px] font-medium transition-colors',
              types.includes(t)
                ? 'border-accent bg-accent-soft text-accent-soft-fg'
                : 'border-border text-fg-muted hover:text-fg hover:border-border-strong'
            )
          "
          @click="toggleType(t)"
        >
          {{ TYPE_LABELS[t] }}
        </button>
      </div>

      <div class="ml-auto w-full md:w-64">
        <Input
          :model-value="search"
          icon-left="search"
          placeholder="Поиск по комментарию"
          size="sm"
          @update:model-value="(v) => emit('update:search', v)"
        />
      </div>
    </div>

    <!-- Account chips -->
    <div v-if="accountsData && accountsData.length > 1" class="flex flex-wrap items-center gap-1.5">
      <button
        type="button"
        :class="
          cn(
            'h-7 px-2.5 rounded-pill border text-[12px] font-medium transition-colors',
            accountsAllSelected
              ? 'border-fg-subtle bg-fg text-fg-inverse'
              : 'border-border text-fg-muted hover:text-fg'
          )
        "
        @click="resetAccounts"
      >
        Все счета
      </button>
      <button
        v-for="acc in accountsData"
        :key="acc.id"
        type="button"
        :class="
          cn(
            'h-7 px-2.5 rounded-pill border text-[12px] transition-colors flex items-center gap-1.5',
            selectedAccountSet.has(acc.id)
              ? 'border-accent bg-accent-soft text-accent-soft-fg'
              : 'border-border text-fg-muted hover:text-fg hover:border-border-strong'
          )
        "
        @click="toggleAccount(acc.id)"
      >
        {{ acc.name }}
        <span class="text-[10px] text-fg-subtle uppercase tracking-wider">{{ acc.currency }}</span>
      </button>
    </div>
  </div>
</template>
