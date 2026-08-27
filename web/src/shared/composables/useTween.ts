import { onUnmounted, ref, watch, type Ref } from 'vue'

const prefersReducedMotion = () =>
  typeof window !== 'undefined' &&
  window.matchMedia?.('(prefers-reduced-motion: reduce)').matches

// ease-out-cubic — наша основная кривая для UI-анимаций
const easeOutCubic = (t: number) => 1 - Math.pow(1 - t, 3)

interface UseTweenOptions {
  /** длительность анимации в мс (default: 500) */
  duration?: number
  /** не анимировать первое появление значения */
  skipFirst?: boolean
}

/**
 * Превращает реактивное число в плавно меняющееся.
 * При изменении `source` анимирует от текущего отрисованного значения
 * к новому за `duration` мс через easeOutCubic.
 */
export function useTween(source: Ref<number>, options: UseTweenOptions = {}): Ref<number> {
  const { duration = 500, skipFirst = true } = options
  const display = ref<number>(source.value)
  let raf = 0
  let cancelled = false

  function animate(from: number, to: number) {
    if (raf) cancelAnimationFrame(raf)

    if (prefersReducedMotion() || duration <= 0) {
      display.value = to
      return
    }

    const start = performance.now()
    const delta = to - from

    function step(now: number) {
      if (cancelled) return
      const t = Math.min(1, (now - start) / duration)
      display.value = from + delta * easeOutCubic(t)
      if (t < 1) {
        raf = requestAnimationFrame(step)
      } else {
        display.value = to
        raf = 0
      }
    }

    raf = requestAnimationFrame(step)
  }

  let first = true
  watch(
    source,
    (next, prev) => {
      if (first && skipFirst) {
        first = false
        display.value = next
        return
      }
      first = false
      animate(prev ?? display.value, next)
    },
    { immediate: false }
  )

  onUnmounted(() => {
    cancelled = true
    if (raf) cancelAnimationFrame(raf)
  })

  return display
}
