import { computed, ref, watch, onMounted, onUnmounted } from 'vue'
import { useUiStore } from '@/shared/stores/ui'

export interface ChartTokens {
  accent: string
  success: string
  danger: string
  info: string
  fg: string
  fgMuted: string
  fgSubtle: string
  border: string
  surface: string
  surfaceHi: string
  /** convenience flag for echarts theme variants */
  isDark: boolean
}

const VAR_NAMES = {
  accent: '--accent',
  success: '--success',
  danger: '--danger',
  info: '--info',
  fg: '--fg',
  fgMuted: '--fg-muted',
  fgSubtle: '--fg-subtle',
  border: '--border',
  surface: '--surface',
  surfaceHi: '--surface-hi'
} as const

function readTokens(isDark: boolean): ChartTokens {
  const styles = getComputedStyle(document.documentElement)
  const read = (cssVar: string) => styles.getPropertyValue(cssVar).trim() || 'transparent'
  return {
    accent: read(VAR_NAMES.accent),
    success: read(VAR_NAMES.success),
    danger: read(VAR_NAMES.danger),
    info: read(VAR_NAMES.info),
    fg: read(VAR_NAMES.fg),
    fgMuted: read(VAR_NAMES.fgMuted),
    fgSubtle: read(VAR_NAMES.fgSubtle),
    border: read(VAR_NAMES.border),
    surface: read(VAR_NAMES.surface),
    surfaceHi: read(VAR_NAMES.surfaceHi),
    isDark
  }
}

/**
 * Reactive ECharts color tokens. Re-reads CSS variables from `:root`
 * whenever the theme flips. Plug the returned `tokens` ref into your
 * echarts option computed and the chart picks up the new theme automatically.
 */
export function useChartTokens() {
  const ui = useUiStore()
  const tokens = ref<ChartTokens>(readTokens(ui.isDark))

  let mounted = false
  onMounted(() => {
    mounted = true
    tokens.value = readTokens(ui.isDark)
  })

  const stop = watch(
    () => ui.isDark,
    (dark) => {
      if (!mounted) return
      // CSS-переменные меняются вместе с data-theme, поэтому ждём следующий кадр
      requestAnimationFrame(() => {
        tokens.value = readTokens(dark)
      })
    }
  )

  onUnmounted(() => stop())

  return computed(() => tokens.value)
}
