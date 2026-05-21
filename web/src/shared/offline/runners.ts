import { transactionsApi } from '@/entities/transaction/api/transactionsApi'
import type { QueuedOp } from './types'

/** Map of op kind → executor. Each runner must propagate ApiError on failure. */
export const runners: { [K in QueuedOp['type']]: (op: Extract<QueuedOp, { type: K }>) => Promise<void> } = {
  'transaction.create': async (op) => {
    await transactionsApi.create(op.payload, op.id)
  }
}
