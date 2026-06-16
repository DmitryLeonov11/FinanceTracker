<script setup lang="ts">
import { computed } from 'vue'
import VChart from 'vue-echarts'
import type { EChartsOption } from 'echarts'

import '@/widgets/CashflowChart/echarts-setup'

import { Skeleton } from '@/shared/ui/primitives'
import { useAccountBalanceHistory } from '@/entities/account/api/useAccountBalanceHistory'
import { useChartTokens } from '@/shared/composables/useChartTokens'
import { fmtDate, fmtDayMonth } from '@/shared/lib/format/date'
import { formatMoney } from '@/shared/lib/format/money'
import type { Currency } from '@/shared/api/schemas'

interface Props {
  accountId: string
  currency: Currency
  days?: number
}

const props = withDefaults(defineProps<Props>(), { days: 30 })

const accountIdRef = computed(() => props.accountId)
const daysRef = computed(() => props.days)
const { data, isPending } = useAccountBalanceHistory(accountIdRef, daysRef)

const tokens = useChartTokens()

interface TooltipParam {
  dataIndex: number
  axisValueLabel?: string
}

const chartOption = computed<EChartsOption | null>(() => {
  if (!data.value) return null
  const t = tokens.value
  const dates = data.value.points.map((p) => p.date)
  const balances = data.value.points.map((p) => p.balance)
  const labels = dates.map((d) => fmtDayMonth(d))

  return {
    animation: true,
    animationDuration: 500,
    animationEasing: 'cubicOut',
    grid: { top: 6, right: 6, bottom: 6, left: 6, containLabel: false },
    tooltip: {
      trigger: 'axis',
      backgroundColor: t.surface,
      borderColor: t.border,
      borderWidth: 1,
      padding: [8, 10],
      textStyle: { color: t.fg, fontFamily: 'Inter, sans-serif', fontSize: 12 },
      axisPointer: {
        type: 'line',
        lineStyle: { color: t.border, type: 'solid', width: 1 }
      },
      formatter: (params: unknown) => {
        const arr = params as TooltipParam[]
        if (!arr?.length) return ''
        const idx = arr[0]!.dataIndex
        const dateIso = dates[idx] ?? ''
        const dateLabel = dateIso ? fmtDate(dateIso) : (arr[0]!.axisValueLabel ?? '')
        const balance = balances[idx] ?? 0
        return `
          <div style="font-weight:600;margin-bottom:4px;color:${t.fg}">${dateLabel}</div>
          <div style="color:${t.fgMuted};font-variant-numeric:tabular-nums">
            ${formatMoney(balance, props.currency, { withSymbol: true })}
          </div>
        `
      }
    },
    xAxis: {
      type: 'category',
      data: labels,
      show: false,
      boundaryGap: false
    },
    yAxis: {
      type: 'value',
      show: false,
      scale: true
    },
    series: [
      {
        type: 'line',
        smooth: true,
        symbol: 'none',
        sampling: 'lttb',
        lineStyle: { color: t.accent, width: 2 },
        areaStyle: {
          opacity: 1,
          color: {
            type: 'linear',
            x: 0, y: 0, x2: 0, y2: 1,
            colorStops: [
              { offset: 0, color: `color-mix(in oklch, ${t.accent} 26%, transparent)` },
              { offset: 1, color: 'transparent' }
            ]
          }
        },
        data: balances
      }
    ]
  }
})
</script>

<template>
  <div class="h-[80px]">
    <Skeleton v-if="isPending && !data" width="100%" height="80" rounded="md" />
    <VChart
      v-else-if="chartOption"
      :key="`${tokens.isDark}-${accountId}-${days}`"
      :option="chartOption"
      autoresize
      style="height: 100%; width: 100%"
    />
  </div>
</template>
