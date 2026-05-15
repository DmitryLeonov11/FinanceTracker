import { z } from 'zod'

const schema = z.object({
  VITE_API_BASE_URL: z.string().default('/api'),
  VITE_SENTRY_DSN: z.string().optional()
})

export const env = schema.parse({
  VITE_API_BASE_URL: import.meta.env.VITE_API_BASE_URL,
  VITE_SENTRY_DSN: import.meta.env.VITE_SENTRY_DSN
})
