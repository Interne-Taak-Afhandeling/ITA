import { get, post } from '@/utils/fetchWrapper';
import type { Internetaken } from '@/types/internetaken';

export const userService = {

  getAssignedInternetaken: (): Promise<Internetaken[]> => {
    return get<Internetaken[]>('/api/myinternetaken/internetaken');
  },
    assignInternetakenToSelf: (id: string): Promise<boolean> => {
      return post<boolean>(`/api/internetaken/${id}/assign-to-self`, {});
  }
};
