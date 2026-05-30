<script setup lang="ts">
import { ref, watch, computed } from 'vue'
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
import { useAccount } from '@/entities/account/api/useAccount'
import { useRenameAccount } from '@/entities/account/api/useRenameAccount'
import { useUiStore } from '@/shared/stores/ui'
import { ApiError } from '@/shared/api/errors'
import { t } from '@/shared/lib/i18n'

const ui = useUiStore()
const accountIdRef = computed(() => ui.renamingAccountId ?? undefined)
const { data: account } = useAccount(accountIdRef)
const { mutateAsync, isPending } = useRenameAccount()

const name = ref('')
const error = ref<string | null>(null)

watch(
  () => ui.renamingAccountId,
  (id) => {
    if (id) {
      name.value = account.value?.name ?? ''
      error.value = null
    }
  }
)

watch(account, (a) => {
  if (a && name.value.trim().length === 0) name.value = a.name
})

async function onSubmit(e: Event) {
  e.preventDefault()
  error.value = null

  const trimmed = name.value.trim()
  if (!trimmed) {
    error.value = 'Название обязательно.'
    return
  }
  if (trimmed.length > 100) {
    error.value = 'Не более 100 символов.'
    return
  }
  const id = ui.renamingAccountId
  if (!id) return

  try {
    const updated = await mutateAsync({ id, name: trimmed })
    toast.success(`Счёт переименован в «${updated.name}»`)
    ui.closeRenameAccount()
  } catch (err) {
    if (err instanceof ApiError) {
      if (err.isValidation) {
        error.value = Object.values(err.fieldErrors)[0]?.[0] ?? err.message
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
  <DialogRoot
    :open="!!ui.renamingAccountId"
    @update:open="(v) => !v && ui.closeRenameAccount()"
  >
    <DialogPortal>
      <DialogOverlay class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out data-[state=open]:fade-in" />
      <DialogContent
        class="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-[min(420px,calc(100vw-32px))] bg-surface border border-border rounded-xl shadow-xl p-6 focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:fade-in data-[state=open]:zoom-in-95 data-[state=closed]:zoom-out-95 data-[state=closed]:fade-out"
      >
        <div class="flex items-start justify-between gap-3 mb-4">
          <div>
            <DialogTitle class="text-[16px] font-semibold tracking-[-0.005em]">
              Переименовать счёт
            </DialogTitle>
            <DialogDescription class="text-[13px] text-fg-muted mt-1">
              Изменится только название. Валюта и операции остаются прежними.
            </DialogDescription>
          </div>
          <DialogClose
            class="size-8 rounded-md inline-flex items-center justify-center text-fg-muted hover:text-fg hover:bg-surface-hi transition-colors"
          >
            <Icon name="x" :size="16" />
          </DialogClose>
        </div>

        <form class="space-y-4" novalidate @submit="onSubmit">
          <Field label="Название" :error="error ?? undefined" required>
            <template #default="{ invalid }">
              <Input v-model="name" placeholder="Например, Карта Зарплатная" :invalid="invalid" />
            </template>
          </Field>

          <div class="flex items-center justify-end gap-2 pt-2">
            <DialogClose as-child>
              <Button intent="ghost" type="button">{{ t.common.cancel }}</Button>
            </DialogClose>
            <Button type="submit" :loading="isPending" icon-left="check">
              Сохранить
            </Button>
          </div>
        </form>
      </DialogContent>
    </DialogPortal>
  </DialogRoot>
</template>
