<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter, useRoute, RouterLink } from 'vue-router'
import { toast } from 'vue-sonner'

import { Button, Input } from '@/shared/ui/primitives'
import Field from '@/shared/ui/form/Field.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import { useAuthStore } from '@/shared/stores/auth'
import { realtime } from '@/shared/api/signalr'
import { LoginCommandSchema } from '@/entities/user/model/schemas'
import { ApiError } from '@/shared/api/errors'
import { t } from '@/shared/lib/i18n'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const form = reactive({ email: '', password: '' })
const errors = reactive<Record<string, string>>({})
const submitting = ref(false)

async function onSubmit(e: Event) {
  e.preventDefault()
  Object.keys(errors).forEach((k) => delete errors[k])

  const parsed = LoginCommandSchema.safeParse(form)
  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const key = String(issue.path[0])
      if (!errors[key]) errors[key] = issue.message
    }
    return
  }

  submitting.value = true
  try {
    await auth.login(parsed.data)
    realtime.connect().catch(() => {})
    const redirect = (route.query.redirect as string) || '/dashboard'
    await router.push(redirect)
  } catch (err) {
    if (err instanceof ApiError) {
      if (err.isUnauthorized || err.isForbidden) {
        toast.error(err.message || t.auth.invalidCredentials)
      } else if (err.isValidation) {
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
    <h1 class="text-[28px] font-semibold tracking-[-0.015em] leading-tight">{{ t.auth.loginTitle }}</h1>
    <p class="text-fg-muted mt-1.5 text-[14px]">{{ t.auth.loginSubtitle }}</p>

    <form class="mt-8 space-y-4" novalidate @submit="onSubmit">
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

      <Field :label="t.auth.password" :error="errors.password" required>
        <template #default="{ invalid }">
          <Input
            v-model="form.password"
            type="password"
            autocomplete="current-password"
            placeholder="••••••••"
            :invalid="invalid"
            show-password-toggle
          />
        </template>
      </Field>

      <Button type="submit" block size="lg" :loading="submitting">
        {{ t.auth.submitLogin }}
        <Icon name="arrowRight" :size="16" />
      </Button>
    </form>

    <p class="mt-6 text-center text-[13px] text-fg-muted">
      {{ t.auth.noAccount }}
      <RouterLink to="/register" class="text-accent hover:text-accent-hover font-medium ml-1">
        {{ t.auth.signUp }}
      </RouterLink>
    </p>
  </div>
</template>
