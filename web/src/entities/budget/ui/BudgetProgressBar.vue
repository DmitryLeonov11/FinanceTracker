<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '@/shared/lib/cn'

interface Props {
  percent: number
  isOverLimit?: boolean
  height?: number
}

const props = withDefaults(defineProps<Props>(), { isOverLimit: false, height: 8 })

const clampedWidth = computed(() => Math.min(100, Math.max(0, props.percent)))

const fillCls = computed(() => {
  if (props.isOverLimit) return 'bg-danger'
  if (props.percent >= 80) return 'bg-warning'
  if (props.percent >= 50) return 'bg-warning'
  return 'bg-success'
})

const styleVars = computed(() => ({
  height: `${props.height}px`
}))
</script>

<template>
  <div
    class="relative w-full overflow-hidden bg-surface-sunk rounded-pill border border-border"
    :style="styleVars"
    :aria-valuenow="Math.round(percent)"
    aria-valuemin="0"
    aria-valuemax="100"
    role="progressbar"
  >
    <div
      :class="cn('h-full transition-[width] duration-[320ms] ease-[var(--ease-out)]', fillCls)"
      :style="{ width: `${clampedWidth}%` }"
    />
    <div
      v-if="isOverLimit"
      class="absolute right-0 top-0 h-full w-1 bg-danger live-pulse"
      aria-hidden="true"
    />
  </div>
</template>
