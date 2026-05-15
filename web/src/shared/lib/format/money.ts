import { CURRENCY_SYMBOLS, type CurrencyCode } from '@/shared/config/currencies'

const formatters = new Map<string, Intl.NumberFormat>()

function getFormatter(maximumFractionDigits: number) {
  const key = `ru-RU:${maximumFractionDigits}`
  let f = formatters.get(key)
  if (!f) {
    f = new Intl.NumberFormat('ru-RU', {
      minimumFractionDigits: 2,
      maximumFractionDigits,
      useGrouping: true
    })
    formatters.set(key, f)
  }
  return f
}

export interface FormatMoneyOptions {
  /** show + sign for positive */
  showSign?: boolean
  /** strip decimals if integer */
  trimZeros?: boolean
  /** include currency symbol/code (default: true) */
  withSymbol?: boolean
}

export function formatMoney(amount: number, currency: CurrencyCode, opts: FormatMoneyOptions = {}): string {
  const { showSign = false, trimZeros = false, withSymbol = true } = opts
  const isInt = Number.isInteger(amount)
  const fmt = getFormatter(trimZeros && isInt ? 0 : 2)
  const abs = Math.abs(amount)
  const num = fmt.format(abs).replace(/ /g, ' ')
  const sign = amount < 0 ? '−' : showSign && amount > 0 ? '+' : ''
  if (!withSymbol) return `${sign}${num}`
  const sym = CURRENCY_SYMBOLS[currency]
  return `${sign}${num} ${sym}`
}

/** Splits a money string into integer + decimal + currency parts for typographic alignment. */
export function splitMoney(amount: number, currency: CurrencyCode) {
  const isNeg = amount < 0
  const abs = Math.abs(amount)
  const fmt = getFormatter(2)
  const formatted = fmt.format(abs).replace(/ /g, ' ')
  const parts = formatted.split(',')
  const integer = parts[0] ?? '0'
  const decimal = parts[1] ?? '00'
  return {
    sign: isNeg ? '−' : '',
    integer,
    decimal,
    symbol: CURRENCY_SYMBOLS[currency]
  }
}
