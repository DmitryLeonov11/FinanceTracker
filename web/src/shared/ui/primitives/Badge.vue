<script setup lang="ts">
import { computed } from 'vue'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/shared/lib/cn'

const badge = cva(
  [
    'inline-flex items-center gap-1 rounded-md border px-1.5 py-0.5',
    'text-[11px] font-medium leading-none whitespace-nowrap'
  ],
  {
    variants: {
      tone: {
        neutral: 'border-border bg-surface-hi text-fg-muted',
        accent: 'border-transparent bg-accent-soft text-accent-soft-fg',
        success: 'border-transparent bg-success-soft text-success',
        warning: 'border-transparent bg-warning-soft text-warning',
        danger: 'border-transparent bg-danger-soft text-danger',
        info: 'border-transparent bg-info-soft text-info'
      }
    },
    defaultVariants: { tone: 'neutral' }
  }
)

type BadgeVariants = VariantProps<typeof badge>

interface Props {
  tone?: BadgeVariants['tone']
}

const props = withDefaults(defineProps<Props>(), { tone: 'neutral' })
const cls = computed(() => cn(badge({ tone: props.tone })))
</script>

<template>
  <span :class="cls">
    <slot />
  </span>
</template>
