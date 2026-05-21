<script setup lang="ts">
import { computed, ref } from 'vue'
import VChart from 'vue-echarts'
import type { EChartsOption } from 'echarts'

import './echarts-setup'

import { Card, Skeleton } from '@/shared/ui/primitives'
import Money from '@/entities/money/ui/Money.vue'
import Icon from '@/shared/ui/icons/Icon.vue'
import { cn } from '@/shared/lib/cn'
import { fmtDate, fmtDayMonth } from '@/shared/lib/format/date'
import { formatMoney } from '@/shared/lib/format/money'

import { useCashflow } from '@/entities/dashboard/api/useCashflow'
import { useDashboardBalance } from '@/entities/dashboard/api/useDashboardBalance'
import { useChartTokens } from '@/shared/composables/useChartTokens'

import { PERIOD_DAYS, PERIOD_LABELS, PERIOD_ORDER, type CashflowPeriod } from './usePeriod'

const period = ref<CashflowPeriod>('30d')
const days = computed(() => PERIOD_DAYS[period.value])

const { data: balance } = useDashboardBalance()
const primaryCurrency = computed(() => balance.value?.displayCurrency)

const { data, isPending, isError, refetch } = useCashflow({ days, currency: primaryCurrency })

const tokens = useChartTokens()

interface TooltipParam {
  axisValueLabel: string
  data: number
  dataIndex: number
  seriesName: string
  marker: string
  color: string
}

const chartOption = computed<EChartsOption | null>(() => {
  if (!data.value) return null
  const t = tokens.value
  const dates = data.value.points.map((p) => p.date)
  const incomes = data.value.points.map((p) => p.income)
  // expense shown as negative for classic cashflow look (income up, expense down)
  const expenses = data.value.points.map((p) => -p.expense)

  const labels = dates.map((d) => fmtDayMonth(d))
  // sparse axis labels to avoid clutter
  const visibleLabelInterval = Math.max(1, Math.floor(dates.length / 8))

  return {
    animation: true,
    animationDuration: 600,
    animationEasing: 'cubicOut',
    grid: {
      top: 16,
      right: 12,
      bottom: 28,
      left: 12,
      containLabel: true
    },
    tooltip: {
      trigger: 'axis',
      backgroundColor: t.surface,
      borderColor: t.border,
      borderWidth: 1,
      padding: [10, 12],
      textStyle: { color: t.fg, fontFamily: 'Inter, sans-serif', fontSize: 13 },
      axisPointer: {
        type: 'line',
        lineStyle: { color: t.border, type: 'solid', width: 1 }
      },
      formatter: (params: unknown) => {
        const arr = params as TooltipParam[]
        if (!arr || arr.length === 0) return ''
        const idx = arr[0]!.dataIndex
        const dateIso = dates[idx] ?? ''
        const dateLabel = dateIso ? fmtDate(dateIso) : (arr[0]!.axisValueLabel ?? '')
        const income = incomes[idx] ?? 0
        const expense = -(expenses[idx] ?? 0)
        const curr = data.value!.currency
        const net = income - expense
        const row = (label: string, value: number, color: string) =>
          `<div style="display:flex;justify-content:space-between;gap:24px;align-items:baseline">
             <span style="color:${t.fgMuted};font-size:12px">${label}</span>
             <span style="color:${color};font-variant-numeric:tabular-nums">${formatMoney(value, curr, { withSymbol: true })}</span>
           </div>`
        return [
          `<div style="font-weight:600;margin-bottom:6px;color:${t.fg}">${dateLabel}</div>`,
          row('Доход', income, t.success),
          row('Расход', expense, t.danger),
          `<div style="height:1px;background:${t.border};margin:6px 0"></div>`,
          row('Net', net, net >= 0 ? t.success : t.danger)
        ].join('')
      }
    },
    xAxis: {
      type: 'category',
      data: labels,
      boundaryGap: false,
      axisLine: { show: false },
      axisTick: { show: false },
      axisLabel: {
        color: t.fgSubtle,
        fontSize: 11,
        margin: 12,
        interval: (idx: number) => idx % visibleLabelInterval === 0
      }
    },
    yAxis: {
      type: 'value',
      show: true,
      axisLine: { show: false },
      axisTick: { show: false },
      axisLabel: { show: false },
      splitLine: {
        lineStyle: { color: t.border, type: 'dashed', opacity: 0.6 }
      }
    },
    series: [
      {
        name: 'Доход',
        type: 'line',
        smooth: true,
        symbol: 'none',
        sampling: 'lttb',
        lineStyle: { color: t.success, width: 2 },
        areaStyle: {
          opacity: 1,
          color: {
            type: 'linear',
            x: 0, y: 0, x2: 0, y2: 1,
            colorStops: [
              { offset: 0, color: `color-mix(in oklch, ${t.success} 32%, transparent)` },
              { offset: 1, color: 'transparent' }
            ]
          }
        },
        data: incomes,
        z: 2
      },
      {
        name: 'Расход',
        type: 'line',
        smooth: true,
        symbol: 'none',
        sampling: 'lttb',
        lineStyle: { color: t.danger, width: 2 },
        areaStyle: {
          opacity: 1,
          color: {
            type: 'linear',
            x: 0, y: 1, x2: 0, y2: 0,
            colorStops: [
              { offset: 0, color: `color-mix(in oklch, ${t.danger} 28%, transparent)` },
              { offset: 1, color: 'transparent' }
            ]
          }
        },
        data: expenses,
        markLine: {
          symbol: 'none',
          silent: true,
          label: { show: false },
          lineStyle: { color: t.border, type: 'solid', width: 1 },
          data: [{ yAxis: 0 }]
        },
        z: 1
      }
    ]
  }
})

const hasData = computed(
  () => (data.value?.totalIncome ?? 0) > 0 || (data.value?.totalExpense ?? 0) > 0
)
</script>

<template>
  <section>
    <Card padding="lg">
      <header class="flex items-start justify-between gap-3 flex-wrap mb-4">
        <div>
          <h2 class="text-[15px] font-semibold tracking-[-0.005em]">Денежный поток</h2>
          <p class="text-[12px] text-fg-subtle mt-0.5">
            Доходы и расходы по дням
          </p>
        </div>

        <div class="flex items-center gap-1 bg-surface-sunk border border-border rounded-md p-1">
          <button
            v-for="p in PERIOD_ORDER"
            :key="p"
            type="button"
            :class="
              cn(
                'h-7 px-2.5 rounded-[6px] text-[12px] font-medium transition-colors',
                period === p
                  ? 'bg-surface text-fg shadow-xs'
                  : 'text-fg-muted hover:text-fg'
              )
            "
            @click="period = p"
          >
            {{ PERIOD_LABELS[p] }}
          </button>
        </div>
      </header>

      <!-- Summary row -->
      <div v-if="!isPending && data" class="flex flex-wrap gap-x-8 gap-y-2 mb-5">
        <div class="flex flex-col">
          <span class="text-[11px] text-fg-subtle uppercase tracking-wider">Доход</span>
          <span class="text-success">
            <Money :amount="data.totalIncome" :currency="data.currency" size="lg" :fade-decimals="false" />
          </span>
        </div>
        <div class="flex flex-col">
          <span class="text-[11px] text-fg-subtle uppercase tracking-wider">Расход</span>
          <span class="text-danger">
            <Money :amount="data.totalExpense" :currency="data.currency" size="lg" :fade-decimals="false" />
          </span>
        </div>
        <div class="flex flex-col">
          <span class="text-[11px] text-fg-subtle uppercase tracking-wider">Net</span>
          <span :class="data.net >= 0 ? 'text-success' : 'text-danger'">
            <Money
              :amount="data.net"
              :currency="data.currency"
              size="lg"
              :show-sign="data.net > 0"
              :fade-decimals="false"
            />
          </span>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="isPending && !data" class="space-y-2">
        <div class="flex gap-6">
          <Skeleton width="80" height="32" />
          <Skeleton width="80" height="32" />
          <Skeleton width="80" height="32" />
        </div>
        <Skeleton width="100%" height="200" rounded="md" />
      </div>

      <!-- Error -->
      <div v-else-if="isError" class="text-center py-10">
        <p class="text-[13px] text-fg-muted mb-2">Не удалось загрузить данные</p>
        <button class="text-accent hover:text-accent-hover text-[13px]" @click="refetch()">
          Повторить
        </button>
      </div>

      <!-- Empty -->
      <div v-else-if="!hasData" class="text-center py-10">
        <div class="mx-auto size-10 rounded-full bg-surface-hi flex items-center justify-center mb-3">
          <Icon name="barChart" :size="18" class="text-fg-subtle" />
        </div>
        <p class="text-[13px] text-fg-muted">
          За выбранный период операций в {{ data?.currency }} не было.
        </p>
      </div>

      <!-- Chart -->
      <div v-else class="h-[220px] -mx-2">
        <VChart
          :key="`${tokens.isDark}-${period}`"
          :option="chartOption!"
          autoresize
          style="height: 100%; width: 100%"
        />
      </div>
    </Card>
  </section>
</template>
