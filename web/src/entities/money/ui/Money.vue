<script setup lang="ts">
import { computed, toRef } from 'vue'
import { cn } from '@/shared/lib/cn'
import { splitMoney } from '@/shared/lib/format/money'
import { useTween } from '@/shared/composables/useTween'
import type { CurrencyCode } from '@/shared/config/currencies'

interface Props {
  amount: number
  currency: CurrencyCode
  size?: 'sm' | 'md' | 'lg' | 'xl' | '2xl'
  /** make decimal part visually subdued */
  fadeDecimals?: boolean
  /** show + sign for positive */
  showSign?: boolean
  /** smoothly tween between values on prop change */
  animate?: boolean
  /** tween duration in ms (default: 500) */
  duration?: number
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  fadeDecimals: true,
  showSign: false,
  animate: false,
  duration: 500
})

const amountSource = toRef(props, 'amount')
const animatedAmount = useTween(amountSource, { duration: props.duration })
const displayAmount = computed(() => (props.animate ? animatedAmount.value : props.amount))

const parts = computed(() => splitMoney(displayAmount.value, props.currency))

const sizeCls = computed(() =>
  ({
    sm: 'text-[13px]',
    md: 'text-[14px]',
    lg: 'text-[18px] font-semibold tracking-[-0.005em]',
    xl: 'text-[28px] font-semibold tracking-[-0.015em]',
    '2xl': 'text-[40px] font-semibold tracking-[-0.02em] leading-tight'
  })[props.size]
)

const sign = computed(() =>
  displayAmount.value > 0 && props.showSign ? '+' : parts.value.sign
)
</script>

<template>
  <span :class="cn('num inline-flex items-baseline gap-1', sizeCls)">
    <span v-if="sign" class="text-fg-muted">{{ sign }}</span>
    <span class="text-fg">{{ parts.integer }}</span>
    <span :class="fadeDecimals ? 'text-fg-muted' : 'text-fg'">,{{ parts.decimal }}</span>
    <span class="text-fg-subtle text-[0.7em] font-normal ml-0.5">{{ parts.symbol }}</span>
  </span>
</template>
