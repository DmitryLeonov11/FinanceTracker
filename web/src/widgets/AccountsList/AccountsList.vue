<script setup lang="ts">
import { Card, Skeleton, Button } from '@/shared/ui/primitives'
import AccountBalanceCard from '@/entities/account/ui/AccountBalanceCard.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import { useDashboardBalance } from '@/entities/dashboard/api/useDashboardBalance'

interface Props {
  onCreate?: () => void
}

defineProps<Props>()

const { data, isPending } = useDashboardBalance()
</script>

<template>
  <section>
    <header class="flex items-end justify-between mb-3">
      <h2 class="text-[15px] font-semibold tracking-[-0.005em]">Счета</h2>
      <Button intent="ghost" size="sm" icon-left="plus" @click="onCreate?.()">Новый счёт</Button>
    </header>

    <div v-if="isPending" class="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
      <Skeleton v-for="n in 3" :key="n" height="96" rounded="lg" />
    </div>

    <Card
      v-else-if="!data?.accounts.length"
      variant="outlined"
      padding="lg"
      class="text-center border-dashed"
    >
      <div class="mx-auto size-12 rounded-full bg-surface-hi flex items-center justify-center mb-3">
        <Icon name="wallet" :size="20" class="text-fg-subtle" />
      </div>
      <p class="font-medium text-fg">У вас пока нет счетов</p>
      <p class="text-[13px] text-fg-muted mt-1 mb-4">
        Создайте счёт, чтобы начать учитывать операции и видеть сводку.
      </p>
      <Button icon-left="plus" @click="onCreate?.()">Добавить первый счёт</Button>
    </Card>

    <div v-else class="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
      <AccountBalanceCard
        v-for="account in data.accounts"
        :key="account.accountId"
        :account-id="account.accountId"
        :name="account.name"
        :currency="account.currency"
        :balance="account.balance"
      />
    </div>
  </section>
</template>
