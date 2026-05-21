<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '@/shared/lib/cn'
import { fmtTime } from '@/shared/lib/format/date'
import Money from '@/entities/money/ui/Money.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import TransactionTypeBadge from './TransactionTypeBadge.vue'
import type { Transaction } from '../model/schemas'

interface Props {
  transaction: Transaction
  accountName?: string
  pending?: boolean
}

const props = withDefaults(defineProps<Props>(), { pending: false })

const signedAmount = computed(() =>
  props.transaction.type === 'Income' ? props.transaction.amount : -props.transaction.amount
)

const moneyTone = computed(() =>
  props.transaction.type === 'Income' ? 'text-success' : props.transaction.type === 'Expense' ? 'text-fg' : 'text-fg-muted'
)
</script>

<template>
  <div
    :class="
      cn(
        'flex items-center gap-3 px-3 md:px-4 h-14 rounded-md',
        'hover:bg-surface-hi transition-colors duration-[120ms] cursor-pointer',
        pending && 'opacity-70'
      )
    "
    :title="pending ? 'Ожидает отправки на сервер' : undefined"
  >
    <TransactionTypeBadge :type="transaction.type" />

    <div class="min-w-0 flex-1">
      <div class="flex items-baseline gap-2 min-w-0">
        <p class="font-medium text-fg truncate">
          {{ transaction.note || (transaction.type === 'Income' ? 'Доход' : transaction.type === 'Expense' ? 'Расход' : 'Перевод') }}
        </p>
      </div>
      <p class="text-[12px] text-fg-subtle mt-0.5 flex items-center gap-1.5 truncate">
        <Icon v-if="pending" name="loader" :size="11" class="text-warning animate-spin shrink-0" />
        <span class="truncate">{{ accountName ?? '—' }} · {{ fmtTime(transaction.occurredAt) }}</span>
        <span v-if="pending" class="text-warning text-[11px] shrink-0">· ожидает</span>
      </p>
    </div>

    <div :class="cn('text-right shrink-0', moneyTone)">
      <Money :amount="signedAmount" :currency="transaction.currency" size="md" :show-sign="transaction.type === 'Income'" />
    </div>
  </div>
</template>
