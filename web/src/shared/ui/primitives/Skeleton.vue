<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '@/shared/lib/cn'

interface Props {
  width?: string | number
  height?: string | number
  rounded?: 'sm' | 'md' | 'lg' | 'pill'
}

const props = withDefaults(defineProps<Props>(), {
  rounded: 'md'
})

const style = computed(() => ({
  width: typeof props.width === 'number' ? `${props.width}px` : props.width,
  height: typeof props.height === 'number' ? `${props.height}px` : props.height
}))

const cls = computed(() =>
  cn(
    'relative overflow-hidden bg-surface-hi',
    'before:absolute before:inset-0 before:-translate-x-full before:animate-[shimmer_1.4s_infinite]',
    'before:bg-[linear-gradient(90deg,transparent,oklch(from_var(--surface-hi)_calc(l+0.04)_c_h_/_0.8),transparent)]',
    {
      sm: 'rounded-sm',
      md: 'rounded-md',
      lg: 'rounded-lg',
      pill: 'rounded-full'
    }[props.rounded]
  )
)
</script>

<template>
  <div :class="cls" :style="style" />
</template>

<style>
@keyframes shimmer {
  100% { transform: translateX(100%); }
}
</style>
