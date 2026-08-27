<script setup lang="ts">
import { useRoute } from 'vue-router'
import Icon from '@/shared/ui/icons/Icon.vue'
import { Card } from '@/shared/ui/primitives'

const route = useRoute()
</script>

<template>
  <Card variant="outlined" padding="lg" class="border-dashed text-center max-w-lg mx-auto mt-8">
    <div class="mx-auto size-12 rounded-full bg-surface-hi flex items-center justify-center mb-3">
      <Icon name="loader" :size="20" class="text-fg-subtle" />
    </div>
    <p class="font-medium">{{ route.meta.title }}</p>
    <p class="text-[13px] text-fg-muted mt-1">Раздел в разработке</p>
  </Card>
</template>
