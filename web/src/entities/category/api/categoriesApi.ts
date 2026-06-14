import { http } from '@/shared/api/http'
import { CategorySchema, type Category, type CreateCategoryCommand } from '../model/schemas'

export const categoriesApi = {
  async create(cmd: CreateCategoryCommand): Promise<Category> {
    const { data } = await http.post('/categories', cmd)
    return CategorySchema.parse(data)
  }
}
