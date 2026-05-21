<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '@/shared/lib/cn'
import { realtime, HubConnectionState } from '@/shared/api/signalr'
import { useOfflineQueue } from '@/shared/offline/useOfflineQueue'
import Icon from '@/shared/ui/icons/Icon.vue'

const state = realtime.state
const { pendingCount, isOnline, isReplaying, replayNow } = useOfflineQueue()

const status = computed(() => {
  if (!isOnline.value) return { tone: 'muted', label: 'Оффлайн', pulse: false }
  switch (state.value) {
    case HubConnectionState.Connected:
      return { tone: 'success', label: 'Подключено', pulse: true }
    case HubConnectionState.Reconnecting:
    case HubConnectionState.Connecting:
      return { tone: 'warning', label: 'Переподключение…', pulse: false }
    default:
      return { tone: 'muted', label: 'Не в сети', pulse: false }
  }
})

const dotCls = computed(() =>
  ({
    success: 'bg-success',
    warning: 'bg-warning',
    muted: 'bg-fg-subtle'
  })[status.value.tone]
)

const queueLabel = computed(() => {
  const n = pendingCount.value
  if (n === 0) return ''
  return `${n} ${n === 1 ? 'ожидает' : n < 5 ? 'ожидают' : 'ожидают'}`
})

const onClick = () => {
  if (pendingCount.value > 0 && isOnline.value) {
    void replayNow()
  }
}
</script>

<template>
  <div class="inline-flex items-center gap-1">
    <button
      type="button"
      class="hidden sm:inline-flex items-center gap-1.5 px-2 h-7 rounded-md text-[11.5px] font-medium text-fg-muted hover:bg-surface-hi transition-colors select-none"
      :title="status.label"
      :aria-label="`Realtime: ${status.label}`"
      :disabled="pendingCount === 0"
      @click="onClick"
    >
      <span
        :class="cn('size-1.5 rounded-full', dotCls, status.pulse && 'live-pulse')"
        aria-hidden="true"
      />
      <span class="hidden md:inline">{{ status.label }}</span>
    </button>

    <button
      v-if="pendingCount > 0"
      type="button"
      class="inline-flex items-center gap-1 px-1.5 h-6 rounded-md bg-warning-soft text-warning text-[11px] font-medium hover:opacity-85 transition-opacity"
      :title="isOnline ? 'Отправить ожидающие операции сейчас' : 'Ждём подключения'"
      :disabled="!isOnline || isReplaying"
      @click="onClick"
    >
      <Icon name="loader" :size="11" :class="isReplaying && 'animate-spin'" />
      {{ queueLabel }}
    </button>
  </div>
</template>
