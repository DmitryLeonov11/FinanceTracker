export const SUPPORTED_CURRENCIES = ['BYN', 'USD', 'EUR', 'RUB'] as const
export type CurrencyCode = (typeof SUPPORTED_CURRENCIES)[number]

export const CURRENCY_SYMBOLS: Record<CurrencyCode, string> = {
  BYN: 'Br',
  USD: '$',
  EUR: '€',
  RUB: '₽'
}

export const CURRENCY_LABELS: Record<CurrencyCode, string> = {
  BYN: 'Белорусский рубль',
  USD: 'Доллар США',
  EUR: 'Евро',
  RUB: 'Российский рубль'
}

export function isSupportedCurrency(code: string): code is CurrencyCode {
  return SUPPORTED_CURRENCIES.includes(code as CurrencyCode)
}
