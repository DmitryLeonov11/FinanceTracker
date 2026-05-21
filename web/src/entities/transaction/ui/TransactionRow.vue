<script setup lang="ts">
import { computed } from 'vue'
import {
  DropdownMenuRoot,
  DropdownMenuTrigger,
  DropdownMenuPortal,
  DropdownMenuContent,
  DropdownMenuItem
} from 'reka-ui'
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
const emit = defineEmits<{ edit: [tx: Transaction]; delete: [tx: Transaction] }>()

const signedAmount = computed(() =>
  props.transaction.type === 'Income' ? props.transaction.amount : -props.transaction.amount
)

const moneyTone = computed(() =>
  props.transaction.type === 'Income'
    ? 'text-success'
    : props.transaction.type === 'Expense'
      ? 'text-fg'
      : 'text-fg-muted'
)

const isTransfer = computed(() => props.transaction.type === 'Transfer')
const menuDisabled = computed(() => props.pending)
</script>

<template>
  <div
    :class="
      cn(
        'group/row flex items-center gap-3 px-3 md:px-4 h-14 rounded-md',
        'hover:bg-surface-hi transition-colors duration-[120ms]',
        pending && 'opacity-70'
      )
    "
    :title="pending ? 'Ожидает отправки на сервер' : undefined"
  >
    <TransactionTypeBadge :type="transaction.type" />

    <div class="min-w-0 flex-1">
      <div class="flex items-baseline gap-2 min-w-0">
        <p class="font-medium text-fg truncate">
          {{
            transaction.note ||
            (transaction.type === 'Income'
              ? 'Доход'
              : transaction.type === 'Expense'
                ? 'Расход'
                : 'Перевод')
          }}
        </p>
      </div>
      <p class="text-[12px] text-fg-subtle mt-0.5 flex items-center gap-1.5 truncate">
        <Icon v-if="pending" name="loader" :size="11" class="text-warning animate-spin shrink-0" />
        <span class="truncate">{{ accountName ?? '—' }} · {{ fmtTime(transaction.occurredAt) }}</span>
        <span v-if="pending" class="text-warning text-[11px] shrink-0">· ожидает</span>
      </p>
    </div>

    <div :class="cn('text-right shrink-0', moneyTone)">
      <Money
        :amount="signedAmount"
        :currency="transaction.currency"
        size="md"
        :show-sign="transaction.type === 'Income'"
      />
    </div>

    <DropdownMenuRoot v-if="!menuDisabled">
      <DropdownMenuTrigger
        as="button"
        class="size-7 inline-flex items-center justify-center rounded-md text-fg-subtle hover:text-fg hover:bg-surface-hi opacity-0 group-hover/row:opacity-100 data-[state=open]:opacity-100 transition-opacity"
        :aria-label="'Действия с операцией'"
        @click.stop
      >
        <Icon name="moreHorizontal" :size="16" />
      </DropdownMenuTrigger>
      <DropdownMenuPortal>
        <DropdownMenuContent
          side="bottom"
          align="end"
          :side-offset="6"
          class="z-50 min-w-[180px] rounded-md border border-border bg-surface shadow-md p-1 focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:fade-in data-[state=open]:zoom-in-95 data-[state=closed]:fade-out data-[state=closed]:zoom-out-95"
        >
          <DropdownMenuItem
            :disabled="isTransfer"
            :class="
              cn(
                'flex items-center gap-2 h-8 px-2 rounded text-[13px] cursor-pointer outline-none',
                'data-[highlighted]:bg-surface-hi data-[highlighted]:text-fg',
                isTransfer && 'opacity-50 cursor-not-allowed'
              )
            "
            @select="!isTransfer && emit('edit', transaction)"
          >
            <Icon name="pencil" :size="14" class="text-fg-subtle" />
            <span>Изменить</span>
            <span v-if="isTransfer" class="ml-auto text-[10.5px] text-fg-subtle">недоступно</span>
          </DropdownMenuItem>
          <DropdownMenuItem
            :disabled="isTransfer"
            :class="
              cn(
                'flex items-center gap-2 h-8 px-2 rounded text-[13px] cursor-pointer outline-none',
                'data-[highlighted]:bg-danger-soft data-[highlighted]:text-danger',
                isTransfer ? 'opacity-50 cursor-not-allowed' : 'text-danger'
              )
            "
            @select="!isTransfer && emit('delete', transaction)"
          >
            <Icon name="trash" :size="14" />
            <span>Удалить</span>
            <span v-if="isTransfer" class="ml-auto text-[10.5px] text-fg-subtle">скоро</span>
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenuPortal>
    </DropdownMenuRoot>
  </div>
</template>
