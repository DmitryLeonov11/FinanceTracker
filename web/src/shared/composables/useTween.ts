import { onUnmounted, ref, watch, type Ref } from 'vue'

const prefersReducedMotion = () =>
  typeof window !== 'undefined' &&
  window.matchMedia?.('(prefers-reduced-motion: reduce)').matches

// ease-out-cubic, основная кривая для анимаций интерфейса
const easeOutCubic = (t: number) => 1 - Math.pow(1 - t, 3)

interface UseTweenOptions {
  /** сколько миллисекунд идёт анимация (по умолчанию 500) */
  duration?: number
  /** не анимировать самое первое значение */
  skipFirst?: boolean
}

/**
 * Делает из реактивного числа плавно меняющееся.
 * Когда `source` меняется, число едет от текущего показанного к новому
 * за `duration` мс по кривой easeOutCubic.
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
