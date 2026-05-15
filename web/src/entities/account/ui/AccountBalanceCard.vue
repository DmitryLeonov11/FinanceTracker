<script setup lang="ts">
import { toRef } from 'vue'
import { Card } from '@/shared/ui/primitives'
import Money from '@/entities/money/ui/Money.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import type { IconName } from '@/shared/ui/icons/paths'
import { useFlash } from '@/shared/composables/useFlash'
import { cn } from '@/shared/lib/cn'

interface Props {
  accountId: string
  name: string
  currency: string
  balance: number
  icon?: IconName
}

const props = withDefaults(defineProps<Props>(), { icon: 'wallet' })

const balanceRef = toRef(props, 'balance')
const flash = useFlash(balanceRef)
</script>

<template>
  <Card
    variant="flat"
    interactive
    :class="
      cn(
        'group rounded-lg',
        flash === 'up' && 'flash-up',
        flash === 'down' && 'flash-down'
      )
    "
  >
    <div class="flex items-start justify-between gap-3">
      <div class="flex items-center gap-3 min-w-0">
        <div
          class="size-9 shrink-0 rounded-md bg-surface-hi text-fg-muted flex items-center justify-center group-hover:text-accent group-hover:bg-accent-soft transition-colors"
        >
          <Icon :name="icon" :size="18" />
        </div>
        <div class="min-w-0">
          <p class="font-medium text-fg truncate">{{ name }}</p>
          <p class="text-[12px] text-fg-subtle uppercase tracking-wider mt-0.5">
            {{ currency }}
          </p>
        </div>
      </div>
      <Icon name="chevronRight" :size="16" class="text-fg-subtle mt-1 shrink-0" />
    </div>
    <div class="mt-4">
      <Money :amount="balance" :currency="currency as any" size="lg" animate :duration="500" />
    </div>
  </Card>
</template>
