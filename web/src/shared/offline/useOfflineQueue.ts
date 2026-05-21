import { computed, onMounted } from 'vue'
import { useOnline } from '@vueuse/core'
import { loadQueue } from './queue'
import { pendingIds, refreshPendingIds, isReplaying } from './state'
import { replayQueue } from './replay'

let bootstrapped = false

/** Composable for components: reactive offline-queue stats + manual replay. */
export function useOfflineQueue() {
  const isOnline = useOnline()

  onMounted(async () => {
    if (bootstrapped) return
    bootstrapped = true
    // hydrate the in-memory queue and reactive ids on first mount
    await loadQueue()
    refreshPendingIds()
  })

  return {
    pendingIds,
    pendingCount: computed(() => pendingIds.value.size),
    isOnline,
    isReplaying,
    replayNow: replayQueue
  }
}

export { replayQueue }
