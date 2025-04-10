<template> 
<div class="login-view">
    <div class="login-container">
      <h1 class="utrecht-heading-1">Inloggen</h1>
      <div v-if="isLoading" class="login-loading">
        <simple-spinner />
        <p class="utrecht-paragraph">Even geduld, u wordt ingelogd...</p>
      </div>
      <div v-else class="login-content">
        <utrecht-alert v-if="error" type="error">{{ error }}</utrecht-alert>
        <p class="utrecht-paragraph login-instruction">U moet ingelogd zijn om toegang te krijgen tot dit systeem.</p>
        <button class="utrecht-button utrecht-button--primary-action login-button" @click="login">
          <span class="utrecht-icon utrecht-icon--lock login-icon"></span>
          Inloggen
        </button>
      </div>
    </div> 
    </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth';
import { onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import SimpleSpinner from '@/components/SimpleSpinner.vue';

const authStore = useAuthStore();
const router = useRouter();
const route = useRoute();

const { isLoading, error } = authStore;
 
const returnUrl = (route.query.returnUrl as string) || '/';

function login() { 
  authStore.login();
}

onMounted(async () => { 
  await authStore.initialize();
  
  if (authStore.isAuthenticated) {
    router.push(returnUrl);
  }
});
</script>

<style lang="scss" scoped>
.login-view {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 70vh;
  padding: 1rem;
  background-color: var(--utrecht-surface-background-color);
}

.login-container {
  width: 100%;
  max-width: 500px;
  display: flex;
  flex-direction: column; 
  background-color: var(--utrecht-surface-background-color);
  border-radius: var(--utrecht-border-radius);
  box-shadow: var(--utrecht-shadow-medium);
  padding: 2.5rem;
  border-left: 4px solid var(--utrecht-color-green-30);
}
 
.utrecht-heading-1 {
  color: var(--utrecht-color-green-30);
  margin-bottom: 1.5rem;
  font-weight: 600;
}

.login-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin: 2rem 0;
}

.login-content {
  margin-top: 2rem;
}

.login-instruction {
  margin-bottom: 2rem;
  line-height: 1.5;
}

.login-button {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0.75rem 1.5rem;
  font-weight: 500;
  background-color: var(--utrecht-color-green-30);
  transition: background-color 0.2s ease;
  
  &:hover {
    background-color: var(--utrecht-color-green-40);
  }
}

.login-icon {
  margin-right: 0.75rem;
}
</style>
