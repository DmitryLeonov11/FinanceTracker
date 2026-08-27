const dateLong = new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' })
const dateShort = new Intl.DateTimeFormat('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' })
const time = new Intl.DateTimeFormat('ru-RU', { hour: '2-digit', minute: '2-digit' })
const monthYear = new Intl.DateTimeFormat('ru-RU', { month: 'long', year: 'numeric' })

export function fmtDate(d: string | Date): string {
  return dateLong.format(new Date(d))
}

export function fmtDateShort(d: string | Date): string {
  return dateShort.format(new Date(d))
}

export function fmtTime(d: string | Date): string {
  return time.format(new Date(d))
}

export function fmtMonthYear(d: string | Date): string {
  return monthYear.format(new Date(d))
}

export function fmtRelative(d: string | Date): string {
  const now = new Date()
  const target = new Date(d)
  const diffMs = now.getTime() - target.getTime()
  const diffSec = Math.round(diffMs / 1000)
  if (Math.abs(diffSec) < 60) return 'только что'
  const diffMin = Math.round(diffSec / 60)
  if (Math.abs(diffMin) < 60) return `${diffMin} мин назад`
  const diffH = Math.round(diffMin / 60)
  if (Math.abs(diffH) < 24) return `${diffH} ч назад`
  const diffD = Math.round(diffH / 24)
  if (Math.abs(diffD) < 7) return `${diffD} дн назад`
  return fmtDateShort(target)
}
