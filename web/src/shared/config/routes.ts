export const routes = {
  root: () => '/',
  login: () => '/login',
  register: () => '/register',
  dashboard: () => '/dashboard',
  accounts: () => '/accounts',
  account: (id: string) => `/accounts/${id}`,
  transactions: () => '/transactions',
  transaction: (id: string) => `/transactions/${id}`,
  budgets: () => '/budgets',
  goals: () => '/goals',
  analytics: () => '/analytics',
  settings: () => '/settings',
  settingsProfile: () => '/settings/profile',
  settingsSecurity: () => '/settings/security',
  settingsPreferences: () => '/settings/preferences'
} as const
