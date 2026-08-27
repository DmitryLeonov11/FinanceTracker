import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/shared/stores/auth'

const routes: RouteRecordRaw[] = [
  { path: '/', redirect: '/dashboard' },
  {
    path: '/login',
    name: 'login',
    component: () => import('@/pages/auth/LoginPage.vue'),
    meta: { layout: 'auth', guest: true, title: 'Вход' }
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('@/pages/auth/RegisterPage.vue'),
    meta: { layout: 'auth', guest: true, title: 'Регистрация' }
  },
  {
    path: '/dashboard',
    name: 'dashboard',
    component: () => import('@/pages/DashboardPage.vue'),
    meta: { layout: 'app', auth: true, title: 'Сводка' }
  },
  {
    path: '/accounts',
    name: 'accounts',
    component: () => import('@/pages/AccountsPage.vue'),
    meta: { layout: 'app', auth: true, title: 'Счета' }
  },
  {
    path: '/transactions',
    name: 'transactions',
    component: () => import('@/pages/TransactionsPage.vue'),
    meta: { layout: 'app', auth: true, title: 'Операции' }
  },
  {
    path: '/budgets',
    name: 'budgets',
    component: () => import('@/pages/PlaceholderPage.vue'),
    meta: { layout: 'app', auth: true, title: 'Бюджеты' }
  },
  {
    path: '/goals',
    name: 'goals',
    component: () => import('@/pages/PlaceholderPage.vue'),
    meta: { layout: 'app', auth: true, title: 'Цели' }
  },
  {
    path: '/analytics',
    name: 'analytics',
    component: () => import('@/pages/PlaceholderPage.vue'),
    meta: { layout: 'app', auth: true, title: 'Аналитика' }
  },
  {
    path: '/settings',
    name: 'settings',
    component: () => import('@/pages/SettingsPage.vue'),
    meta: { layout: 'app', auth: true, title: 'Настройки' }
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/pages/NotFoundPage.vue'),
    meta: { layout: 'empty', title: 'Не найдено' }
  }
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(_to, _from, saved) {
    return saved || { top: 0, behavior: 'smooth' }
  }
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.auth && !auth.isAuthenticated) {
    return { path: '/login', query: { redirect: to.fullPath } }
  }

  if (to.meta.guest && auth.isAuthenticated) {
    return { path: '/dashboard' }
  }

  return true
})

router.afterEach((to) => {
  const title = (to.meta.title as string | undefined) ?? 'Finance Tracker'
  document.title = `${title} · Finance Tracker`
})
