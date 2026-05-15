<script setup lang="ts">
import { computed, ref } from 'vue'
import { cn } from '@/shared/lib/cn'
import Icon from '@/shared/ui/icons/Icon.vue'
import type { IconName } from '@/shared/ui/icons/paths'

interface Props {
  modelValue?: string | number | null
  type?: string
  placeholder?: string
  disabled?: boolean
  invalid?: boolean
  iconLeft?: IconName
  size?: 'sm' | 'md' | 'lg'
  inputmode?: 'text' | 'numeric' | 'decimal' | 'email' | 'tel' | 'url' | 'search'
  autocomplete?: string
  showPasswordToggle?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  type: 'text',
  size: 'md',
  disabled: false,
  invalid: false
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
  blur: [event: FocusEvent]
  focus: [event: FocusEvent]
}>()

const showPassword = ref(false)
const effectiveType = computed(() =>
  props.type === 'password' && showPassword.value ? 'text' : props.type
)

const wrapperCls = computed(() =>
  cn(
    'group relative flex items-center rounded-md border bg-surface',
    'transition-[border-color,box-shadow] duration-[120ms]',
    'focus-within:border-accent focus-within:ring-2 focus-within:ring-accent/20',
    props.invalid
      ? 'border-danger focus-within:border-danger focus-within:ring-danger/20'
      : 'border-border hover:border-border-strong',
    props.disabled && 'opacity-50 pointer-events-none bg-surface-sunk',
    props.size === 'sm' ? 'h-8' : props.size === 'lg' ? 'h-11' : 'h-10'
  )
)

const inputCls = computed(() =>
  cn(
    'flex-1 min-w-0 bg-transparent outline-none placeholder:text-fg-subtle',
    'text-fg',
    props.size === 'sm' ? 'text-[13px] px-2.5' : props.size === 'lg' ? 'text-[15px] px-4' : 'text-[14px] px-3',
    props.iconLeft && (props.size === 'sm' ? 'pl-1.5' : 'pl-2'),
    props.type === 'password' && props.showPasswordToggle && 'pr-9'
  )
)

function onInput(e: Event) {
  emit('update:modelValue', (e.target as HTMLInputElement).value)
}
</script>

<template>
  <div :class="wrapperCls">
    <span v-if="iconLeft" class="pl-3 text-fg-subtle">
      <Icon :name="iconLeft" :size="16" />
    </span>
    <input
      :class="inputCls"
      :type="effectiveType"
      :value="modelValue ?? ''"
      :placeholder="placeholder"
      :disabled="disabled"
      :inputmode="inputmode"
      :autocomplete="autocomplete"
      @input="onInput"
      @blur="emit('blur', $event)"
      @focus="emit('focus', $event)"
    />
    <button
      v-if="type === 'password' && showPasswordToggle"
      type="button"
      tabindex="-1"
      class="absolute right-1 inset-y-0 my-auto h-7 w-7 inline-flex items-center justify-center rounded-md text-fg-subtle hover:text-fg hover:bg-surface-hi transition-colors"
      @click="showPassword = !showPassword"
    >
      <Icon :name="showPassword ? 'eyeOff' : 'eye'" :size="16" />
    </button>
  </div>
</template>
