import { defineStore } from 'pinia'
import { computed, ref, watch } from 'vue'
import { useStorage, usePreferredDark } from '@vueuse/core'
import type { Transaction } from '@/entities/transaction/model/schemas'

export type ThemeMode = 'light' | 'dark' | 'system'

export const useUiStore = defineStore('ui', () => {
  const themeMode = useStorage<ThemeMode>('ft-theme', 'system')
  const sidebarCollapsed = useStorage('ft-sidebar-collapsed', false)
  const density = useStorage<'comfortable' | 'compact'>('ft-density', 'comfortable')
  const prefersDark = usePreferredDark()

  const isDark = computed(() =>
    themeMode.value === 'dark' || (themeMode.value === 'system' && prefersDark.value)
  )

  const mobileSidebarOpen = ref(false)
  const commandPaletteOpen = ref(false)
  const createAccountOpen = ref(false)
  const recordTransactionOpen = ref(false)
  const recordTransferOpen = ref(false)
  const createBudgetOpen = ref(false)
  const editingTransaction = ref<Transaction | null>(null)
  const deletingTransaction = ref<Transaction | null>(null)
  const renamingAccountId = ref<string | null>(null)
  const recordTransactionPrefillAccountId = ref<string | null>(null)
  const recordTransferPrefillSourceId = ref<string | null>(null)

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

  function toggleMobileSidebar() {
    mobileSidebarOpen.value = !mobileSidebarOpen.value
  }

  function closeMobileSidebar() {
    mobileSidebarOpen.value = false
  }

  function toggleCommandPalette() {
    commandPaletteOpen.value = !commandPaletteOpen.value
  }

  function openCreateAccount() {
    commandPaletteOpen.value = false
    createAccountOpen.value = true
  }

  function openRecordTransaction(prefillAccountId?: string) {
    commandPaletteOpen.value = false
    recordTransactionPrefillAccountId.value = prefillAccountId ?? null
    recordTransactionOpen.value = true
  }

  function closeRecordTransaction() {
    recordTransactionOpen.value = false
    recordTransactionPrefillAccountId.value = null
  }

  function openRecordTransfer(prefillSourceId?: string) {
    commandPaletteOpen.value = false
    recordTransferPrefillSourceId.value = prefillSourceId ?? null
    recordTransferOpen.value = true
  }

  function closeRecordTransfer() {
    recordTransferOpen.value = false
    recordTransferPrefillSourceId.value = null
  }

  function openRenameAccount(id: string) {
    commandPaletteOpen.value = false
    renamingAccountId.value = id
  }

  function closeRenameAccount() {
    renamingAccountId.value = null
  }

  function openCreateBudget() {
    commandPaletteOpen.value = false
    createBudgetOpen.value = true
  }

  function closeCreateBudget() {
    createBudgetOpen.value = false
  }

  function openEditTransaction(tx: Transaction) {
    commandPaletteOpen.value = false
    editingTransaction.value = tx
  }

  function closeEditTransaction() {
    editingTransaction.value = null
  }

  function confirmDeleteTransaction(tx: Transaction) {
    commandPaletteOpen.value = false
    deletingTransaction.value = tx
  }

  function dismissDeleteTransaction() {
    deletingTransaction.value = null
  }

  function cycleTheme() {
    const order: ThemeMode[] = ['system', 'light', 'dark']
    themeMode.value = order[(order.indexOf(themeMode.value) + 1) % order.length] ?? 'system'
  }

  return {
    themeMode,
    isDark,
    sidebarCollapsed,
    mobileSidebarOpen,
    density,
    commandPaletteOpen,
    createAccountOpen,
    recordTransactionOpen,
    recordTransferOpen,
    createBudgetOpen,
    editingTransaction,
    deletingTransaction,
    renamingAccountId,
    recordTransactionPrefillAccountId,
    recordTransferPrefillSourceId,
    setTheme,
    toggleSidebar,
    toggleMobileSidebar,
    closeMobileSidebar,
    toggleCommandPalette,
    openCreateAccount,
    openRecordTransaction,
    closeRecordTransaction,
    openRecordTransfer,
    closeRecordTransfer,
    openCreateBudget,
    closeCreateBudget,
    openRenameAccount,
    closeRenameAccount,
    openEditTransaction,
    closeEditTransaction,
    confirmDeleteTransaction,
    dismissDeleteTransaction,
    cycleTheme
  }
})
