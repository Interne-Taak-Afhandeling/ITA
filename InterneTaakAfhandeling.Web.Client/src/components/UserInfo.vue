<template>
  <div class="user-info-component">
    <div v-if="isAuthenticated" class="user-info">
      <span class="user-name utrecht-nav-list__item">{{ user?.name }}</span>
      <a href="/api/logoff" class="utrecht-link utrecht-link--html-a utrecht-nav-list__link">
        Uitloggen
      </a>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from "@/stores/auth";
import { storeToRefs } from "pinia";

const authStore = useAuthStore();
const { user, isAuthenticated } = storeToRefs(authStore);
</script>

<style scoped>
.user-info-component {
  display: flex;
  align-items: center;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-name {
  inline-size: max-content;
}
</style>
