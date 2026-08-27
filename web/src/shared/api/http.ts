import axios, { AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios'
import { env } from '@/shared/config/env'
import { ApiError } from './errors'
import { ProblemDetailsSchema } from './schemas'

let accessTokenGetter: () => string | null = () => null
let refreshHandler: (() => Promise<string | null>) | null = null
let unauthorizedHandler: (() => void) | null = null

interface ExtendedConfig extends InternalAxiosRequestConfig {
  _retry?: boolean
  _skipAuth?: boolean
}

export function configureHttp(opts: {
  getAccessToken: () => string | null
  refresh: () => Promise<string | null>
  onUnauthorized: () => void
}) {
  accessTokenGetter = opts.getAccessToken
  refreshHandler = opts.refresh
  unauthorizedHandler = opts.onUnauthorized
}

export const http: AxiosInstance = axios.create({
  baseURL: env.VITE_API_BASE_URL,
  timeout: 20_000
})

const MUTATION_METHODS = new Set(['post', 'put', 'patch', 'delete'])

http.interceptors.request.use((cfg) => {
  const c = cfg as ExtendedConfig
  if (!c._skipAuth) {
    const token = accessTokenGetter()
    if (token) c.headers.Authorization = `Bearer ${token}`
  }
  if (c.method && MUTATION_METHODS.has(c.method.toLowerCase())) {
    if (!c.headers['Idempotency-Key']) {
      c.headers['Idempotency-Key'] = crypto.randomUUID()
    }
  }
  c.headers['Accept-Language'] = 'ru-RU'
  return c
})

let refreshPromise: Promise<string | null> | null = null

http.interceptors.response.use(
  (r) => r,
  async (err: AxiosError) => {
    if (!err.response) {
      throw new ApiError('Нет соединения с сервером', 0)
    }

    const cfg = err.config as ExtendedConfig | undefined
    const status = err.response.status

    if (status === 401 && cfg && !cfg._retry && !cfg._skipAuth && refreshHandler) {
      cfg._retry = true
      try {
        if (!refreshPromise) {
          refreshPromise = refreshHandler().finally(() => {
            refreshPromise = null
          })
        }
        const newToken = await refreshPromise
        if (newToken) {
          cfg.headers.Authorization = `Bearer ${newToken}`
          return http(cfg)
        }
      } catch {
        // refresh failed — fall through
      }
      unauthorizedHandler?.()
    }

    const parsed = ProblemDetailsSchema.safeParse(err.response.data)
    const problem = parsed.success ? parsed.data : undefined
    const message = problem?.detail ?? problem?.title ?? err.message
    throw new ApiError(message, status, problem)
  }
)
