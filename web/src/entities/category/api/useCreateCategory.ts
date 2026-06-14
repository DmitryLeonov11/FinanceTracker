import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { categoriesApi } from './categoriesApi'
import type { CreateCategoryCommand } from '../model/schemas'

export function useCreateCategory() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (cmd: CreateCategoryCommand) => categoriesApi.create(cmd),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['categories'] })
    }
  })
}
