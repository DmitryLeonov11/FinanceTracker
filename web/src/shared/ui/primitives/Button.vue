<script setup lang="ts">
import { computed } from 'vue'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/shared/lib/cn'
import Icon from '@/shared/ui/icons/Icon.vue'
import type { IconName } from '@/shared/ui/icons/paths'

const button = cva(
  [
    'relative inline-flex items-center justify-center gap-1.5 select-none whitespace-nowrap',
    'rounded-md font-medium transition-[background,color,box-shadow,transform]',
    'duration-[120ms] ease-[var(--ease-out)]',
    'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-accent/50 focus-visible:ring-offset-1 focus-visible:ring-offset-bg',
    'disabled:opacity-50 disabled:cursor-not-allowed disabled:pointer-events-none',
    'active:scale-[0.985]'
  ],
  {
    variants: {
      intent: {
        primary: 'bg-accent text-accent-fg hover:bg-accent-hover shadow-xs',
        secondary: 'bg-surface text-fg border border-border hover:bg-surface-hi hover:border-border-strong shadow-xs',
        ghost: 'bg-transparent text-fg-muted hover:bg-surface-hi hover:text-fg',
        danger: 'bg-danger text-white hover:opacity-90 shadow-xs',
        soft: 'bg-accent-soft text-accent-soft-fg hover:opacity-85'
      },
      size: {
        sm: 'h-7 px-2.5 text-[13px]',
        md: 'h-9 px-3.5 text-[14px]',
        lg: 'h-11 px-5 text-[15px]'
      },
      block: { true: 'w-full', false: '' }
    },
    defaultVariants: { intent: 'primary', size: 'md', block: false }
  }
)

type ButtonVariants = VariantProps<typeof button>

interface Props {
  intent?: ButtonVariants['intent']
  size?: ButtonVariants['size']
  block?: boolean
  loading?: boolean
  disabled?: boolean
  type?: 'button' | 'submit' | 'reset'
  iconLeft?: IconName
  iconRight?: IconName
}

const props = withDefaults(defineProps<Props>(), {
  intent: 'primary',
  size: 'md',
  block: false,
  loading: false,
  disabled: false,
  type: 'button'
})

const cls = computed(() => cn(button({ intent: props.intent, size: props.size, block: props.block })))
const iconSize = computed(() => (props.size === 'sm' ? 14 : props.size === 'lg' ? 18 : 16))
</script>

<template>
  <button :type="type" :class="cls" :disabled="disabled || loading">
    <Icon v-if="loading" name="loader" :size="iconSize" class="animate-spin" />
    <Icon v-else-if="iconLeft" :name="iconLeft" :size="iconSize" />
    <slot />
    <Icon v-if="iconRight && !loading" :name="iconRight" :size="iconSize" />
  </button>
</template>
