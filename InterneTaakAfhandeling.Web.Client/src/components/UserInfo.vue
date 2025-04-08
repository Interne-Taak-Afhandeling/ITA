<template>
  <div class="user-info-component">
   
    <div v-if="isAuthenticated" class="user-info">
      <span class="user-name">{{ user?.name }}</span>
      <button 
        class="utrecht-button utrecht-button--subtle" 
        @click="logout"
        :disabled="isLoading"
      >
        <span v-if="isLoading">Bezig...</span>
        <span v-else>Uitloggen</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth';
import { storeToRefs } from 'pinia';

const authStore = useAuthStore();
const { user, isAuthenticated, isLoading } = storeToRefs(authStore);

function logout() {
  authStore.logout();
}
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
  font-weight: 500;
}
</style>
