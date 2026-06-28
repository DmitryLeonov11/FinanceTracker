<script setup lang="ts">
import { ref, computed } from 'vue'
import { toast } from 'vue-sonner'
import {
  AlertDialogRoot,
  AlertDialogPortal,
  AlertDialogOverlay,
  AlertDialogContent,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogCancel
} from 'reka-ui'

import { Button } from '@/shared/ui/primitives'
import BudgetsList from '@/widgets/BudgetsList/BudgetsList.vue'
import CreateBudgetDialog from '@/features/budget/create-budget/CreateBudgetDialog.vue'
import { useBudgets } from '@/entities/budget/api/useBudgets'
import { useCloseBudget } from '@/entities/budget/api/useCloseBudget'
import { useUiStore } from '@/shared/stores/ui'
import { useRealtimeInvalidation } from '@/shared/composables/useRealtimeInvalidation'
import { cn } from '@/shared/lib/cn'
import { ApiError } from '@/shared/api/errors'
import type { BudgetWithProgress } from '@/entities/budget/model/schemas'

useRealtimeInvalidation()

const ui = useUiStore()

const tab = ref<'active' | 'closed'>('active')
const includeClosed = computed(() => tab.value === 'closed')
const { data, isPending } = useBudgets(includeClosed)

const filtered = computed(() => {
  const all = data.value ?? []
  return all.filter((b) => (tab.value === 'closed' ? b.isClosed : !b.isClosed))
})

const counts = computed(() => {
  const list = data.value ?? []
  return {
    active: list.filter((b) => !b.isClosed).length,
    closed: list.filter((b) => b.isClosed).length
  }
})

const closing = ref<BudgetWithProgress | null>(null)
const { mutateAsync: closeBudget, isPending: isClosing } = useCloseBudget()

async function confirmClose() {
  const target = closing.value
  if (!target) return
  try {
    await closeBudget(target.id)
    toast.success(`Бюджет «${target.name}» закрыт`)
    closing.value = null
  } catch (err) {
    if (err instanceof ApiError) toast.error(err.message)
    else toast.error('Не удалось закрыть бюджет')
  }
}
</script>

<template>
  <div class="space-y-5">
    <header class="flex items-center justify-between gap-3 flex-wrap">
      <div>
        <h1 class="text-[22px] font-semibold tracking-[-0.01em]">Бюджеты</h1>
        <p class="text-[13px] text-fg-muted mt-0.5">
          <span>{{ counts.active }} активных · {{ counts.closed }} закрытых</span>
        </p>
      </div>
      <Button icon-left="plus" @click="ui.openCreateBudget">Новый бюджет</Button>
    </header>

    <div class="flex items-center gap-1 bg-surface border border-border rounded-md p-1 w-fit">
      <button
        type="button"
        :class="
          cn(
            'h-7 px-3 rounded-[6px] text-[12px] font-medium transition-colors',
            tab === 'active' ? 'bg-surface-sunk text-fg shadow-xs' : 'text-fg-muted hover:text-fg'
          )
        "
        @click="tab = 'active'"
      >
        Активные
      </button>
      <button
        type="button"
        :class="
          cn(
            'h-7 px-3 rounded-[6px] text-[12px] font-medium transition-colors',
            tab === 'closed' ? 'bg-surface-sunk text-fg shadow-xs' : 'text-fg-muted hover:text-fg'
          )
        "
        @click="tab = 'closed'"
      >
        Закрытые
      </button>
    </div>

    <BudgetsList
      :budgets="filtered"
      :is-loading="isPending"
      :on-create="ui.openCreateBudget"
      @close="(b) => (closing = b)"
    />

    <CreateBudgetDialog />

    <AlertDialogRoot :open="!!closing" @update:open="(v) => !v && (closing = null)">
      <AlertDialogPortal>
        <AlertDialogOverlay class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out data-[state=open]:fade-in" />
        <AlertDialogContent
          class="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-[min(420px,calc(100vw-32px))] bg-surface border border-border rounded-xl shadow-xl p-6 focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:fade-in data-[state=open]:zoom-in-95 data-[state=closed]:zoom-out-95 data-[state=closed]:fade-out"
        >
          <AlertDialogTitle class="text-[16px] font-semibold tracking-[-0.005em] mb-1">
            Закрыть бюджет?
          </AlertDialogTitle>
          <AlertDialogDescription class="text-[13px] text-fg-muted">
            «{{ closing?.name }}» больше не будет учитываться в активных. История останется, бюджет можно посмотреть на вкладке «Закрытые».
          </AlertDialogDescription>
          <div class="flex items-center justify-end gap-2 mt-5">
            <AlertDialogCancel as-child>
              <Button intent="ghost" type="button">Отмена</Button>
            </AlertDialogCancel>
            <Button intent="danger" :loading="isClosing" @click="confirmClose">Закрыть</Button>
          </div>
        </AlertDialogContent>
      </AlertDialogPortal>
    </AlertDialogRoot>
  </div>
</template>
