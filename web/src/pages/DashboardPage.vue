<script setup lang="ts">
import { ref, computed } from 'vue'
import DashboardSummary from '@/widgets/DashboardSummary/DashboardSummary.vue'
import AccountsList from '@/widgets/AccountsList/AccountsList.vue'
import CreateAccountDialog from '@/features/account/create-account/CreateAccountDialog.vue'
import { useAuthStore } from '@/shared/stores/auth'
import { useRealtimeInvalidation } from '@/shared/composables/useRealtimeInvalidation'

const auth = useAuthStore()

useRealtimeInvalidation()

const createOpen = ref(false)

const greeting = computed(() => {
  const h = new Date().getHours()
  const part = h < 5 ? 'Доброй ночи' : h < 12 ? 'Доброе утро' : h < 18 ? 'Добрый день' : 'Добрый вечер'
  return `${part}, ${auth.user?.displayName ?? ''}`.trim()
})
</script>

<template>
  <div class="space-y-6 md:space-y-8">
    <header class="flex items-center justify-between">
      <div>
        <p class="text-[13px] text-fg-subtle">{{ greeting }}</p>
        <h1 class="text-[22px] font-semibold tracking-[-0.01em] mt-0.5">Сводка</h1>
      </div>
    </header>

    <DashboardSummary />
    <AccountsList :on-create="() => (createOpen = true)" />

    <CreateAccountDialog v-model:open="createOpen" />
  </div>
</template>
