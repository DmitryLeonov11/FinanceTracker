<script setup lang="ts">
import { computed, defineAsyncComponent } from 'vue'
import { useRoute } from 'vue-router'

const AppShellLayout = defineAsyncComponent(() => import('./layouts/AppShellLayout.vue'))
const AuthLayout = defineAsyncComponent(() => import('./layouts/AuthLayout.vue'))
const EmptyLayout = defineAsyncComponent(() => import('./layouts/EmptyLayout.vue'))

const route = useRoute()

const layout = computed(() => {
  switch (route.meta.layout) {
    case 'auth':
      return AuthLayout
    case 'empty':
      return EmptyLayout
    case 'app':
    default:
      return AppShellLayout
  }
})
</script>

<template>
  <component :is="layout">
    <RouterView v-slot="{ Component, route: r }">
      <Transition name="page" mode="out-in">
        <component :is="Component" :key="r.path" />
      </Transition>
    </RouterView>
  </component>
  <Toaster position="bottom-right" :theme="'system'" :duration="3500" />
</template>

<script lang="ts">
import { Toaster } from 'vue-sonner'
export default { components: { Toaster } }
</script>

<style>
.page-enter-active,
.page-leave-active {
  transition: opacity 180ms var(--ease-out), transform 180ms var(--ease-out);
}
.page-enter-from {
  opacity: 0;
  transform: translateY(4px);
}
.page-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}
</style>
