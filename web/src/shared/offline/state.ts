import { ref } from 'vue'
import { peekQueue } from './queue'

/** Reactive set of queued op ids; readers use `pendingIds.value.has(...)`. */
export const pendingIds = ref<Set<string>>(new Set())

/** True while replayQueue() is executing. */
export const isReplaying = ref(false)

/** Recompute pendingIds from the (in-memory) queue snapshot. Call after enqueue/remove. */
export function refreshPendingIds() {
  pendingIds.value = new Set(peekQueue().map((op) => op.id))
}
