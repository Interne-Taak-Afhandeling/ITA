import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authService } from '@/services/auth';
import type { User } from '@/types/user';

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  const isAuthenticated = computed(() => user.value?.isLoggedIn || false);
  const isAdmin = computed(() => user.value?.isAdmin || false);

  async function initialize() {
    isLoading.value = true;
    error.value = null;

    try {
      const currentUser = await authService.getCurrentUser();
      user.value = currentUser;
    } catch (err: any) {
      error.value = err.message || 'Failed to initialize authentication';
      console.error('Auth initialization failed:', err);
    } finally {
      isLoading.value = false;
    }
  }

  async function login() {
    isLoading.value = true;
    error.value = null;

    try {
      await authService.login();
    } catch (err: any) {
      error.value = err.message || 'Login failed';
      console.error('Login failed:', err);
    } finally {
      isLoading.value = false;
    }
  }

  async function logout() {
    isLoading.value = true;
    error.value = null;

    try {
      await authService.logout();
      user.value = null;
    } catch (err: any) {
      error.value = err.message || 'Logout failed';
      console.error('Logout failed:', err);
    } finally {
      isLoading.value = false;
    }
  }
  
   

  return {
    user,
    isLoading,
    error,
    isAuthenticated,
    isAdmin,
    initialize,
    login,
    logout
  };
});
