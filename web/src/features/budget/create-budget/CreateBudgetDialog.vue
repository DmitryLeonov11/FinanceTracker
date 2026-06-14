<script setup lang="ts">
import { reactive, watch } from 'vue'
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
  CreateBudgetCommandSchema,
  BUDGET_PERIOD_LABELS,
  type CreateBudgetCommand,
  type BudgetPeriod
} from '@/entities/budget/model/schemas'
import { useCreateBudget } from '@/entities/budget/api/useCreateBudget'
import CategorySelect from '@/entities/category/ui/CategorySelect.vue'
import { SUPPORTED_CURRENCIES, type CurrencyCode } from '@/shared/config/currencies'
import { useUiStore } from '@/shared/stores/ui'
import { ApiError } from '@/shared/api/errors'
import { cn } from '@/shared/lib/cn'
import { t } from '@/shared/lib/i18n'

const ui = useUiStore()
const { mutateAsync, isPending } = useCreateBudget()

interface FormState {
  name: string
  period: BudgetPeriod
  currency: CurrencyCode
  limit: string
  startDate: string
  rollover: boolean
  categoryId: string | null
}

function todayIso(): string {
  return new Date().toISOString().slice(0, 10)
}

function defaultForm(): FormState {
  return {
    name: '',
    period: 'Month',
    currency: 'BYN',
    limit: '',
    startDate: todayIso(),
    rollover: false,
    categoryId: null
  }
}

const form = reactive<FormState>(defaultForm())
const errors = reactive<Record<string, string>>({})

watch(
  () => ui.createBudgetOpen,
  (open) => {
    if (open) {
      Object.assign(form, defaultForm())
      Object.keys(errors).forEach((k) => delete errors[k])
    }
  }
)

const periodOptions: BudgetPeriod[] = ['Week', 'Month', 'Quarter', 'Year']

async function onSubmit(e: Event) {
  e.preventDefault()
  Object.keys(errors).forEach((k) => delete errors[k])

  const limit = Number(form.limit.replace(',', '.'))

  const cmd: CreateBudgetCommand = {
    name: form.name.trim(),
    period: form.period,
    currency: form.currency,
    limit: Number.isFinite(limit) ? limit : 0,
    startDate: form.startDate,
    rollover: form.rollover,
    categoryId: form.categoryId
  }

  const parsed = CreateBudgetCommandSchema.safeParse(cmd)
  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const k = String(issue.path[0])
      if (!errors[k]) errors[k] = issue.message
    }
    return
  }

  try {
    await mutateAsync(parsed.data)
    toast.success(`Бюджет «${parsed.data.name}» создан`)
    ui.closeCreateBudget()
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
  <DialogRoot :open="ui.createBudgetOpen" @update:open="(v) => !v && ui.closeCreateBudget()">
    <DialogPortal>
      <DialogOverlay
        class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out data-[state=open]:fade-in"
      />
      <DialogContent
        class="fixed top-0 right-0 z-50 h-dvh w-[min(480px,100vw)] bg-surface border-l border-border shadow-xl flex flex-col focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:slide-in-from-right data-[state=closed]:slide-out-to-right"
      >
        <header class="h-14 flex items-center justify-between px-5 border-b border-border">
          <div class="flex items-center gap-2">
            <div class="size-7 rounded-md bg-accent-soft text-accent-soft-fg flex items-center justify-center">
              <Icon name="pieChart" :size="14" />
            </div>
            <div>
              <DialogTitle class="text-[16px] font-semibold tracking-[-0.005em]">
                Новый бюджет
              </DialogTitle>
              <DialogDescription class="sr-only">
                Задайте лимит расходов в валюте на выбранный период.
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
          <Field label="Название" :error="errors.name" required>
            <template #default="{ invalid }">
              <Input v-model="form.name" placeholder="Например, Еда" :invalid="invalid" />
            </template>
          </Field>

          <Field label="Период" :error="errors.period" required>
            <div class="grid grid-cols-4 gap-2">
              <button
                v-for="p in periodOptions"
                :key="p"
                type="button"
                :class="
                  cn(
                    'h-10 rounded-md border text-[13px] font-medium',
                    'transition-[border-color,background] duration-[120ms]',
                    form.period === p
                      ? 'border-accent bg-accent-soft text-accent-soft-fg'
                      : 'border-border hover:border-border-strong text-fg-muted hover:text-fg'
                  )
                "
                @click="form.period = p"
              >
                {{ BUDGET_PERIOD_LABELS[p] }}
              </button>
            </div>
          </Field>

          <div class="grid grid-cols-2 gap-3">
            <Field label="Валюта" :error="errors.currency" required>
              <div class="grid grid-cols-2 gap-2">
                <button
                  v-for="c in SUPPORTED_CURRENCIES"
                  :key="c"
                  type="button"
                  :class="
                    cn(
                      'h-10 rounded-md border text-[13px] font-semibold',
                      'transition-[border-color,background] duration-[120ms]',
                      form.currency === c
                        ? 'border-accent bg-accent-soft text-accent-soft-fg'
                        : 'border-border hover:border-border-strong text-fg-muted hover:text-fg'
                    )
                  "
                  @click="form.currency = c"
                >
                  {{ c }}
                </button>
              </div>
            </Field>

            <Field label="Лимит" :error="errors.limit" required>
              <template #default="{ invalid }">
                <Input
                  v-model="form.limit"
                  inputmode="decimal"
                  placeholder="0,00"
                  size="lg"
                  :invalid="invalid"
                />
              </template>
            </Field>
          </div>

          <Field label="Дата старта" :error="errors.startDate" required>
            <template #default="{ invalid }">
              <Input
                v-model="form.startDate"
                type="date"
                icon-left="calendar"
                :invalid="invalid"
              />
            </template>
          </Field>

          <Field
            label="Категория"
            :error="errors.categoryId"
            hint="Оставьте без категории, чтобы лимит покрывал все расходы выбранной валюты"
          >
            <CategorySelect
              v-model="form.categoryId"
              kind="Expense"
              all-label="Все расходы"
            />
          </Field>
        </form>

        <footer class="px-5 py-4 border-t border-border flex items-center justify-end gap-2 bg-surface">
          <DialogClose as-child>
            <Button intent="ghost" type="button">{{ t.common.cancel }}</Button>
          </DialogClose>
          <Button type="submit" :loading="isPending" icon-left="plus" @click="onSubmit">
            Создать
          </Button>
        </footer>
      </DialogContent>
    </DialogPortal>
  </DialogRoot>
</template>
