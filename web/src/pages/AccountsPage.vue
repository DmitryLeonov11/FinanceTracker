<script setup lang="ts">
import { ref } from 'vue'
import AccountsList from '@/widgets/AccountsList/AccountsList.vue'
import CreateAccountDialog from '@/features/account/create-account/CreateAccountDialog.vue'
import { useRealtimeInvalidation } from '@/shared/composables/useRealtimeInvalidation'

useRealtimeInvalidation()

const createOpen = ref(false)
</script>

<template>
  <div class="space-y-6">
    <AccountsList :on-create="() => (createOpen = true)" />
    <CreateAccountDialog v-model:open="createOpen" />
  </div>
</template>
