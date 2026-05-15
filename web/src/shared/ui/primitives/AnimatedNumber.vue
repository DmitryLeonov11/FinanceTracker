<script setup lang="ts">
import { toRef } from 'vue'
import { useTween } from '@/shared/composables/useTween'

interface Props {
  value: number
  duration?: number
  /** number of decimals to show (default: 2) */
  decimals?: number
  /** locale for grouping & decimal separator (default: ru-RU) */
  locale?: string
}

const props = withDefaults(defineProps<Props>(), {
  duration: 500,
  decimals: 2,
  locale: 'ru-RU'
})

const source = toRef(props, 'value')
const animated = useTween(source, { duration: props.duration })

const fmt = new Intl.NumberFormat(props.locale, {
  minimumFractionDigits: props.decimals,
  maximumFractionDigits: props.decimals
})
</script>

<template>
  <span class="num tabular-nums">{{ fmt.format(animated) }}</span>
</template>
