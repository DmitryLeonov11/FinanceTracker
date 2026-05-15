<script setup lang="ts">
import { computed, useId } from 'vue'

interface Props {
  label?: string
  hint?: string
  error?: string
  required?: boolean
}

const props = defineProps<Props>()
const id = useId()
const errorId = computed(() => (props.error ? `${id}-err` : undefined))
const hintId = computed(() => (props.hint && !props.error ? `${id}-hint` : undefined))
const describedBy = computed(() => errorId.value || hintId.value)
</script>

<template>
  <div class="flex flex-col gap-1.5">
    <label v-if="label" :for="id" class="text-[13px] font-medium text-fg flex items-center gap-1">
      {{ label }}
      <span v-if="required" class="text-danger">*</span>
    </label>
    <slot :id="id" :describedBy="describedBy" :invalid="!!error" />
    <p v-if="error" :id="errorId" class="text-[12px] text-danger leading-snug">
      {{ error }}
    </p>
    <p v-else-if="hint" :id="hintId" class="text-[12px] text-fg-subtle leading-snug">
      {{ hint }}
    </p>
  </div>
</template>
