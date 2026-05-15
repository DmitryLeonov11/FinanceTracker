import { onUnmounted, ref, watch, type Ref } from 'vue'

export type FlashDirection = 'up' | 'down' | null

interface UseFlashOptions {
  /** длительность подсветки в мс (default: 700) */
  duration?: number
  /** минимальный |delta|, при котором подсветка срабатывает (default: 0.005) */
  threshold?: number
}

/**
 * Возвращает ref, который кратковременно становится 'up' или 'down'
 * при изменении наблюдаемого значения. После duration сам сбрасывается в null.
 *
 * Используется для CSS-флэша карточек после refetch (например, при SignalR-обновлении).
 */
export function useFlash(source: Ref<number>, options: UseFlashOptions = {}): Ref<FlashDirection> {
  const { duration = 700, threshold = 0.005 } = options
  const flash = ref<FlashDirection>(null)
  let timer: ReturnType<typeof setTimeout> | null = null

  let prev = source.value

  watch(source, (next) => {
    const delta = next - prev
    prev = next
    if (Math.abs(delta) < threshold) return

    flash.value = delta > 0 ? 'up' : 'down'
    if (timer) clearTimeout(timer)
    timer = setTimeout(() => {
      flash.value = null
      timer = null
    }, duration)
  })

  onUnmounted(() => {
    if (timer) clearTimeout(timer)
  })

  return flash
}
