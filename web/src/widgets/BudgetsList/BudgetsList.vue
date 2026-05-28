<script setup lang="ts">
import { Card, Skeleton, Button } from '@/shared/ui/primitives'
import BudgetCard from '@/entities/budget/ui/BudgetCard.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import type { BudgetWithProgress } from '@/entities/budget/model/schemas'

interface Props {
  budgets: BudgetWithProgress[] | undefined
  isLoading: boolean
  onCreate?: () => void
}

defineProps<Props>()
const emit = defineEmits<{
  edit: [budget: BudgetWithProgress]
  close: [budget: BudgetWithProgress]
}>()
</script>

<template>
  <section>
    <div v-if="isLoading" class="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
      <Skeleton v-for="n in 3" :key="n" height="160" rounded="lg" />
    </div>

    <Card
      v-else-if="!budgets || budgets.length === 0"
      variant="outlined"
      padding="lg"
      class="text-center border-dashed"
    >
      <div class="mx-auto size-12 rounded-full bg-surface-hi flex items-center justify-center mb-3">
        <Icon name="pieChart" :size="20" class="text-fg-subtle" />
      </div>
      <p class="font-medium text-fg">Активных бюджетов нет</p>
      <p class="text-[13px] text-fg-muted mt-1 mb-4">
        Бюджет помогает увидеть, сколько денег осталось до конца периода.
      </p>
      <Button icon-left="plus" @click="onCreate?.()">Создать бюджет</Button>
    </Card>

    <div v-else class="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
      <BudgetCard
        v-for="b in budgets"
        :key="b.id"
        :budget="b"
        @edit="emit('edit', $event)"
        @close="emit('close', $event)"
      />
    </div>
  </section>
</template>
