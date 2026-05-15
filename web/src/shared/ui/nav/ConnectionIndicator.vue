<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '@/shared/lib/cn'
import { realtime, HubConnectionState } from '@/shared/api/signalr'

const state = realtime.state

const status = computed(() => {
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
</script>

<template>
  <div
    class="hidden sm:inline-flex items-center gap-1.5 px-2 h-7 rounded-md text-[11.5px] font-medium text-fg-muted hover:bg-surface-hi transition-colors select-none"
    :title="status.label"
    :aria-label="`Realtime: ${status.label}`"
  >
    <span
      :class="cn('size-1.5 rounded-full', dotCls, status.pulse && 'live-pulse')"
      aria-hidden="true"
    />
    <span class="hidden md:inline">{{ status.label }}</span>
  </div>
</template>
