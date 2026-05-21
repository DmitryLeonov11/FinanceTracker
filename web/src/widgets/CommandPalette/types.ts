import type { Router } from 'vue-router'
import type { IconName } from '@/shared/ui/icons/paths'
import type { useUiStore } from '@/shared/stores/ui'
import type { useAuthStore } from '@/shared/stores/auth'

export type UiStore = ReturnType<typeof useUiStore>
export type AuthStore = ReturnType<typeof useAuthStore>

export interface CommandContext {
  router: Router
  ui: UiStore
  auth: AuthStore
}

export interface CommandItem {
  id: string
  title: string
  hint?: string
  icon: IconName
  /** synonyms / translations to match against during search */
  keywords?: string[]
  run: (ctx: CommandContext) => void | Promise<void>
}

export interface CommandSection {
  id: 'actions' | 'navigation'
  label: string
  items: CommandItem[]
}
