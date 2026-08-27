import { ref } from 'vue'
import { peekQueue } from './queue'

export const pendingIds = ref<Set<string>>(new Set())

export const isReplaying = ref(false)

export function refreshPendingIds() {
  pendingIds.value = new Set(peekQueue().map((op) => op.id))
}
