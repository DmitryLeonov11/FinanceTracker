<script setup lang="ts">
import { useRoute, useRouter, RouterLink } from 'vue-router'
import { computed } from 'vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import type { IconName } from '@/shared/ui/icons/paths'
import ConnectionIndicator from '@/shared/ui/nav/ConnectionIndicator.vue'
import { useAuthStore } from '@/shared/stores/auth'
import { useUiStore } from '@/shared/stores/ui'
import { t } from '@/shared/lib/i18n'
import { cn } from '@/shared/lib/cn'

interface NavItem {
  label: string
  to: string
  icon: IconName
}

const navItems: NavItem[] = [
  { label: t.nav.dashboard, to: '/dashboard', icon: 'home' },
  { label: t.nav.accounts, to: '/accounts', icon: 'wallet' },
  { label: t.nav.transactions, to: '/transactions', icon: 'swap' },
  { label: t.nav.budgets, to: '/budgets', icon: 'pieChart' },
  { label: t.nav.goals, to: '/goals', icon: 'target' },
  { label: t.nav.analytics, to: '/analytics', icon: 'barChart' }
]

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const ui = useUiStore()

const initials = computed(() => {
  const name = auth.user?.displayName ?? ''
  return name
    .split(' ')
    .map((p) => p[0])
    .filter(Boolean)
    .slice(0, 2)
    .join('')
    .toUpperCase()
})

const cycleTheme = () => {
  const order = ['system', 'light', 'dark'] as const
  const next = order[(order.indexOf(ui.themeMode) + 1) % order.length]
  ui.setTheme(next)
}

const themeIcon = computed<IconName>(() =>
  ui.themeMode === 'system' ? 'monitor' : ui.themeMode === 'dark' ? 'moon' : 'sun'
)

async function handleLogout() {
  auth.logout()
  await router.push('/login')
}
</script>

<template>
  <div class="min-h-dvh flex bg-bg">
    <!-- Sidebar (desktop) -->
    <aside
      :class="
        cn(
          'hidden md:flex flex-col border-r border-border bg-surface',
          'transition-[width] duration-200',
          ui.sidebarCollapsed ? 'w-[72px]' : 'w-[240px]'
        )
      "
    >
      <div class="h-14 flex items-center gap-2 px-4">
        <div class="size-7 shrink-0 rounded-md bg-accent flex items-center justify-center text-accent-fg">
          <Icon name="logo" :size="16" :stroke-width="2" />
        </div>
        <span v-if="!ui.sidebarCollapsed" class="font-semibold tracking-[-0.005em] truncate">
          {{ t.app.name }}
        </span>
      </div>
      <nav class="flex-1 px-2 pt-2 space-y-0.5">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          :title="ui.sidebarCollapsed ? item.label : undefined"
          :class="
            cn(
              'group flex items-center gap-2.5 rounded-md px-2.5 h-9 text-[13.5px] font-medium',
              'transition-colors duration-[120ms]',
              'text-fg-muted hover:bg-surface-hi hover:text-fg',
              route.path.startsWith(item.to) && 'bg-surface-hi text-fg'
            )
          "
        >
          <Icon
            :name="item.icon"
            :size="18"
            :class="
              cn(
                'shrink-0 transition-colors',
                route.path.startsWith(item.to) ? 'text-accent' : 'text-fg-subtle group-hover:text-fg-muted'
              )
            "
          />
          <span v-if="!ui.sidebarCollapsed" class="truncate">{{ item.label }}</span>
        </RouterLink>
      </nav>
      <div class="p-2 border-t border-border">
        <button
          class="w-full flex items-center gap-2 rounded-md px-2 h-9 text-[13.5px] text-fg-muted hover:bg-surface-hi hover:text-fg transition-colors"
          @click="ui.toggleSidebar"
          :title="ui.sidebarCollapsed ? 'Развернуть' : 'Свернуть'"
        >
          <Icon :name="ui.sidebarCollapsed ? 'chevronRight' : 'chevronLeft'" :size="16" />
          <span v-if="!ui.sidebarCollapsed">Свернуть</span>
        </button>
      </div>
    </aside>

    <!-- Main column -->
    <div class="flex-1 flex flex-col min-w-0">
      <header
        class="h-14 px-4 md:px-6 flex items-center gap-3 border-b border-border bg-surface/80 backdrop-blur-md sticky top-0 z-20"
      >
        <button
          class="md:hidden inline-flex items-center justify-center size-9 -ml-1 rounded-md text-fg-muted hover:text-fg hover:bg-surface-hi"
          @click="ui.toggleSidebar"
        >
          <Icon name="menu" :size="20" />
        </button>
        <h1 class="text-[15px] font-semibold tracking-[-0.005em]">{{ route.meta.title }}</h1>
        <div class="ml-auto flex items-center gap-1">
          <ConnectionIndicator class="mr-1" />
          <button
            class="inline-flex items-center justify-center size-9 rounded-md text-fg-muted hover:text-fg hover:bg-surface-hi transition-colors"
            :title="`Тема: ${ui.themeMode}`"
            @click="cycleTheme"
          >
            <Icon :name="themeIcon" :size="18" />
          </button>
          <button
            class="inline-flex items-center justify-center size-9 rounded-md text-fg-muted hover:text-fg hover:bg-surface-hi transition-colors"
          >
            <Icon name="bell" :size="18" />
          </button>
          <div class="ml-1 flex items-center gap-2">
            <div
              class="size-8 rounded-full bg-accent-soft text-accent-soft-fg flex items-center justify-center text-[12px] font-semibold"
            >
              {{ initials || '·' }}
            </div>
            <button
              class="hidden sm:inline-flex items-center gap-1.5 text-[13px] text-fg-muted hover:text-fg transition-colors"
              @click="handleLogout"
            >
              <Icon name="logOut" :size="16" />
              {{ t.auth.logout }}
            </button>
          </div>
        </div>
      </header>

      <main class="flex-1 px-4 md:px-6 py-6 md:py-8 max-w-[1400px] w-full mx-auto">
        <slot />
      </main>
    </div>
  </div>
</template>
