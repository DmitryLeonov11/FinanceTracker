import { z } from 'zod'

export const CategoryKindSchema = z.enum(['Income', 'Expense'])
export type CategoryKind = z.infer<typeof CategoryKindSchema>

export const CategorySchema = z.object({
  id: z.string().uuid(),
  parentId: z.string().uuid().nullable(),
  name: z.string(),
  kind: CategoryKindSchema,
  icon: z.string().nullable().optional(),
  color: z.string().nullable().optional(),
  createdAt: z.string()
})
export type Category = z.infer<typeof CategorySchema>

export const CreateCategoryCommandSchema = z.object({
  name: z.string().min(1, 'Название обязательно').max(100),
  kind: CategoryKindSchema,
  parentId: z.string().uuid().nullable().optional(),
  icon: z.string().max(50).nullable().optional(),
  color: z.string().max(20).nullable().optional()
})
export type CreateCategoryCommand = z.infer<typeof CreateCategoryCommandSchema>
