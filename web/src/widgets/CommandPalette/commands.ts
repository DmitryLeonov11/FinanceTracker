import { routes } from '@/shared/config/routes'
import type { CommandSection } from './types'

export const commandSections: CommandSection[] = [
  {
    id: 'actions',
    label: 'Действия',
    items: [
      {
        id: 'create-transaction',
        title: 'Новая операция',
        hint: 'Доход или расход',
        icon: 'plus',
        keywords: ['добавить', 'transaction', 'доход', 'расход', 'income', 'expense', 'new'],
        run: ({ ui }) => ui.openRecordTransaction()
      },
      {
        id: 'record-transfer',
        title: 'Перевод между счетами',
        hint: 'Между двумя счетами в одной валюте',
        icon: 'swap',
        keywords: ['transfer', 'перевод', 'между', 'счетами'],
        run: ({ ui }) => ui.openRecordTransfer()
      },
      {
        id: 'create-account',
        title: 'Создать счёт',
        hint: 'Кошелёк, карта, крипто',
        icon: 'wallet',
        keywords: ['account', 'счет', 'кошелек', 'карта', 'new'],
        run: ({ ui }) => ui.openCreateAccount()
      },
      {
        id: 'create-budget',
        title: 'Создать бюджет',
        hint: 'Лимит расходов на период',
        icon: 'pieChart',
        keywords: ['budget', 'бюджет', 'лимит', 'plan', 'new'],
        run: ({ ui }) => ui.openCreateBudget()
      },
      {
        id: 'cycle-theme',
        title: 'Переключить тему',
        hint: 'Light · Dark · System',
        icon: 'sun',
        keywords: ['theme', 'тема', 'оформление', 'тёмная', 'светлая', 'dark', 'light'],
        run: ({ ui }) => ui.cycleTheme()
      },
      {
        id: 'logout',
        title: 'Выйти',
        hint: 'Завершить сессию',
        icon: 'logOut',
        keywords: ['logout', 'sign out', 'exit', 'выход'],
        run: async ({ auth, router }) => {
          auth.logout()
          await router.push('/login')
        }
      }
    ]
  },
  {
    id: 'navigation',
    label: 'Навигация',
    items: [
      {
        id: 'nav-dashboard',
        title: 'Сводка',
        hint: 'Главная',
        icon: 'home',
        keywords: ['dashboard', 'main', 'home', 'главная'],
        run: ({ router }) => {
          router.push(routes.dashboard())
        }
      },
      {
        id: 'nav-accounts',
        title: 'Счета',
        icon: 'wallet',
        keywords: ['accounts', 'кошельки'],
        run: ({ router }) => {
          router.push(routes.accounts())
        }
      },
      {
        id: 'nav-transactions',
        title: 'Операции',
        icon: 'swap',
        keywords: ['transactions', 'переводы', 'операции', 'история'],
        run: ({ router }) => {
          router.push(routes.transactions())
        }
      },
      {
        id: 'nav-budgets',
        title: 'Бюджеты',
        icon: 'pieChart',
        keywords: ['budgets', 'лимиты'],
        run: ({ router }) => {
          router.push(routes.budgets())
        }
      },
      {
        id: 'nav-goals',
        title: 'Цели',
        icon: 'target',
        keywords: ['goals', 'goals', 'накопления'],
        run: ({ router }) => {
          router.push(routes.goals())
        }
      },
      {
        id: 'nav-analytics',
        title: 'Аналитика',
        icon: 'barChart',
        keywords: ['analytics', 'отчёты', 'статистика'],
        run: ({ router }) => {
          router.push(routes.analytics())
        }
      },
      {
        id: 'nav-settings',
        title: 'Настройки',
        icon: 'cog',
        keywords: ['settings', 'preferences', 'настройки'],
        run: ({ router }) => {
          router.push(routes.settings())
        }
      }
    ]
  }
]

/** Lowercased substring match across title + hint + keywords. */
export function matchItem(itemSearchHay: string, query: string): boolean {
  if (!query) return true
  return itemSearchHay.includes(query.toLowerCase())
}

/** Pre-built search hay per item id, computed once. */
export function buildSearchHay(): Map<string, string> {
  const hay = new Map<string, string>()
  for (const section of commandSections) {
    for (const item of section.items) {
      const parts = [item.title, item.hint ?? '', ...(item.keywords ?? [])]
      hay.set(item.id, parts.join(' ').toLowerCase())
    }
  }
  return hay
}
