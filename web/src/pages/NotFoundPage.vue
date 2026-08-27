<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { Button } from '@/shared/ui/primitives'
</script>

<template>
  <div class="text-center max-w-md">
    <p class="text-[80px] font-semibold tracking-[-0.04em] leading-none text-fg-muted">404</p>
    <p class="mt-2 text-[18px] font-semibold">Страница не найдена</p>
    <p class="text-fg-muted text-[14px] mt-1">
      Возможно, ссылка устарела или была удалена.
    </p>
    <RouterLink to="/dashboard" class="inline-block mt-5">
      <Button intent="primary" icon-left="arrowLeft">На главную</Button>
    </RouterLink>
  </div>
</template>
