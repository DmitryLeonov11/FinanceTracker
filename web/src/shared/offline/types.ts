import type { AddTransactionCommand, Transaction } from '@/entities/transaction/model/schemas'

export interface QueuedOpBase {
  id: string              // becomes Idempotency-Key on send
  createdAt: number
  attempts: number
  lastError?: string
  retryAfter?: number     // epoch ms
}

export interface QueuedTransactionCreate extends QueuedOpBase {
  type: 'transaction.create'
  payload: AddTransactionCommand
  /** Pre-built record to inject into Vue Query cache and dashboard balance. */
  optimistic: Transaction
}

export type QueuedOp = QueuedTransactionCreate

export type OpKind = QueuedOp['type']
