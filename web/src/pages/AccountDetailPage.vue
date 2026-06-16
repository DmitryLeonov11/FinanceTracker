<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter, RouterLink } from 'vue-router'
import { toast } from 'vue-sonner'
import {
  AlertDialogRoot,
  AlertDialogPortal,
  AlertDialogOverlay,
  AlertDialogContent,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogCancel,
  AlertDialogAction
} from 'reka-ui'

import { Card, Button, Skeleton, Badge } from '@/shared/ui/primitives'
import Money from '@/entities/money/ui/Money.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import TransactionsTable from '@/widgets/TransactionsTable/TransactionsTable.vue'
import AccountBalanceSparkline from '@/widgets/AccountBalanceSparkline/AccountBalanceSparkline.vue'
import RenameAccountDialog from '@/features/account/rename-account/RenameAccountDialog.vue'

import { useAccount } from '@/entities/account/api/useAccount'
import { useAccounts } from '@/entities/account/api/useAccounts'
import { useArchiveAccount } from '@/entities/account/api/useArchiveAccount'
import { useTransactions } from '@/entities/transaction/api/useTransactions'
import { useUiStore } from '@/shared/stores/ui'
import { useRealtimeInvalidation } from '@/shared/composables/useRealtimeInvalidation'
import { useFlash } from '@/shared/composables/useFlash'

import { fmtDate } from '@/shared/lib/format/date'
import { ApiError } from '@/shared/api/errors'
import { cn } from '@/shared/lib/cn'
import { routes } from '@/shared/config/routes'
import type { AccountType } from '@/entities/account/model/schemas'
import type { IconName } from '@/shared/ui/icons/paths'

useRealtimeInvalidation()

const route = useRoute()
const router = useRouter()
const ui = useUiStore()

const accountId = computed(() => route.params.id as string)
const { data: account, isPending, isError } = useAccount(accountId)
const { data: accountsData } = useAccounts()

const balanceRef = computed(() => account.value?.balance ?? 0)
const flash = useFlash(balanceRef)

const filters = computed(() => ({
  accountIds: [accountId.value],
  page: 1,
  pageSize: 20
}))
const {
  data: txData,
  isPending: txPending,
  isFetching: txFetching
} = useTransactions(filters)

const TYPE_LABELS: Record<AccountType, string> = {
  Cash: 'Наличные',
  Bank: 'Банковский',
  Card: 'Карта',
  Crypto: 'Крипто',
  Other: 'Прочее'
}

const TYPE_ICONS: Record<AccountType, IconName> = {
  Cash: 'wallet',
  Bank: 'card',
  Card: 'card',
  Crypto: 'wallet',
  Other: 'wallet'
}

const archiveConfirmOpen = ref(false)
const { mutateAsync: archiveAccount, isPending: isArchiving } = useArchiveAccount()

async function confirmArchive() {
  if (!account.value) return
  try {
    await archiveAccount(account.value.id)
    toast.success(`Счёт «${account.value.name}» отправлен в архив`)
    archiveConfirmOpen.value = false
    await router.push(routes.accounts())
  } catch (err) {
    if (err instanceof ApiError) toast.error(err.message)
    else toast.error('Не удалось заархивировать счёт')
  }
}

function quickIncome() {
  ui.openRecordTransaction(accountId.value)
  // RecordTransactionDialog default type is Expense — page-level pre-set is best-effort
}

function quickExpense() {
  ui.openRecordTransaction(accountId.value)
}

function quickTransfer() {
  ui.openRecordTransfer(accountId.value)
}
</script>

<template>
  <div class="space-y-6">
    <!-- back nav -->
    <RouterLink :to="routes.accounts()" class="inline-flex items-center gap-1.5 text-[13px] text-fg-muted hover:text-fg transition-colors">
      <Icon name="arrowLeft" :size="14" />
      Все счета
    </RouterLink>

    <!-- loading -->
    <div v-if="isPending" class="space-y-4">
      <Skeleton width="320" height="32" />
      <Skeleton width="100%" height="120" rounded="lg" />
    </div>

    <!-- error -->
    <Card v-else-if="isError || !account" variant="outlined" padding="lg" class="border-dashed text-center">
      <Icon name="alert" :size="22" class="mx-auto text-fg-subtle mb-2" />
      <p class="font-medium text-fg">Счёт не найден</p>
      <p class="text-[13px] text-fg-muted mt-1">Возможно, он был удалён или ссылка устарела.</p>
      <RouterLink :to="routes.accounts()" class="inline-block mt-3">
        <Button intent="secondary" icon-left="arrowLeft">К списку счетов</Button>
      </RouterLink>
    </Card>

    <template v-else>
      <!-- Header -->
      <header class="flex items-start justify-between gap-3 flex-wrap">
        <div class="flex items-center gap-3 min-w-0">
          <div class="size-11 rounded-md bg-surface-hi text-fg-muted flex items-center justify-center">
            <Icon :name="TYPE_ICONS[account.type]" :size="22" />
          </div>
          <div class="min-w-0">
            <div class="flex items-center gap-2 mb-0.5">
              <h1 class="text-[22px] font-semibold tracking-[-0.01em] truncate">{{ account.name }}</h1>
              <Badge tone="neutral">{{ TYPE_LABELS[account.type] }}</Badge>
              <Badge v-if="account.isArchived" tone="warning">в архиве</Badge>
            </div>
            <p class="text-[13px] text-fg-subtle">
              {{ account.currency }} · создан {{ fmtDate(account.createdAt) }}
            </p>
          </div>
        </div>
        <div v-if="!account.isArchived" class="flex items-center gap-2">
          <Button intent="ghost" icon-left="cog" @click="ui.openRenameAccount(account.id)">Переименовать</Button>
          <Button intent="ghost" icon-left="x" @click="archiveConfirmOpen = true">В архив</Button>
        </div>
      </header>

      <!-- KPI -->
      <Card
        variant="elevated"
        padding="lg"
        :class="
          cn(
            'overflow-hidden transition-shadow',
            flash === 'up' && 'flash-up',
            flash === 'down' && 'flash-down'
          )
        "
      >
        <p class="text-[13px] text-fg-muted uppercase tracking-wider font-medium">Баланс</p>
        <div class="mt-2">
          <Money :amount="account.balance" :currency="account.currency" size="2xl" animate :duration="600" />
        </div>
      </Card>

      <!-- Balance history sparkline -->
      <Card padding="md">
        <div class="flex items-center justify-between mb-2">
          <h2 class="text-[13px] font-medium text-fg-muted uppercase tracking-wider">Динамика баланса</h2>
          <span class="text-[12px] text-fg-subtle">30 дней</span>
        </div>
        <AccountBalanceSparkline :account-id="account.id" :currency="account.currency" />
      </Card>

      <!-- Quick actions -->
      <div v-if="!account.isArchived" class="flex items-center gap-2 flex-wrap">
        <Button intent="secondary" icon-left="arrowDown" @click="quickIncome">Доход</Button>
        <Button intent="secondary" icon-left="arrowUp" @click="quickExpense">Расход</Button>
        <Button intent="secondary" icon-left="swap" @click="quickTransfer">Перевод</Button>
      </div>

      <!-- Recent transactions -->
      <section>
        <header class="flex items-end justify-between mb-3">
          <h2 class="text-[15px] font-semibold tracking-[-0.005em]">Последние операции</h2>
          <RouterLink :to="`${routes.transactions()}?accountId=${account.id}`" class="text-[13px] text-fg-muted hover:text-fg transition-colors">
            Все →
          </RouterLink>
        </header>

        <TransactionsTable
          :transactions="txData?.items ?? []"
          :accounts="accountsData"
          :is-loading="txPending"
          :is-fetching="txFetching"
          :total="txData?.total ?? 0"
          :has-more="false"
          empty-title="Операций по счёту пока нет"
          :empty-hint="account.isArchived ? 'Счёт в архиве — новых операций не будет.' : 'Создайте первую операцию через кнопки выше.'"
          @load-more="() => {}"
          @create="quickExpense"
        />
      </section>

      <RenameAccountDialog />

      <AlertDialogRoot v-model:open="archiveConfirmOpen">
        <AlertDialogPortal>
          <AlertDialogOverlay class="fixed inset-0 bg-black/40 backdrop-blur-sm z-40 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out data-[state=open]:fade-in" />
          <AlertDialogContent
            class="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-50 w-[min(420px,calc(100vw-32px))] bg-surface border border-border rounded-xl shadow-xl p-6 focus:outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=open]:fade-in data-[state=open]:zoom-in-95 data-[state=closed]:zoom-out-95 data-[state=closed]:fade-out"
          >
            <AlertDialogTitle class="text-[16px] font-semibold tracking-[-0.005em] mb-1">
              Отправить счёт в архив?
            </AlertDialogTitle>
            <AlertDialogDescription class="text-[13px] text-fg-muted">
              «{{ account.name }}» исчезнет из активных списков. Новые операции создать не получится, история и баланс сохранятся.
            </AlertDialogDescription>
            <div class="flex items-center justify-end gap-2 mt-5">
              <AlertDialogCancel as-child>
                <Button intent="ghost" type="button">Отмена</Button>
              </AlertDialogCancel>
              <AlertDialogAction as-child>
                <Button intent="danger" :loading="isArchiving" @click="confirmArchive">В архив</Button>
              </AlertDialogAction>
            </div>
          </AlertDialogContent>
        </AlertDialogPortal>
      </AlertDialogRoot>
    </template>
  </div>
</template>
