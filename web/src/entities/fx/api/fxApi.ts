import { http } from '@/shared/api/http'
import { z } from 'zod'
import { FxConversionSchema, FxRateSchema, type FxConversion, type FxRate } from '../model/schemas'
import type { CurrencyCode } from '@/shared/config/currencies'

const FxRateListSchema = z.array(FxRateSchema)

export const fxApi = {
  async list(): Promise<FxRate[]> {
    const { data } = await http.get('/fx/rates')
    return FxRateListSchema.parse(data)
  },
  async convert(amount: number, from: CurrencyCode, to: CurrencyCode): Promise<FxConversion> {
    const { data } = await http.get('/fx/convert', { params: { amount, from, to } })
    return FxConversionSchema.parse(data)
  }
}
