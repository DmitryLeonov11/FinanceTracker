<script setup lang="ts">
import { computed, ref } from 'vue'
import { toast } from 'vue-sonner'
import { useCategories } from '../api/useCategories'
import { useCreateCategory } from '../api/useCreateCategory'
import type { Category, CategoryKind } from '../model/schemas'
import { cn } from '@/shared/lib/cn'
import Icon from '@/shared/ui/icons/Icon.vue'
import { Button, Input, Spinner } from '@/shared/ui/primitives'
import { ApiError } from '@/shared/api/errors'

interface Props {
  modelValue: string | null
  kind?: CategoryKind
  allowAll?: boolean
  allLabel?: string
  emptyLabel?: string
  allowCreate?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  kind: undefined,
  allowAll: true,
  allLabel: 'Без категории',
  emptyLabel: 'Категорий пока нет',
  allowCreate: true
})

const emit = defineEmits<{
  'update:modelValue': [value: string | null]
}>()

const kindRef = computed(() => props.kind)
const { data, isLoading } = useCategories(kindRef)
const { mutateAsync: createCategory, isPending: isCreating } = useCreateCategory()

const showCreateForm = ref(false)
const newName = ref('')

async function submitCreate() {
  const name = newName.value.trim()
  if (!name) return
  try {
    const created = await createCategory({
      name,
      kind: props.kind ?? 'Expense',
      parentId: null
    })
    emit('update:modelValue', created.id)
    newName.value = ''
    showCreateForm.value = false
    toast.success(`Категория «${created.name}» создана`)
  } catch (err) {
    if (err instanceof ApiError) {
      toast.error(err.message)
    } else {
      toast.error('Не удалось создать категорию')
    }
  }
}

interface Node {
  category: Category
  depth: number
}

const tree = computed<Node[]>(() => {
  const list = data.value ?? []
  const roots = list.filter((c) => c.parentId === null)
  const result: Node[] = []
  for (const root of roots) {
    result.push({ category: root, depth: 0 })
    const children = list.filter((c) => c.parentId === root.id)
    for (const child of children) {
      result.push({ category: child, depth: 1 })
    }
  }
  return result
})

function select(id: string | null) {
  emit('update:modelValue', id)
}

function onCreateKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter') {
    event.preventDefault()
    void submitCreate()
  } else if (event.key === 'Escape') {
    event.preventDefault()
    showCreateForm.value = false
  }
}
</script>

<template>
  <div class="grid gap-1.5">
    <div v-if="isLoading" class="flex items-center gap-2 text-[13px] text-fg-subtle py-2">
      <Spinner :size="14" /> Загружаем категории…
    </div>
    <template v-else>
      <button
        v-if="allowAll"
        type="button"
        :class="
          cn(
            'flex items-center gap-2 h-9 px-3 rounded-md border text-[13px] text-left',
            'transition-[border-color,background] duration-[120ms]',
            modelValue === null
              ? 'border-accent bg-accent-soft text-accent-soft-fg'
              : 'border-border hover:border-border-strong text-fg-muted hover:text-fg'
          )
        "
        @click="select(null)"
      >
        <Icon name="filter" :size="14" class="opacity-50" />
        {{ allLabel }}
      </button>

      <template v-if="tree.length">
        <button
          v-for="node in tree"
          :key="node.category.id"
          type="button"
          :class="
            cn(
              'flex items-center gap-2 h-9 px-3 rounded-md border text-[13px] text-left',
              'transition-[border-color,background] duration-[120ms]',
              modelValue === node.category.id
                ? 'border-accent bg-accent-soft text-accent-soft-fg'
                : 'border-border hover:border-border-strong text-fg-muted hover:text-fg'
            )
          "
          :style="node.depth > 0 ? { paddingLeft: `${12 + node.depth * 16}px` } : undefined"
          @click="select(node.category.id)"
        >
          <span
            v-if="node.category.color"
            class="size-2.5 rounded-full"
            :style="{ background: node.category.color }"
          />
          <span class="truncate">{{ node.category.name }}</span>
        </button>
      </template>
      <p v-else-if="!allowAll" class="text-[13px] text-fg-subtle py-2">
        {{ emptyLabel }}
      </p>

      <div v-if="allowCreate" class="pt-1">
        <button
          v-if="!showCreateForm"
          type="button"
          class="flex items-center gap-2 h-9 px-3 rounded-md text-[13px] text-fg-muted hover:text-fg hover:bg-surface-hi w-full text-left transition-colors"
          @click="showCreateForm = true"
        >
          <Icon name="plus" :size="14" />
          Создать категорию
        </button>
        <div v-else class="flex items-center gap-2">
          <Input
            v-model="newName"
            placeholder="Например, Продукты"
            class="flex-1"
            @keydown="onCreateKeydown"
          />
          <Button
            type="button"
            size="sm"
            :loading="isCreating"
            :disabled="!newName.trim()"
            icon-left="check"
            @click="submitCreate"
          >
            Создать
          </Button>
        </div>
      </div>
    </template>
  </div>
</template>
