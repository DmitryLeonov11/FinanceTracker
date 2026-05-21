<script setup lang="ts">
import { reactive, computed, watch } from 'vue'
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
import { RecordTransferCommandSchema, type RecordTransferCommand } from '@/entities/transaction/model/schemas'
import { useRecordTransfer } from '@/entities/transaction/api/useRecordTransfer'
import { useAccounts } from '@/entities/account/api/useAccounts'
import { useUiStore } from '@/shared/stores/ui'
import { ApiError } from '@/shared/api/errors'
import { cn } from '@/shared/lib/cn'
import { t } from '@/shared/lib/i18n'

const ui = useUiStore()
const { data: accountsData } = useAccounts()

interface FormState {
  sourceAccountId: string
  destinationAccountId: string
  amount: string
  occurredAt: string
  note: string
}

function defaultForm(): FormState {
  const now = new Date()
  const local = new Date(now.getTime() - now.getTimezoneOffset() * 60_000)
  return {
    sourceAccountId: '',
    destinationAccountId: '',
    amount: '',
    occurredAt: local.toISOString().slice(0, 16),
    note: ''
  }
}

const form = reactive<FormState>(defaultForm())
const errors = reactive<Record<string, string>>({})

const { mutateAsync, isPending } = useRecordTransfer()

const sourceAccount = computed(() =>
  accountsData.value?.find((a) => a.id === form.sourceAccountId) ?? null
)

/** Only same-currency destinations are valid in v1. */
const destinationCandidates = computed(() => {
  const list = accountsData.value ?? []
  if (!form.sourceAccountId) return list
  const src = list.find((a) => a.id === form.sourceAccountId)
  if (!src) return list
  return list.filter((a) => a.id !== src.id && a.currency === src.currency)
})

const hasCrossCurrencyAccounts = computed(() => {
  const list = accountsData.value ?? []
  if (list.length < 2 || !form.sourceAccountId) return false
  const src = list.find((a) => a.id === form.sourceAccountId)
  if (!src) return false
  return list.some((a) => a.id !== src.id && a.currency !== src.currency)
})

watch(
  () => ui.recordTransferOpen,
  (open) => {
    if (open) {
      Object.assign(form, defaultForm())
      Object.keys(errors).forEach((k) => delete errors[k])
      const list = accountsData.value ?? []
      if (list.length >= 1) form.sourceAccountId = list[0]!.id
    }
  }
)

// reset destination if source changes (different-currency accounts become invalid)
watch(
  () => form.sourceAccountId,
  () => {
    if (form.destinationAccountId && !destinationCandidates.value.some((a) => a.id === form.destinationAccountId)) {
      form.destinationAccountId = ''
    }
  }
)

async function onSubmit(e: Event) {
  e.preventDefault()
  Object.keys(errors).forEach((k) => delete errors[k])

  const amount = Number(form.amount.replace(',', '.'))
  const occurredIso = new Date(form.occurredAt).toISOString()

  const cmd: RecordTransferCommand = {
    sourceAccountId: form.sourceAccountId,
    destinationAccountId: form.destinationAccountId,
    amount: Number.isFinite(amount) ? amount : 0,
    occurredAt: occurredIso,
    note: form.note.trim() || null
  }

  const parsed = RecordTransferCommandSchema.safeParse(cmd)
  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const k = String(issue.path[0])
      if (!errors[k]) errors[k] = issue.message
    }
    return
  }

  try {
    await mutateAsync(parsed.data)
    toast.success('Перевод выполнен')
    ui.closeRecordTransfer()
  } catch (err) {
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
}
</script>

<template>
  <DialogRoot :open="ui.recordTransferOpen" @update:open="(v) => !v && ui.closeRecordTransfer()">
    <DialogPortal>
      <DialogOverlay
        class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out data-[state=open]:fade-in"
      />
      <DialogContent
        class="fixed top-0 right-0 z-50 h-dvh w-[min(480px,100vw)] bg-surface border-l border-border shadow-xl flex flex-col focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:slide-in-from-right data-[state=closed]:slide-out-to-right"
      >
        <header class="h-14 flex items-center justify-between px-5 border-b border-border">
          <div class="flex items-center gap-2">
            <div class="size-7 rounded-md bg-info-soft text-info flex items-center justify-center">
              <Icon name="swap" :size="14" />
            </div>
            <div>
              <DialogTitle class="text-[16px] font-semibold tracking-[-0.005em]">
                Перевод между счетами
              </DialogTitle>
              <DialogDescription class="sr-only">
                Переместить деньги между двумя счетами в одной валюте.
              </DialogDescription>
            </div>
          </div>
          <DialogClose
            class="size-8 rounded-md inline-flex items-center justify-center text-fg-muted hover:text-fg hover:bg-surface-hi transition-colors"
          >
            <Icon name="x" :size="16" />
          </DialogClose>
        </header>

        <form class="flex-1 overflow-auto px-5 py-5 space-y-5" novalidate @submit="onSubmit">
          <Field label="Откуда" :error="errors.sourceAccountId" required>
            <div v-if="accountsData?.length" class="grid gap-2">
              <button
                v-for="acc in accountsData"
                :key="acc.id"
                type="button"
                :class="
                  cn(
                    'flex items-center justify-between gap-3 h-12 px-3 rounded-md border text-left',
                    'transition-[border-color,background] duration-[120ms]',
                    form.sourceAccountId === acc.id
                      ? 'border-accent bg-accent-soft'
                      : 'border-border hover:border-border-strong'
                  )
                "
                @click="form.sourceAccountId = acc.id"
              >
                <span class="font-medium text-fg truncate">{{ acc.name }}</span>
                <span class="num text-[13px] text-fg-muted tabular-nums">{{ acc.currency }}</span>
              </button>
            </div>
            <p v-else class="text-[13px] text-fg-subtle">Сначала создайте хотя бы один счёт.</p>
          </Field>

          <Field label="Куда" :error="errors.destinationAccountId" required>
            <div v-if="destinationCandidates.length" class="grid gap-2">
              <button
                v-for="acc in destinationCandidates"
                :key="acc.id"
                type="button"
                :class="
                  cn(
                    'flex items-center justify-between gap-3 h-12 px-3 rounded-md border text-left',
                    'transition-[border-color,background] duration-[120ms]',
                    form.destinationAccountId === acc.id
                      ? 'border-accent bg-accent-soft'
                      : 'border-border hover:border-border-strong'
                  )
                "
                @click="form.destinationAccountId = acc.id"
              >
                <span class="font-medium text-fg truncate">{{ acc.name }}</span>
                <span class="num text-[13px] text-fg-muted tabular-nums">{{ acc.currency }}</span>
              </button>
            </div>
            <p v-else-if="form.sourceAccountId" class="text-[13px] text-fg-subtle">
              Нет других счетов в той же валюте.
            </p>
            <p v-else class="text-[13px] text-fg-subtle">Сначала выберите счёт-источник.</p>

            <p
              v-if="hasCrossCurrencyAccounts"
              class="text-[12px] text-fg-subtle mt-2"
            >
              Кросс-валютные переводы появятся вместе с FX-модулем.
            </p>
          </Field>

          <Field
            label="Сумма"
            :hint="sourceAccount ? `Переводим в ${sourceAccount.currency}` : undefined"
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
                  placeholder="Например, перевод на сбережения"
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
            :disabled="!destinationCandidates.length"
            icon-left="swap"
            @click="onSubmit"
          >
            Перевести
          </Button>
        </footer>
      </DialogContent>
    </DialogPortal>
  </DialogRoot>
</template>
