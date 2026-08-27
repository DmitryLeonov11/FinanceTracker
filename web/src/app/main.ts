import { createApp } from 'vue'
import { createPinia } from 'pinia'
import * as Sentry from '@sentry/vue'

import App from './App.vue'
import { router } from './router'
import { installVueQuery } from './plugins/vueQuery'
import { configureHttp } from '@/shared/api/http'
import { realtime } from '@/shared/api/signalr'
import { useAuthStore } from '@/shared/stores/auth'
import { useUiStore } from '@/shared/stores/ui'
import { env } from '@/shared/config/env'

import './styles/tailwind.css'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
installVueQuery(app)

if (env.VITE_SENTRY_DSN) {
  Sentry.init({
    app,
    dsn: env.VITE_SENTRY_DSN,
    integrations: [Sentry.browserTracingIntegration({ router })],
    tracesSampleRate: 0.1
  })
}

const auth = useAuthStore()
useUiStore() // initialize theme listener

configureHttp({
  getAccessToken: () => auth.accessToken,
  refresh: () => auth.refresh(),
  onUnauthorized: () => {
    auth.logout()
    if (router.currentRoute.value.meta.auth) {
      router.push({ path: '/login', query: { redirect: router.currentRoute.value.fullPath } })
    }
  }
})

realtime.configure(() => auth.accessToken)

if (auth.isAuthenticated) {
  realtime.connect().catch(() => {
    /* will retry on next login */
  })
}

app.mount('#app')
