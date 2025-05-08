import { defineStore } from 'pinia';
import { ref } from 'vue';
import { userService } from '@/services/user';
import type { Internetaken } from '@/types/internetaken';
import { klantcontactService, type CreateKlantcontactRequest } from '@/services/klantcontactService';

export const useUserStore = defineStore('user', () => {
  const assignedInternetaken = ref<Internetaken[]>([]);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  const fetchAssignedInternetaken = async () => {
    isLoading.value = true;
    error.value = null;

    try {
      assignedInternetaken.value = await userService.getAssignedInternetaken();
    } catch (err: any) {
      error.value = err.message || 'Er is een fout opgetreden bij het ophalen van de interne taken';
      console.error('Error fetching internetaken:', err);
    } finally {
      isLoading.value = false;
    }
  };

  const createKlantcontact = async (request: CreateKlantcontactRequest) => {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await klantcontactService.createKlantcontact(request);
      return response;
    } catch (err: any) {
      error.value = err.message || 'Er is een fout opgetreden bij het aanmaken van het klantcontact';
      console.error('Error creating klantcontact:', err);
      throw err;
    } finally {
      isLoading.value = false;
    }
  };

  return {
    assignedInternetaken,
    isLoading,
    error,
    fetchAssignedInternetaken,
    createKlantcontact
  };
});