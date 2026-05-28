<script setup lang="ts">
import { computed } from 'vue'
import { DropdownMenuRoot, DropdownMenuTrigger, DropdownMenuPortal, DropdownMenuContent, DropdownMenuItem } from 'reka-ui'
import { Card, Badge } from '@/shared/ui/primitives'
import Money from '@/entities/money/ui/Money.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import BudgetProgressBar from './BudgetProgressBar.vue'
import { BUDGET_PERIOD_LABELS, type BudgetWithProgress } from '../model/schemas'
import { fmtDate } from '@/shared/lib/format/date'

interface Props {
  budget: BudgetWithProgress
}

const props = defineProps<Props>()
const emit = defineEmits<{ edit: [budget: BudgetWithProgress]; close: [budget: BudgetWithProgress] }>()

const periodLabel = computed(() => BUDGET_PERIOD_LABELS[props.budget.period])

const footerText = computed(() => {
  if (props.budget.isOverLimit) {
    const over = Math.abs(props.budget.remaining)
    return { tone: 'danger' as const, text: `Превышено на ${over.toFixed(2).replace('.', ',')} ${props.budget.currency}` }
  }
  return { tone: 'muted' as const, text: `до ${fmtDate(props.budget.currentWindowTo)}` }
})
</script>

<template>
  <Card variant="flat" padding="md" class="flex flex-col gap-3.5">
    <header class="flex items-start justify-between gap-3">
      <div class="min-w-0">
        <div class="flex items-center gap-2 mb-1">
          <p class="font-medium text-fg truncate">{{ budget.name }}</p>
          <Badge tone="neutral">{{ periodLabel }}</Badge>
          <Badge v-if="budget.isClosed" tone="warning">закрыт</Badge>
        </div>
        <p class="text-[12px] text-fg-subtle uppercase tracking-wider">
          {{ budget.currency }}
        </p>
      </div>

      <DropdownMenuRoot>
        <DropdownMenuTrigger
          class="size-7 rounded-md inline-flex items-center justify-center text-fg-subtle hover:text-fg hover:bg-surface-hi transition-colors"
          aria-label="Действия"
        >
          <Icon name="menu" :size="14" />
        </DropdownMenuTrigger>
        <DropdownMenuPortal>
          <DropdownMenuContent
            class="z-40 min-w-[160px] bg-surface border border-border rounded-md shadow-md p-1 text-[13px] data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:fade-in data-[state=closed]:fade-out"
            :side-offset="4"
            align="end"
          >
            <DropdownMenuItem
              v-if="!budget.isClosed"
              class="flex items-center gap-2 h-8 px-2 rounded text-fg-muted hover:bg-surface-hi hover:text-fg cursor-pointer focus:outline-none focus:bg-surface-hi"
              @select="emit('edit', budget)"
            >
              <Icon name="cog" :size="14" />
              Изменить
            </DropdownMenuItem>
            <DropdownMenuItem
              v-if="!budget.isClosed"
              class="flex items-center gap-2 h-8 px-2 rounded text-danger hover:bg-danger-soft cursor-pointer focus:outline-none focus:bg-danger-soft"
              @select="emit('close', budget)"
            >
              <Icon name="x" :size="14" />
              Закрыть бюджет
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenuPortal>
      </DropdownMenuRoot>
    </header>

    <div class="flex items-baseline justify-between gap-2">
      <Money :amount="budget.spent" :currency="budget.currency" size="lg" animate />
      <span class="text-[13px] text-fg-subtle">
        из <Money :amount="budget.limit" :currency="budget.currency" size="sm" :fade-decimals="false" />
      </span>
    </div>

    <BudgetProgressBar :percent="budget.progressPercent" :is-over-limit="budget.isOverLimit" />

    <footer class="flex items-center justify-between text-[12px]">
      <span :class="footerText.tone === 'danger' ? 'text-danger font-medium' : 'text-fg-subtle'">
        {{ footerText.text }}
      </span>
      <span class="text-fg-muted num">{{ Math.round(budget.progressPercent) }}%</span>
    </footer>
  </Card>
</template>
