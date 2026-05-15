<script setup lang="ts">
import Icon from '@/shared/ui/icons/Icon.vue'

interface Props {
  size?: number
}

withDefaults(defineProps<Props>(), { size: 18 })
</script>

<template>
  <Icon name="loader" :size="size" class="animate-spin text-fg-muted" />
</template>
