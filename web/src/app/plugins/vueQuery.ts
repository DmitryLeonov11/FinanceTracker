import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'
import type { App } from 'vue'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      gcTime: 5 * 60 * 1000,
      retry: (failureCount, error) => {
        const status = (error as { status?: number })?.status
        if (status && status < 500 && status !== 408) return false
        return failureCount < 1
      },
      refetchOnWindowFocus: false
    }
  }
})

export function installVueQuery(app: App) {
  app.use(VueQueryPlugin, { queryClient })
}

export { queryClient }
