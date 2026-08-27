<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { toast } from 'vue-sonner'
import {
  DialogRoot,
  DialogPortal,
  DialogOverlay,
  DialogContent,
  DialogTitle,
  DialogDescription,
  DialogClose
} from 'radix-vue'

import { Button, Input } from '@/shared/ui/primitives'
import Field from '@/shared/ui/form/Field.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import { CreateAccountCommandSchema, type CreateAccountCommand } from '@/entities/account/model/schemas'
import { useCreateAccount } from '@/entities/account/api/useCreateAccount'
import { SUPPORTED_CURRENCIES } from '@/shared/config/currencies'
import { t } from '@/shared/lib/i18n'
import { ApiError } from '@/shared/api/errors'
import { cn } from '@/shared/lib/cn'

const props = defineProps<{ open: boolean }>()
const emit = defineEmits<{ 'update:open': [v: boolean] }>()

const form = reactive<CreateAccountCommand>({
  name: '',
  type: 'Bank',
  currency: 'BYN',
  initialBalance: 0
})
const errors = reactive<Record<string, string>>({})

const { mutateAsync, isPending } = useCreateAccount()

watch(
  () => props.open,
  (open) => {
    if (open) {
      form.name = ''
      form.type = 'Bank'
      form.currency = 'BYN'
      form.initialBalance = 0
      Object.keys(errors).forEach((k) => delete errors[k])
    }
  }
)

const accountTypes: { value: CreateAccountCommand['type']; label: string; icon: 'card' | 'wallet' }[] = [
  { value: 'Cash', label: t.accounts.types.Cash, icon: 'wallet' },
  { value: 'Bank', label: t.accounts.types.Bank, icon: 'card' },
  { value: 'Card', label: t.accounts.types.Card, icon: 'card' },
  { value: 'Crypto', label: t.accounts.types.Crypto, icon: 'wallet' }
]

const balanceInput = ref('0')

async function onSubmit(e: Event) {
  e.preventDefault()
  Object.keys(errors).forEach((k) => delete errors[k])

  const initialBalance = Number(balanceInput.value.replace(',', '.')) || 0

  const parsed = CreateAccountCommandSchema.safeParse({ ...form, initialBalance })
  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const k = String(issue.path[0])
      if (!errors[k]) errors[k] = issue.message
    }
    return
  }

  try {
    await mutateAsync(parsed.data)
    toast.success(`Счёт «${parsed.data.name}» создан`)
    emit('update:open', false)
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
  <DialogRoot :open="open" @update:open="emit('update:open', $event)">
    <DialogPortal>
      <DialogOverlay
        class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out data-[state=open]:fade-in"
      />
      <DialogContent
        class="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-[min(520px,calc(100vw-32px))] max-h-[calc(100vh-32px)] overflow-auto bg-surface border border-border rounded-xl shadow-xl p-6 focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:fade-in data-[state=open]:zoom-in-95 data-[state=closed]:zoom-out-95 data-[state=closed]:fade-out"
      >
        <div class="flex items-start justify-between gap-3 mb-4">
          <div>
            <DialogTitle class="text-[18px] font-semibold tracking-[-0.01em]">
              Новый счёт
            </DialogTitle>
            <DialogDescription class="text-[13px] text-fg-muted mt-1">
              Добавьте счёт, чтобы начать учитывать операции.
            </DialogDescription>
          </div>
          <DialogClose
            class="size-8 rounded-md inline-flex items-center justify-center text-fg-muted hover:text-fg hover:bg-surface-hi transition-colors"
          >
            <Icon name="x" :size="16" />
          </DialogClose>
        </div>

        <form class="space-y-4" novalidate @submit="onSubmit">
          <Field :label="t.accounts.name" :error="errors.name" required>
            <template #default="{ invalid }">
              <Input
                v-model="form.name"
                placeholder="Например, Зарплатная карта"
                :invalid="invalid"
              />
            </template>
          </Field>

          <Field :label="t.accounts.type">
            <div class="grid grid-cols-4 gap-2">
              <button
                v-for="opt in accountTypes"
                :key="opt.value"
                type="button"
                :class="
                  cn(
                    'flex flex-col items-center justify-center gap-1 h-16 rounded-md border text-[12px]',
                    'transition-[border-color,background] duration-[120ms]',
                    form.type === opt.value
                      ? 'border-accent bg-accent-soft text-accent-soft-fg'
                      : 'border-border hover:border-border-strong text-fg-muted hover:text-fg'
                  )
                "
                @click="form.type = opt.value"
              >
                <Icon :name="opt.icon" :size="16" />
                {{ opt.label }}
              </button>
            </div>
          </Field>

          <div class="grid grid-cols-2 gap-3">
            <Field :label="t.accounts.currency">
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

            <Field :label="t.accounts.initialBalance" :error="errors.initialBalance">
              <template #default="{ invalid }">
                <Input
                  v-model="balanceInput"
                  inputmode="decimal"
                  placeholder="0,00"
                  :invalid="invalid"
                />
              </template>
            </Field>
          </div>

          <div class="flex items-center justify-end gap-2 pt-2">
            <DialogClose as-child>
              <Button intent="ghost" type="button">{{ t.common.cancel }}</Button>
            </DialogClose>
            <Button type="submit" :loading="isPending" icon-left="plus">
              {{ t.common.create }}
            </Button>
          </div>
        </form>
      </DialogContent>
    </DialogPortal>
  </DialogRoot>
</template>
