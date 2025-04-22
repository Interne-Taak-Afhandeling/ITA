import { get } from '@/utils/fetchWrapper';
import type { AssignedInternetaken } from '@/types/internetaken';

export const userService = {

  getAssignedInternetaken: (): Promise<AssignedInternetaken[]> => {
    return get<AssignedInternetaken[]>('/api/user/internetaken');
  }
};
