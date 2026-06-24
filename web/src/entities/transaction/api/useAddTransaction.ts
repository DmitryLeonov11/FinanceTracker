import { useMutation, useQueryClient, type QueryClient } from '@tanstack/vue-query'
import { transactionsApi } from './transactionsApi'
import type { AddTransactionCommand, Transaction } from '../model/schemas'
import type { Account } from '@/entities/account/model/schemas'
import type { DashboardBalance } from '@/entities/dashboard/model/schemas'
import { ApiError } from '@/shared/api/errors'
import { isNetworkError } from '@/shared/offline/networkError'
import { enqueue } from '@/shared/offline/queue'
import { refreshPendingIds } from '@/shared/offline/state'
import { replayQueue } from '@/shared/offline/replay'

export interface AddTransactionResult {
  transaction: Transaction
  /** true if request was queued for later send (offline path) */
  queued: boolean
}

function getAccount(qc: QueryClient, accountId: string): Account | undefined {
  const accounts = qc.getQueryData<Account[]>(['accounts'])
  return accounts?.find((a) => a.id === accountId)
}

function applyOptimisticToCache(qc: QueryClient, optimistic: Transaction) {
  // 1) Prepend to every cached transactions page
  qc.setQueriesData<{ items: Transaction[]; total: number; hasMore: boolean }>(
    { queryKey: ['transactions'] },
    (old) => (old ? { ...old, items: [optimistic, ...old.items], total: old.total + 1 } : old)
  )

  // 2) Adjust dashboard balance: shift the relevant account + currency total
  qc.setQueryData<DashboardBalance>(['dashboard', 'balance'], (old) => {
    if (!old) return old
    const delta = optimistic.type === 'Income' ? optimistic.amount : -optimistic.amount
    const accounts = old.accounts.map((a) =>
      a.accountId === optimistic.accountId ? { ...a, balance: a.balance + delta } : a
    )
    const balancesByCurrency = old.balancesByCurrency.map((b) =>
      b.currency === optimistic.currency ? { ...b, total: b.total + delta } : b
    )
    return { ...old, accounts, balancesByCurrency }
  })
}

export function useAddTransaction() {
  const qc = useQueryClient()

  return useMutation({
    mutationFn: async (cmd: AddTransactionCommand): Promise<AddTransactionResult> => {
      const account = getAccount(qc, cmd.accountId)
      const id = crypto.randomUUID()
      const optimistic: Transaction = {
        id,
        accountId: cmd.accountId,
        counterAccountId: null,
        categoryId: cmd.categoryId ?? null,
        type: cmd.type as Transaction['type'],
        amount: cmd.amount,
        currency: account?.currency ?? 'BYN',
        occurredAt: cmd.occurredAt,
        note: cmd.note ?? null
      }

      // Уже офлайн? Тогда сеть даже не трогаем
      if (typeof navigator !== 'undefined' && navigator.onLine === false) {
        await enqueue({
          id,
          type: 'transaction.create',
          payload: cmd,
          optimistic,
          createdAt: Date.now(),
          attempts: 0
        })
        refreshPendingIds()
        applyOptimisticToCache(qc, optimistic)
        return { transaction: optimistic, queued: true }
      }

      try {
        const server = await transactionsApi.create(cmd, id)
        // онлайн и сервер ответил: сбрасываем кэш, он подтянет точные данные
        qc.invalidateQueries({ queryKey: ['transactions'] })
        qc.invalidateQueries({ queryKey: ['dashboard'] })
        qc.invalidateQueries({ queryKey: ['accounts'] })
        // заодно сливаем очередь, если с прошлой сессии что-то осталось
        void replayQueue()
        return { transaction: server, queued: false }
      } catch (err) {
        // первая попытка сорвалась на сетевой ошибке? кладём запись в очередь
        if (isNetworkError(err)) {
          await enqueue({
            id,
            type: 'transaction.create',
            payload: cmd,
            optimistic,
            createdAt: Date.now(),
            attempts: 0
          })
          refreshPendingIds()
          applyOptimisticToCache(qc, optimistic)
          return { transaction: optimistic, queued: true }
        }
        // постоянная ошибка (валидация, доступ, конфликт): пробрасываем как обычно
        if (err instanceof ApiError) throw err
        throw err
      }
    }
  })
}
