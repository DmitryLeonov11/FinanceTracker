import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { authApi } from '@/entities/user/api/authApi'
import type { AuthResult, CurrentUser, LoginCommand, RegisterCommand } from '@/entities/user/model/schemas'

const STORAGE_KEY = 'ft-auth'

interface PersistedAuth {
  userId: string
  email: string
  displayName: string
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
}

function load(): PersistedAuth | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? (JSON.parse(raw) as PersistedAuth) : null
  } catch {
    return null
  }
}

function save(state: PersistedAuth | null) {
  if (state) localStorage.setItem(STORAGE_KEY, JSON.stringify(state))
  else localStorage.removeItem(STORAGE_KEY)
}

export const useAuthStore = defineStore('auth', () => {
  const persisted = load()

  const accessToken = ref<string | null>(persisted?.accessToken ?? null)
  const accessTokenExpiresAt = ref<string | null>(persisted?.accessTokenExpiresAt ?? null)
  const refreshToken = ref<string | null>(persisted?.refreshToken ?? null)
  const user = ref<CurrentUser | null>(
    persisted ? { userId: persisted.userId, email: persisted.email, displayName: persisted.displayName } : null
  )

  const isAuthenticated = computed(() => !!accessToken.value && !!user.value)

  const EXPIRY_SKEW_MS = 30_000

  function applyAuthResult(result: AuthResult) {
    accessToken.value = result.accessToken
    accessTokenExpiresAt.value = result.accessTokenExpiresAt
    refreshToken.value = result.refreshToken
    user.value = {
      userId: result.userId,
      email: result.email,
      displayName: result.displayName
    }
    save({
      userId: result.userId,
      email: result.email,
      displayName: result.displayName,
      accessToken: result.accessToken,
      accessTokenExpiresAt: result.accessTokenExpiresAt,
      refreshToken: result.refreshToken,
      refreshTokenExpiresAt: result.refreshTokenExpiresAt
    })
  }

  async function login(cmd: LoginCommand) {
    const result = await authApi.login(cmd)
    applyAuthResult(result)
  }

  async function register(cmd: RegisterCommand) {
    const result = await authApi.register(cmd)
    applyAuthResult(result)
  }

  // Deduplicate concurrent refreshes (http 401 interceptor + SignalR factory can
  // race). Rotating refresh tokens are single-use, so two parallel calls would
  // invalidate each other.
  let refreshInFlight: Promise<string | null> | null = null

  async function refresh(): Promise<string | null> {
    if (!refreshToken.value) return null
    if (refreshInFlight) return refreshInFlight
    refreshInFlight = (async () => {
      try {
        const result = await authApi.refresh(refreshToken.value!)
        applyAuthResult(result)
        return result.accessToken
      } catch {
        logout()
        return null
      }
    })().finally(() => {
      refreshInFlight = null
    })
    return refreshInFlight
  }

  /** Returns a non-expired access token, refreshing proactively if needed. */
  async function ensureValidAccessToken(): Promise<string | null> {
    if (!accessToken.value) return null
    const expiresAt = accessTokenExpiresAt.value ? Date.parse(accessTokenExpiresAt.value) : NaN
    if (!Number.isNaN(expiresAt) && expiresAt - Date.now() < EXPIRY_SKEW_MS) {
      return await refresh()
    }
    return accessToken.value
  }

  function logout() {
    accessToken.value = null
    accessTokenExpiresAt.value = null
    refreshToken.value = null
    user.value = null
    save(null)
  }

  return {
    accessToken,
    accessTokenExpiresAt,
    refreshToken,
    user,
    isAuthenticated,
    login,
    register,
    refresh,
    ensureValidAccessToken,
    logout
  }
})
