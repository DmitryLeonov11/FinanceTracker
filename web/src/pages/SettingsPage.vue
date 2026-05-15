<script setup lang="ts">
import { Card, Button } from '@/shared/ui/primitives'
import { useUiStore, type ThemeMode } from '@/shared/stores/ui'
import { useAuthStore } from '@/shared/stores/auth'
import Icon from '@/shared/ui/icons/Icon.vue'
import type { IconName } from '@/shared/ui/icons/paths'
import { cn } from '@/shared/lib/cn'
import { t } from '@/shared/lib/i18n'

const ui = useUiStore()
const auth = useAuthStore()

const themes: { mode: ThemeMode; label: string; icon: IconName }[] = [
  { mode: 'light', label: t.common.theme.light, icon: 'sun' },
  { mode: 'dark', label: t.common.theme.dark, icon: 'moon' },
  { mode: 'system', label: t.common.theme.system, icon: 'monitor' }
]
</script>

<template>
  <div class="space-y-6 max-w-2xl">
    <Card>
      <h2 class="text-[15px] font-semibold mb-1">Профиль</h2>
      <p class="text-[13px] text-fg-muted mb-4">Базовая информация об аккаунте.</p>
      <dl class="space-y-2 text-[14px]">
        <div class="flex justify-between gap-4">
          <dt class="text-fg-muted">Имя</dt>
          <dd>{{ auth.user?.displayName }}</dd>
        </div>
        <div class="flex justify-between gap-4">
          <dt class="text-fg-muted">Email</dt>
          <dd>{{ auth.user?.email }}</dd>
        </div>
      </dl>
    </Card>

    <Card>
      <h2 class="text-[15px] font-semibold mb-1">Тема</h2>
      <p class="text-[13px] text-fg-muted mb-4">Выберите оформление интерфейса.</p>
      <div class="grid grid-cols-3 gap-2">
        <button
          v-for="opt in themes"
          :key="opt.mode"
          :class="
            cn(
              'flex flex-col items-center justify-center gap-1 h-16 rounded-md border text-[13px]',
              'transition-[border-color,background] duration-[120ms]',
              ui.themeMode === opt.mode
                ? 'border-accent bg-accent-soft text-accent-soft-fg'
                : 'border-border hover:border-border-strong text-fg-muted hover:text-fg'
            )
          "
          @click="ui.setTheme(opt.mode)"
        >
          <Icon :name="opt.icon" :size="18" />
          {{ opt.label }}
        </button>
      </div>
    </Card>

    <Card>
      <h2 class="text-[15px] font-semibold mb-1">Сессия</h2>
      <p class="text-[13px] text-fg-muted mb-4">Завершить текущую сессию.</p>
      <Button intent="secondary" icon-left="logOut" @click="auth.logout">
        {{ t.auth.logout }}
      </Button>
    </Card>
  </div>
</template>
