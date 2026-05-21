import { get, set, del } from 'idb-keyval'
import type { QueuedOp } from './types'

const STORAGE_KEY = 'ft-offline-queue'

let memoryCache: QueuedOp[] | null = null

export async function loadQueue(): Promise<QueuedOp[]> {
  if (memoryCache) return memoryCache
  const raw = await get<QueuedOp[]>(STORAGE_KEY)
  memoryCache = raw ?? []
  return memoryCache
}

async function saveQueue(queue: QueuedOp[]): Promise<void> {
  memoryCache = queue
  if (queue.length === 0) await del(STORAGE_KEY)
  else await set(STORAGE_KEY, queue)
}

export async function enqueue(op: QueuedOp): Promise<void> {
  const q = await loadQueue()
  await saveQueue([...q, op])
}

export async function removeFromQueue(id: string): Promise<void> {
  const q = await loadQueue()
  await saveQueue(q.filter((op) => op.id !== id))
}

export async function updateInQueue(op: QueuedOp): Promise<void> {
  const q = await loadQueue()
  await saveQueue(q.map((existing) => (existing.id === op.id ? op : existing)))
}

export async function clearQueue(): Promise<void> {
  await saveQueue([])
}

export function peekQueue(): QueuedOp[] {
  return memoryCache ?? []
}
