import { http } from '@/shared/api/http'
import {
  type AuthResult,
  type LoginCommand,
  type RegisterCommand,
  AuthResultSchema
} from '../model/schemas'

export const authApi = {
  async register(cmd: RegisterCommand): Promise<AuthResult> {
    const { data } = await http.post('/auth/register', cmd, {
      // refresh interceptor would loop on register if 401 happened — skip auth
      headers: {},
      // @ts-expect-error custom flag picked up by interceptor
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
