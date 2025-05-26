import { defineStore } from "pinia";
import { ref } from "vue";
import { userService } from "@/services/userService";
import type { Internetaken } from "@/types/internetaken";

export const useUserStore = defineStore("user", () => {
  const assignedInternetaken = ref<Internetaken[]>([]);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  const fetchAssignedInternetaken = async () => {
    isLoading.value = true;
    error.value = null;

    try {
      assignedInternetaken.value = await userService.getAssignedInternetaken();
    } catch (err: unknown) {
      error.value =
        err instanceof Error && err.message
          ? err.message
          : "Er is een fout opgetreden bij het ophalen van de interne taken";
      console.error("Error fetching internetaken:", err);
    } finally {
      isLoading.value = false;
    }
  };

  return {
    assignedInternetaken,
    isLoading,
    error,
    fetchAssignedInternetaken
  };
});
