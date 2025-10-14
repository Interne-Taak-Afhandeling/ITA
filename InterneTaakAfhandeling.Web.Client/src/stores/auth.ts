import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { authService } from "@/services/authService";
import type { User } from "@/types/user";

export const useAuthStore = defineStore("auth", () => {
  const user = ref<User | null>(null);
  const isLoading = ref(false);
  const error = ref<string | null>(null);
  const initialized = computed(() => user.value !== null);

  const isAuthenticated = computed(() => user.value?.isLoggedIn || false);
  const hasITASystemAccess = computed(() => user.value?.hasITASystemAccess || false);
  const hasFunctioneelBeheerderAccess = computed(
    () => user.value?.hasFunctioneelBeheerderAccess || false
  );

  async function initialize() {
    isLoading.value = true;
    error.value = null;

    try {
      const currentUser = await authService.getCurrentUser();
      user.value = currentUser;
    } catch (err: unknown) {
      error.value =
        err instanceof Error && err.message ? err.message : "Failed to initialize authentication";
      console.error("Auth initialization failed:", err);
    } finally {
      isLoading.value = false;
    }
  }

  async function login() {
    isLoading.value = true;
    error.value = null;

    try {
      await authService.login();
    } catch (err: unknown) {
      error.value = err instanceof Error && err.message ? err.message : "Login failed";
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
    } catch (err: unknown) {
      error.value = err instanceof Error && err.message ? err.message : "Logout failed";
      console.error("Logout failed:", err);
    } finally {
      isLoading.value = false;
    }
  }

  return {
    user,
    isLoading,
    error,
    isAuthenticated,
    hasITASystemAccess,
    hasFunctioneelBeheerderAccess,
    initialized,
    initialize,
    login,
    logout
  };
});
