import { defineStore } from 'pinia'
import { computed, ref, watch } from 'vue'
import { useStorage, usePreferredDark } from '@vueuse/core'

export type ThemeMode = 'light' | 'dark' | 'system'

export const useUiStore = defineStore('ui', () => {
  const themeMode = useStorage<ThemeMode>('ft-theme', 'system')
  const sidebarCollapsed = useStorage('ft-sidebar-collapsed', false)
  const density = useStorage<'comfortable' | 'compact'>('ft-density', 'comfortable')
  const prefersDark = usePreferredDark()

  const isDark = computed(() =>
    themeMode.value === 'dark' || (themeMode.value === 'system' && prefersDark.value)
  )

  const commandPaletteOpen = ref(false)

  watch(
    isDark,
    (dark) => {
      document.documentElement.dataset.theme = dark ? 'dark' : 'light'
    },
    { immediate: true }
  )

  watch(
    density,
    (d) => {
      document.documentElement.dataset.density = d
    },
    { immediate: true }
  )

  function setTheme(mode: ThemeMode) {
    themeMode.value = mode
  }

  function toggleSidebar() {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  function toggleCommandPalette() {
    commandPaletteOpen.value = !commandPaletteOpen.value
  }

  return {
    themeMode,
    isDark,
    sidebarCollapsed,
    density,
    commandPaletteOpen,
    setTheme,
    toggleSidebar,
    toggleCommandPalette
  }
})
