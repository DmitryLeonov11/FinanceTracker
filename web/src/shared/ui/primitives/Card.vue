<script setup lang="ts">
import { computed } from 'vue'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/shared/lib/cn'

const card = cva(['rounded-lg border bg-surface'], {
  variants: {
    variant: {
      flat: 'border-border shadow-none',
      elevated: 'border-border shadow-sm',
      outlined: 'border-border-strong shadow-none'
    },
    padding: {
      none: '',
      sm: 'p-3',
      md: 'p-5',
      lg: 'p-6'
    },
    interactive: {
      true: 'transition-shadow duration-150 hover:shadow-md cursor-pointer',
      false: ''
    }
  },
  defaultVariants: { variant: 'flat', padding: 'md', interactive: false }
})

type CardVariants = VariantProps<typeof card>

interface Props {
  variant?: CardVariants['variant']
  padding?: CardVariants['padding']
  interactive?: boolean
  as?: string
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'flat',
  padding: 'md',
  interactive: false,
  as: 'div'
})

const cls = computed(() => cn(card({ variant: props.variant, padding: props.padding, interactive: props.interactive })))
</script>

<template>
  <component :is="as" :class="cls">
    <slot />
  </component>
</template>
