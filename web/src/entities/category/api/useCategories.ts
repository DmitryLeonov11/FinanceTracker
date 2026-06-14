import { computed, type MaybeRefOrGetter, toValue } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { http } from '@/shared/api/http'
import { z } from 'zod'
import { CategorySchema, type CategoryKind } from '../model/schemas'

const CategoryListSchema = z.array(CategorySchema)

export function useCategories(kind?: MaybeRefOrGetter<CategoryKind | undefined>) {
  const kindRef = computed(() => toValue(kind))
  return useQuery({
    queryKey: computed(() => ['categories', kindRef.value ?? 'all'] as const),
    queryFn: async () => {
      const params = kindRef.value ? { kind: kindRef.value } : undefined
      const { data } = await http.get('/categories', { params })
      return CategoryListSchema.parse(data)
    },
    staleTime: 60_000
  })
}
