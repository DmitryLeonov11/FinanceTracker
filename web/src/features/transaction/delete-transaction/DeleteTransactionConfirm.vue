<script setup lang="ts">
import { computed } from 'vue'
import { toast } from 'vue-sonner'
import {
  AlertDialogRoot,
  AlertDialogPortal,
  AlertDialogOverlay,
  AlertDialogContent,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogCancel,
  AlertDialogAction
} from 'reka-ui'

import { Button } from '@/shared/ui/primitives'
import Icon from '@/shared/ui/icons/Icon.vue'
import Money from '@/entities/money/ui/Money.vue'
import { useDeleteTransaction } from '@/entities/transaction/api/useDeleteTransaction'
import { useUiStore } from '@/shared/stores/ui'
import { fmtDate, fmtTime } from '@/shared/lib/format/date'
import { ApiError } from '@/shared/api/errors'

const ui = useUiStore()
const { mutateAsync, isPending } = useDeleteTransaction()

const tx = computed(() => ui.deletingTransaction)
const open = computed(() => tx.value !== null)

const signedAmount = computed(() => {
  if (!tx.value) return 0
  return tx.value.type === 'Income' ? tx.value.amount : -tx.value.amount
})

async function onConfirm() {
  if (!tx.value) return
  try {
    await mutateAsync(tx.value.id)
    toast.success('Операция удалена')
    ui.dismissDeleteTransaction()
  } catch (err) {
    if (err instanceof ApiError) {
      toast.error(err.message)
    } else {
      toast.error('Не удалось удалить операцию')
    }
  }
}
</script>

<template>
  <AlertDialogRoot :open="open" @update:open="(v) => !v && ui.dismissDeleteTransaction()">
    <AlertDialogPortal>
      <AlertDialogOverlay
        class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out data-[state=open]:fade-in"
      />
      <AlertDialogContent
        class="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-[min(440px,calc(100vw-32px))] bg-surface border border-border rounded-xl shadow-xl p-6 focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:fade-in data-[state=open]:zoom-in-95 data-[state=closed]:zoom-out-95 data-[state=closed]:fade-out"
      >
        <div class="flex items-start gap-3 mb-4">
          <div class="shrink-0 size-10 rounded-full bg-danger-soft text-danger flex items-center justify-center">
            <Icon name="trash" :size="18" />
          </div>
          <div class="flex-1 min-w-0">
            <AlertDialogTitle class="text-[16px] font-semibold tracking-[-0.005em]">
              Удалить операцию?
            </AlertDialogTitle>
            <AlertDialogDescription class="text-[13px] text-fg-muted mt-1">
              Баланс счёта будет скорректирован. Действие необратимо.
            </AlertDialogDescription>
          </div>
        </div>

        <div v-if="tx" class="bg-surface-sunk border border-border rounded-md p-3 mb-5 space-y-1">
          <div class="flex items-center justify-between gap-3">
            <span class="text-[12px] text-fg-subtle uppercase tracking-wider">
              {{ tx.type === 'Income' ? 'Доход' : tx.type === 'Expense' ? 'Расход' : 'Перевод' }}
            </span>
            <Money
              :amount="signedAmount"
              :currency="tx.currency"
              size="lg"
              :show-sign="tx.type === 'Income'"
            />
          </div>
          <p class="text-[12px] text-fg-subtle">
            {{ fmtDate(tx.occurredAt) }} · {{ fmtTime(tx.occurredAt) }}
          </p>
          <p v-if="tx.note" class="text-[13px] text-fg truncate">{{ tx.note }}</p>
        </div>

        <div class="flex items-center justify-end gap-2">
          <AlertDialogCancel as-child>
            <Button intent="ghost" type="button" :disabled="isPending">Отмена</Button>
          </AlertDialogCancel>
          <AlertDialogAction as-child>
            <Button intent="danger" :loading="isPending" icon-left="trash" @click="onConfirm">
              Удалить
            </Button>
          </AlertDialogAction>
        </div>
      </AlertDialogContent>
    </AlertDialogPortal>
  </AlertDialogRoot>
</template>
