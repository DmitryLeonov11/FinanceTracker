<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import {
  DialogRoot,
  DialogPortal,
  DialogOverlay,
  DialogContent,
  DialogTitle,
  DialogDescription,
  ComboboxRoot,
  ComboboxAnchor,
  ComboboxInput,
  ComboboxContent,
  ComboboxViewport,
  ComboboxGroup,
  ComboboxLabel,
  ComboboxItem,
  ComboboxEmpty
} from 'reka-ui'

import { useUiStore } from '@/shared/stores/ui'
import { useAuthStore } from '@/shared/stores/auth'
import { useGlobalShortcut } from '@/shared/composables/useGlobalShortcut'
import Icon from '@/shared/ui/icons/Icon.vue'
import { cn } from '@/shared/lib/cn'

import { commandSections, buildSearchHay } from './commands'
import type { CommandContext } from './types'

const ui = useUiStore()
const auth = useAuthStore()
const router = useRouter()

const query = ref('')
const searchHay = buildSearchHay()

useGlobalShortcut('k', () => {
  ui.toggleCommandPalette()
})

const filteredSections = computed(() => {
  const q = query.value.trim().toLowerCase()
  if (!q) return commandSections
  return commandSections
    .map((section) => ({
      ...section,
      items: section.items.filter((item) => (searchHay.get(item.id) ?? '').includes(q))
    }))
    .filter((section) => section.items.length > 0)
})

const hasResults = computed(() => filteredSections.value.length > 0)

const ctx: CommandContext = { router, ui, auth }

function runById(id: string | null | undefined) {
  if (!id) return
  for (const section of commandSections) {
    const item = section.items.find((i) => i.id === id)
    if (item) {
      ui.commandPaletteOpen = false
      // microtask to let dialog start closing before navigation
      void Promise.resolve().then(() => item.run(ctx))
      return
    }
  }
}

watch(
  () => ui.commandPaletteOpen,
  (open) => {
    if (!open) query.value = ''
  }
)
</script>

<template>
  <DialogRoot v-model:open="ui.commandPaletteOpen">
    <DialogPortal>
      <DialogOverlay
        class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out data-[state=open]:fade-in"
      />
      <DialogContent
        class="fixed top-[18vh] left-1/2 -translate-x-1/2 z-50 w-[min(640px,calc(100vw-32px))] bg-surface border border-border rounded-xl shadow-xl overflow-hidden focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:fade-in data-[state=open]:zoom-in-95 data-[state=closed]:zoom-out-95 data-[state=closed]:fade-out"
      >
        <DialogTitle class="sr-only">Командная палитра</DialogTitle>
        <DialogDescription class="sr-only">
          Быстрый поиск действий и навигация по приложению.
        </DialogDescription>

        <ComboboxRoot
          :model-value="null"
          :open="true"
          @update:model-value="(v: unknown) => runById(typeof v === 'string' ? v : null)"
        >
          <ComboboxAnchor class="flex items-center gap-3 h-12 px-4 border-b border-border">
            <Icon name="search" :size="18" class="text-fg-subtle shrink-0" />
            <ComboboxInput
              v-model="query"
              placeholder="Что вы хотите сделать?"
              class="flex-1 bg-transparent outline-none text-[14px] text-fg placeholder:text-fg-subtle"
              auto-focus
            />
            <kbd
              class="hidden sm:inline-flex items-center px-1.5 h-5 rounded-sm border border-border bg-surface-hi text-[10px] text-fg-subtle uppercase tracking-wider"
            >
              esc
            </kbd>
          </ComboboxAnchor>

          <ComboboxContent
            class="max-h-[60vh] overflow-y-auto scrollbar-thin"
            :side-offset="0"
          >
            <ComboboxViewport class="py-2">
              <ComboboxEmpty class="px-4 py-8 text-center text-[13px] text-fg-muted">
                По запросу <span class="text-fg">«{{ query }}»</span> ничего не найдено
              </ComboboxEmpty>

              <ComboboxGroup
                v-for="section in filteredSections"
                :key="section.id"
                class="py-1"
              >
                <ComboboxLabel
                  class="block px-3 pt-2 pb-1 text-[11px] uppercase tracking-wider font-medium text-fg-subtle"
                >
                  {{ section.label }}
                </ComboboxLabel>
                <ComboboxItem
                  v-for="item in section.items"
                  :key="item.id"
                  :value="item.id"
                  class="group/item flex items-center gap-3 mx-1 px-2.5 h-10 rounded-md cursor-pointer outline-none data-[highlighted]:bg-surface-hi"
                >
                  <span
                    :class="
                      cn(
                        'size-7 shrink-0 rounded-md flex items-center justify-center transition-colors',
                        'bg-surface-hi text-fg-muted',
                        'group-data-[highlighted]/item:bg-accent-soft group-data-[highlighted]/item:text-accent-soft-fg'
                      )
                    "
                  >
                    <Icon :name="item.icon" :size="15" />
                  </span>
                  <span class="text-[14px] text-fg flex-1 truncate">{{ item.title }}</span>
                  <span v-if="item.hint" class="text-[12px] text-fg-subtle hidden sm:inline truncate">
                    {{ item.hint }}
                  </span>
                </ComboboxItem>
              </ComboboxGroup>
            </ComboboxViewport>
          </ComboboxContent>
        </ComboboxRoot>

        <footer
          class="h-9 px-4 border-t border-border bg-surface-sunk flex items-center gap-4 text-[11px] text-fg-subtle"
        >
          <span class="inline-flex items-center gap-1.5">
            <kbd class="px-1.5 h-4 rounded-sm border border-border bg-surface text-[10px]">↑↓</kbd>
            навигация
          </span>
          <span class="inline-flex items-center gap-1.5">
            <kbd class="px-1.5 h-4 rounded-sm border border-border bg-surface text-[10px]">↵</kbd>
            выбрать
          </span>
          <span v-if="!hasResults" class="ml-auto text-fg-muted">—</span>
          <span v-else class="ml-auto inline-flex items-center gap-1.5">
            <kbd class="px-1.5 h-4 rounded-sm border border-border bg-surface text-[10px]">esc</kbd>
            закрыть
          </span>
        </footer>
      </DialogContent>
    </DialogPortal>
  </DialogRoot>
</template>
