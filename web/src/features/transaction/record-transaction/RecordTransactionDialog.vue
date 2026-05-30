<script setup lang="ts">
import { reactive, watch, computed } from 'vue'
import { toast } from 'vue-sonner'
import {
  DialogRoot,
  DialogPortal,
  DialogOverlay,
  DialogContent,
  DialogTitle,
  DialogDescription,
  DialogClose
} from 'reka-ui'

import { Button, Input } from '@/shared/ui/primitives'
import Field from '@/shared/ui/form/Field.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import {
  AddTransactionCommandSchema,
  EditTransactionCommandSchema,
  type AddTransactionCommand,
  type EditTransactionCommand
} from '@/entities/transaction/model/schemas'
import { useAddTransaction } from '@/entities/transaction/api/useAddTransaction'
import { useEditTransaction } from '@/entities/transaction/api/useEditTransaction'
import { useAccounts } from '@/entities/account/api/useAccounts'
import { useUiStore } from '@/shared/stores/ui'
import { ApiError } from '@/shared/api/errors'
import { cn } from '@/shared/lib/cn'
import { t } from '@/shared/lib/i18n'

const ui = useUiStore()
const { data: accountsData } = useAccounts()

interface FormState {
  accountId: string
  type: 'Income' | 'Expense'
  amount: string
  occurredAt: string  // local datetime-input format
  note: string
}

const editing = computed(() => ui.editingTransaction)
const isEditMode = computed(() => editing.value !== null)

const open = computed(() => ui.recordTransactionOpen || isEditMode.value)

function defaultForm(): FormState {
  const now = new Date()
  const local = new Date(now.getTime() - now.getTimezoneOffset() * 60_000)
  return {
    accountId: '',
    type: 'Expense',
    amount: '',
    occurredAt: local.toISOString().slice(0, 16),
    note: ''
  }
}

function formFromTransaction(tx: NonNullable<typeof editing.value>): FormState {
  const dt = new Date(tx.occurredAt)
  const local = new Date(dt.getTime() - dt.getTimezoneOffset() * 60_000)
  return {
    accountId: tx.accountId,
    type: tx.type === 'Income' ? 'Income' : 'Expense',
    amount: String(tx.amount).replace('.', ','),
    occurredAt: local.toISOString().slice(0, 16),
    note: tx.note ?? ''
  }
}

const form = reactive<FormState>(defaultForm())
const errors = reactive<Record<string, string>>({})

const { mutateAsync: addMutate, isPending: isAdding } = useAddTransaction()
const { mutateAsync: editMutate, isPending: isEditingPending } = useEditTransaction()
const isPending = computed(() => isAdding.value || isEditingPending.value)

const selectedAccount = computed(() =>
  accountsData.value?.find((a) => a.id === form.accountId)
)

function clearErrors() {
  Object.keys(errors).forEach((k) => delete errors[k])
}

watch(
  open,
  (isOpen) => {
    if (!isOpen) return
    clearErrors()
    if (editing.value) {
      Object.assign(form, formFromTransaction(editing.value))
    } else {
      Object.assign(form, defaultForm())
      const prefillId = ui.recordTransactionPrefillAccountId
      const hasPrefillAccount = prefillId && accountsData.value?.some((a) => a.id === prefillId)
      if (hasPrefillAccount) form.accountId = prefillId!
      else if (accountsData.value?.length) form.accountId = accountsData.value[0]!.id
    }
  },
  { immediate: true }
)

watch(
  accountsData,
  (list) => {
    if (open.value && !isEditMode.value && !form.accountId && list?.length) {
      form.accountId = list[0]!.id
    }
  }
)

function closeDialog() {
  if (isEditMode.value) ui.closeEditTransaction()
  else ui.closeRecordTransaction()
}

async function onSubmit(e: Event) {
  e.preventDefault()
  clearErrors()

  const amount = Number(form.amount.replace(',', '.'))
  const occurredIso = new Date(form.occurredAt).toISOString()

  if (isEditMode.value && editing.value) {
    const cmd: EditTransactionCommand = {
      amount: Number.isFinite(amount) ? amount : 0,
      occurredAt: occurredIso,
      categoryId: null,
      note: form.note.trim() || null
    }
    const parsed = EditTransactionCommandSchema.safeParse(cmd)
    if (!parsed.success) {
      for (const issue of parsed.error.issues) {
        const k = String(issue.path[0])
        if (!errors[k]) errors[k] = issue.message
      }
      return
    }

    try {
      await editMutate({ id: editing.value.id, cmd: parsed.data })
      toast.success('Операция изменена')
      ui.closeEditTransaction()
    } catch (err) {
      handleSubmitError(err)
    }
    return
  }

  const cmd: AddTransactionCommand = {
    accountId: form.accountId,
    type: form.type,
    amount: Number.isFinite(amount) ? amount : 0,
    occurredAt: occurredIso,
    note: form.note.trim() || null,
    categoryId: null
  }

  const parsed = AddTransactionCommandSchema.safeParse(cmd)
  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const k = String(issue.path[0])
      if (!errors[k]) errors[k] = issue.message
    }
    return
  }

  try {
    const result = await addMutate(parsed.data)
    if (result.queued) {
      toast.info('Сохранено локально — отправим при появлении сети')
    } else {
      toast.success(form.type === 'Income' ? 'Доход добавлен' : 'Расход добавлен')
    }
    ui.recordTransactionOpen = false
  } catch (err) {
    handleSubmitError(err)
  }
}

function handleSubmitError(err: unknown) {
  if (err instanceof ApiError) {
    if (err.isValidation) {
      for (const [k, v] of Object.entries(err.fieldErrors)) {
        errors[k.toLowerCase()] = v[0] ?? ''
      }
    } else {
      toast.error(err.message)
    }
  } else {
    toast.error(t.common.unknownError)
  }
}
</script>

<template>
  <DialogRoot :open="open" @update:open="(v) => !v && closeDialog()">
    <DialogPortal>
      <DialogOverlay
        class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out data-[state=open]:fade-in"
      />
      <DialogContent
        class="fixed top-0 right-0 z-50 h-dvh w-[min(480px,100vw)] bg-surface border-l border-border shadow-xl flex flex-col focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:slide-in-from-right data-[state=closed]:slide-out-to-right"
      >
        <header class="h-14 flex items-center justify-between px-5 border-b border-border">
          <div>
            <DialogTitle class="text-[16px] font-semibold tracking-[-0.005em]">
              {{ isEditMode ? 'Изменить операцию' : 'Новая операция' }}
            </DialogTitle>
            <DialogDescription class="sr-only">
              {{
                isEditMode
                  ? 'Измените сумму, дату или комментарий уже сохранённой операции.'
                  : 'Добавьте доход или расход на один из ваших счетов.'
              }}
            </DialogDescription>
          </div>
          <DialogClose
            class="size-8 rounded-md inline-flex items-center justify-center text-fg-muted hover:text-fg hover:bg-surface-hi transition-colors"
          >
            <Icon name="x" :size="16" />
          </DialogClose>
        </header>

        <form class="flex-1 overflow-auto px-5 py-5 space-y-5" novalidate @submit="onSubmit">
          <!-- Type toggle -->
          <div
            class="grid grid-cols-2 gap-1 p-1 bg-surface-sunk rounded-md"
            :class="isEditMode && 'opacity-60 pointer-events-none'"
          >
            <button
              type="button"
              :class="
                cn(
                  'h-9 rounded-[6px] text-[13px] font-medium transition-colors',
                  form.type === 'Expense' ? 'bg-surface text-fg shadow-xs' : 'text-fg-muted hover:text-fg'
                )
              "
              @click="form.type = 'Expense'"
            >
              <Icon name="arrowUp" :size="14" class="inline -mt-px mr-1" />
              Расход
            </button>
            <button
              type="button"
              :class="
                cn(
                  'h-9 rounded-[6px] text-[13px] font-medium transition-colors',
                  form.type === 'Income' ? 'bg-surface text-fg shadow-xs' : 'text-fg-muted hover:text-fg'
                )
              "
              @click="form.type = 'Income'"
            >
              <Icon name="arrowDown" :size="14" class="inline -mt-px mr-1" />
              Доход
            </button>
          </div>
          <p v-if="isEditMode" class="text-[11.5px] text-fg-subtle -mt-3">
            Тип и счёт зафиксированы при создании.
          </p>

          <Field label="Счёт" :error="errors.accountId" required>
            <div v-if="accountsData?.length" class="grid gap-2">
              <button
                v-for="acc in accountsData"
                :key="acc.id"
                type="button"
                :disabled="isEditMode"
                :class="
                  cn(
                    'flex items-center justify-between gap-3 h-12 px-3 rounded-md border text-left',
                    'transition-[border-color,background] duration-[120ms]',
                    form.accountId === acc.id
                      ? 'border-accent bg-accent-soft'
                      : 'border-border hover:border-border-strong',
                    isEditMode && form.accountId !== acc.id && 'opacity-40',
                    isEditMode && 'cursor-not-allowed'
                  )
                "
                @click="!isEditMode && (form.accountId = acc.id)"
              >
                <span class="font-medium text-fg truncate">{{ acc.name }}</span>
                <span class="num text-[13px] text-fg-muted tabular-nums">{{ acc.currency }}</span>
              </button>
            </div>
            <p v-else class="text-[13px] text-fg-subtle">
              Сначала создайте счёт на странице «Счета».
            </p>
          </Field>

          <Field
            label="Сумма"
            :hint="selectedAccount ? `Будет ${form.type === 'Income' ? 'зачислено' : 'списано'} в ${selectedAccount.currency}` : undefined"
            :error="errors.amount"
            required
          >
            <template #default="{ invalid }">
              <Input
                v-model="form.amount"
                inputmode="decimal"
                placeholder="0,00"
                size="lg"
                :invalid="invalid"
              />
            </template>
          </Field>

          <div class="grid grid-cols-1 gap-4">
            <Field label="Дата и время" :error="errors.occurredAt" required>
              <template #default="{ invalid }">
                <Input
                  v-model="form.occurredAt"
                  type="datetime-local"
                  icon-left="calendar"
                  :invalid="invalid"
                />
              </template>
            </Field>

            <Field label="Комментарий" :hint="`${form.note.length}/500`" :error="errors.note">
              <template #default="{ invalid }">
                <Input
                  v-model="form.note"
                  placeholder="Например, обед в кафе"
                  :invalid="invalid"
                />
              </template>
            </Field>
          </div>
        </form>

        <footer class="px-5 py-4 border-t border-border flex items-center justify-end gap-2 bg-surface">
          <DialogClose as-child>
            <Button intent="ghost" type="button">{{ t.common.cancel }}</Button>
          </DialogClose>
          <Button
            type="submit"
            :loading="isPending"
            :disabled="!accountsData?.length"
            :icon-left="isEditMode ? 'check' : 'plus'"
            @click="onSubmit"
          >
            {{ isEditMode ? 'Сохранить' : 'Добавить' }}
          </Button>
        </footer>
      </DialogContent>
    </DialogPortal>
  </DialogRoot>
</template>
