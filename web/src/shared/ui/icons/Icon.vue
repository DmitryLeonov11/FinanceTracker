<script setup lang="ts">
import { computed } from 'vue'
import { ICON_PATHS, type IconName } from './paths'

interface Props {
  name: IconName
  size?: number | string
  strokeWidth?: number
  fill?: 'none' | 'currentColor'
}

const props = withDefaults(defineProps<Props>(), {
  size: 20,
  strokeWidth: 1.5,
  fill: 'none'
})

const path = computed(() => ICON_PATHS[props.name])
</script>

<template>
  <svg
    :width="size"
    :height="size"
    viewBox="0 0 24 24"
    :fill="fill"
    stroke="currentColor"
    :stroke-width="strokeWidth"
    stroke-linecap="round"
    stroke-linejoin="round"
    aria-hidden="true"
    focusable="false"
  >
    <path :d="path" />
  </svg>
</template>
