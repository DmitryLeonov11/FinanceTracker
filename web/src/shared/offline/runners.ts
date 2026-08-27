import { transactionsApi } from '@/entities/transaction/api/transactionsApi'
import type { QueuedOp } from './types'

export const runners: { [K in QueuedOp['type']]: (op: Extract<QueuedOp, { type: K }>) => Promise<void> } = {
  'transaction.create': async (op) => {
    await transactionsApi.create(op.payload, op.id)
  }
}
