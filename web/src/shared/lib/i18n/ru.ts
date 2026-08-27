/**
 * Minimal i18n: project is Russian-first.
 * Using a flat dict so keys are visible in code and easy to grep.
 * Switch to vue-i18n if a second locale is ever needed.
 */
export const ru = {
  app: {
    name: 'Finance Tracker',
    tagline: 'Личный финансовый трекер'
  },
  nav: {
    dashboard: 'Сводка',
    accounts: 'Счета',
    transactions: 'Операции',
    budgets: 'Бюджеты',
    goals: 'Цели',
    analytics: 'Аналитика',
    settings: 'Настройки'
  },
  auth: {
    loginTitle: 'Вход',
    loginSubtitle: 'Добро пожаловать. Войдите, чтобы продолжить.',
    registerTitle: 'Создание аккаунта',
    registerSubtitle: 'Зарегистрируйтесь, чтобы начать вести учёт.',
    email: 'Email',
    password: 'Пароль',
    displayName: 'Ваше имя',
    displayCurrency: 'Основная валюта',
    submitLogin: 'Войти',
    submitRegister: 'Создать аккаунт',
    haveAccount: 'Уже есть аккаунт?',
    noAccount: 'Нет аккаунта?',
    signIn: 'Войти',
    signUp: 'Зарегистрироваться',
    logout: 'Выйти',
    invalidCredentials: 'Неверный email или пароль'
  },
  dashboard: {
    greeting: (name: string) => `Привет, ${name}`,
    netWorth: 'Чистая стоимость',
    accountsCount: (n: number) =>
      n === 1 ? 'на 1 счёте' : n >= 2 && n <= 4 ? `на ${n} счетах` : `на ${n} счетах`,
    perCurrency: 'По валютам',
    accounts: 'Счета',
    noAccounts: 'У вас пока нет счетов',
    addFirstAccount: 'Добавить первый счёт'
  },
  accounts: {
    create: 'Новый счёт',
    name: 'Название',
    type: 'Тип',
    currency: 'Валюта',
    initialBalance: 'Начальный баланс',
    types: {
      Cash: 'Наличные',
      Bank: 'Банковский',
      Card: 'Карта',
      Crypto: 'Криптовалюта',
      Other: 'Другое'
    }
  },
  common: {
    save: 'Сохранить',
    cancel: 'Отмена',
    create: 'Создать',
    delete: 'Удалить',
    edit: 'Редактировать',
    confirm: 'Подтвердить',
    loading: 'Загрузка…',
    retry: 'Повторить',
    error: 'Ошибка',
    networkError: 'Нет соединения с сервером',
    unknownError: 'Что-то пошло не так',
    theme: { light: 'Светлая', dark: 'Тёмная', system: 'Системная' }
  }
} as const

export type Translation = typeof ru
