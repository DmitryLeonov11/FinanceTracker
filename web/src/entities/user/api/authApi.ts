import { http } from '@/shared/api/http'
import {
  type AuthResult,
  type LoginCommand,
  type RegisterCommand,
  AuthResultSchema
} from '../model/schemas'

export const authApi = {
  async register(cmd: RegisterCommand): Promise<AuthResult> {
    // _skipAuth читает интерцептор в http.ts: без него 401 на регистрации/логине/рефреше
    // зациклил бы refresh-логику
    const { data } = await http.post('/auth/register', cmd, {
      headers: {},
      // @ts-expect-error custom flag
      _skipAuth: true
    })
    return AuthResultSchema.parse(data)
  },

  async login(cmd: LoginCommand): Promise<AuthResult> {
    const { data } = await http.post('/auth/login', cmd, {
      // @ts-expect-error custom flag
      _skipAuth: true
    })
    return AuthResultSchema.parse(data)
  },

  async refresh(refreshToken: string): Promise<AuthResult> {
    const { data } = await http.post(
      '/auth/refresh',
      { refreshToken },
      {
        // @ts-expect-error custom flag
        _skipAuth: true
      }
    )
    return AuthResultSchema.parse(data)
  }
}
