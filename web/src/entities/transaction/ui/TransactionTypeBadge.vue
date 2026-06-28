<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '@/shared/lib/cn'
import Icon from '@/shared/ui/icons/Icon.vue'
import type { IconName } from '@/shared/ui/icons/paths'
import type { TransactionType } from '../model/schemas'

interface Props {
  type: TransactionType
  size?: 'sm' | 'md'
}

const props = withDefaults(defineProps<Props>(), { size: 'md' })

const META: Record<TransactionType, { icon: IconName; cls: string; label: string }> = {
  Income: { icon: 'arrowDown', cls: 'bg-success-soft text-success', label: 'Доход' },
  Expense: { icon: 'arrowUp', cls: 'bg-danger-soft text-danger', label: 'Расход' },
  Transfer: { icon: 'swap', cls: 'bg-info-soft text-info', label: 'Перевод' }
}

const meta = computed(() => META[props.type])

const dim = computed(() => (props.size === 'sm' ? 'size-7' : 'size-9'))
const iconSize = computed(() => (props.size === 'sm' ? 14 : 16))
</script>

<template>
  <div
    :class="cn('shrink-0 rounded-md flex items-center justify-center', dim, meta.cls)"
    :title="meta.label"
    :aria-label="meta.label"
  >
    <Icon :name="meta.icon" :size="iconSize" />
  </div>
</template>
