import { onUnmounted, ref, watch, type Ref } from 'vue'

export type FlashDirection = 'up' | 'down' | null

interface UseFlashOptions {
  /** сколько миллисекунд держать подсветку (по умолчанию 700) */
  duration?: number
  /** насколько должно измениться значение, чтобы подсветка сработала (по умолчанию 0.005) */
  threshold?: number
}

/**
 * Отдаёт ref, который на короткое время становится 'up' или 'down',
 * когда меняется значение. Через duration сам гаснет обратно в null.
 *
 * Пригодится для CSS-вспышки карточек после refetch, например когда прилетело обновление по SignalR.
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
