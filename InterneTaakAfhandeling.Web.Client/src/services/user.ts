import { get } from '@/utils/fetchWrapper';
import type { Internetaken } from '@/types/internetaken';

export const userService = {

  getAssignedInternetaken: (): Promise<Internetaken[]> => {
    return get<Internetaken[]>('/api/user/internetaken');
  }
};
