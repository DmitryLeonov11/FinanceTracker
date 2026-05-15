import { z } from 'zod'
import { CurrencySchema } from '@/shared/api/schemas'

export const AuthResultSchema = z.object({
  userId: z.string().uuid(),
  email: z.string().email(),
  displayName: z.string(),
  accessToken: z.string(),
  accessTokenExpiresAt: z.string(),
  refreshToken: z.string(),
  refreshTokenExpiresAt: z.string()
})
export type AuthResult = z.infer<typeof AuthResultSchema>

export const RegisterCommandSchema = z.object({
  email: z.string().email('Некорректный формат email').max(254),
  password: z
    .string()
    .min(8, 'Пароль должен содержать не менее 8 символов')
    .max(128)
    .regex(/[A-Z]/, 'Пароль должен содержать хотя бы одну заглавную букву')
    .regex(/[a-z]/, 'Пароль должен содержать хотя бы одну строчную букву')
    .regex(/\d/, 'Пароль должен содержать хотя бы одну цифру'),
  displayName: z.string().min(1, 'Имя обязательно').max(100),
  displayCurrency: CurrencySchema
})
export type RegisterCommand = z.infer<typeof RegisterCommandSchema>

export const LoginCommandSchema = z.object({
  email: z.string().email('Некорректный формат email'),
  password: z.string().min(1, 'Пароль обязателен')
})
export type LoginCommand = z.infer<typeof LoginCommandSchema>

export interface CurrentUser {
  userId: string
  email: string
  displayName: string
}
