<script setup lang="ts">
import Icon from '@/shared/ui/icons/Icon.vue'
import { t } from '@/shared/lib/i18n'
</script>

<template>
  <div class="min-h-dvh flex bg-bg">
    <main class="flex-1 flex items-center justify-center px-4 py-10">
      <div class="w-full max-w-[400px]">
        <div class="flex items-center gap-2 mb-10">
          <div class="size-8 rounded-md bg-accent flex items-center justify-center text-accent-fg">
            <Icon name="logo" :size="18" :stroke-width="2" />
          </div>
          <span class="font-semibold tracking-[-0.005em]">{{ t.app.name }}</span>
        </div>
        <slot />
      </div>
    </main>

    <aside
      class="hidden lg:flex flex-1 relative overflow-hidden bg-surface-sunk border-l border-border"
    >
      <div class="absolute inset-0 opacity-70" aria-hidden="true">
        <div
          class="absolute -top-32 -right-32 size-[480px] rounded-full"
          style="background: radial-gradient(closest-side, oklch(0.7 0.16 265 / 0.35), transparent);"
        />
        <div
          class="absolute -bottom-40 -left-20 size-[520px] rounded-full"
          style="background: radial-gradient(closest-side, oklch(0.65 0.15 200 / 0.25), transparent);"
        />
      </div>
      <div class="relative z-10 flex flex-col justify-end p-12 max-w-lg">
        <p class="text-[28px] font-semibold tracking-[-0.015em] leading-tight text-fg">
          Видеть свои финансы — значит ими управлять.
        </p>
        <p class="text-fg-muted mt-3 text-[15px]">
          Учёт счетов, транзакций и бюджетов в одном месте, со скоростью и точностью.
        </p>
      </div>
    </aside>
  </div>
</template>
