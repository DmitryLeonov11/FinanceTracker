<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import { toast } from 'vue-sonner'

import { Button, Input } from '@/shared/ui/primitives'
import Field from '@/shared/ui/form/Field.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import { useAuthStore } from '@/shared/stores/auth'
import { realtime } from '@/shared/api/signalr'
import { RegisterCommandSchema } from '@/entities/user/model/schemas'
import { SUPPORTED_CURRENCIES, CURRENCY_LABELS, type CurrencyCode } from '@/shared/config/currencies'
import { ApiError } from '@/shared/api/errors'
import { t } from '@/shared/lib/i18n'
import { cn } from '@/shared/lib/cn'

const auth = useAuthStore()
const router = useRouter()

const form = reactive({
  email: '',
  password: '',
  displayName: '',
  displayCurrency: 'BYN' as CurrencyCode
})
const errors = reactive<Record<string, string>>({})
const submitting = ref(false)

async function onSubmit(e: Event) {
  e.preventDefault()
  Object.keys(errors).forEach((k) => delete errors[k])

  const parsed = RegisterCommandSchema.safeParse(form)
  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const key = String(issue.path[0])
      if (!errors[key]) errors[key] = issue.message
    }
    return
  }

  submitting.value = true
  try {
    await auth.register(parsed.data)
    realtime.connect().catch(() => {})
    toast.success('Аккаунт создан')
    await router.push('/dashboard')
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
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div>
    <h1 class="text-[28px] font-semibold tracking-[-0.015em] leading-tight">
      {{ t.auth.registerTitle }}
    </h1>
    <p class="text-fg-muted mt-1.5 text-[14px]">{{ t.auth.registerSubtitle }}</p>

    <form class="mt-8 space-y-4" novalidate @submit="onSubmit">
      <Field :label="t.auth.displayName" :error="errors.displayName" required>
        <template #default="{ invalid }">
          <Input
            v-model="form.displayName"
            autocomplete="name"
            placeholder="Иван Иванов"
            :invalid="invalid"
          />
        </template>
      </Field>

      <Field :label="t.auth.email" :error="errors.email" required>
        <template #default="{ invalid }">
          <Input
            v-model="form.email"
            type="email"
            inputmode="email"
            autocomplete="email"
            icon-left="user"
            placeholder="user@example.com"
            :invalid="invalid"
          />
        </template>
      </Field>

      <Field
        :label="t.auth.password"
        :hint="!errors.password ? 'Минимум 8 символов: заглавные, строчные и цифры' : undefined"
        :error="errors.password"
        required
      >
        <template #default="{ invalid }">
          <Input
            v-model="form.password"
            type="password"
            autocomplete="new-password"
            placeholder="••••••••"
            :invalid="invalid"
            show-password-toggle
          />
        </template>
      </Field>

      <Field :label="t.auth.displayCurrency" :error="errors.displayCurrency" required>
        <div class="grid grid-cols-4 gap-2">
          <button
            v-for="c in SUPPORTED_CURRENCIES"
            :key="c"
            type="button"
            :class="
              cn(
                'flex flex-col items-center justify-center gap-0.5 h-14 rounded-md border px-2',
                'transition-[border-color,background] duration-[120ms]',
                form.displayCurrency === c
                  ? 'border-accent bg-accent-soft text-accent-soft-fg'
                  : 'border-border hover:border-border-strong text-fg-muted hover:text-fg'
              )
            "
            @click="form.displayCurrency = c"
          >
            <span class="text-[13px] font-semibold">{{ c }}</span>
            <span class="text-[10px] text-fg-subtle truncate w-full text-center">
              {{ CURRENCY_LABELS[c] }}
            </span>
          </button>
        </div>
      </Field>

      <Button type="submit" block size="lg" :loading="submitting">
        {{ t.auth.submitRegister }}
        <Icon name="arrowRight" :size="16" />
      </Button>
    </form>

    <p class="mt-6 text-center text-[13px] text-fg-muted">
      {{ t.auth.haveAccount }}
      <RouterLink to="/login" class="text-accent hover:text-accent-hover font-medium ml-1">
        {{ t.auth.signIn }}
      </RouterLink>
    </p>
  </div>
</template>
