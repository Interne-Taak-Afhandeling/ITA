import type { User } from '@/types/user';
import { get } from '@/utils/fetchWrapper';


 
class AuthService {
 
  async getCurrentUser(): Promise<User | null> {
    try {
      const data = await get<User>(`/api/me`);
      
      if (data) {
        const userData: User = {
          isLoggedIn: data.isLoggedIn,
          email: data.email || '',
          name: data.name || '',
          roles: data.roles || [],
          hasITASystemAccess: data.hasITASystemAccess || false
        };
        return userData;
      }
      return null;
    } catch (error) {
      throw error;
    }
  }

  
  async login(returnUrl?: string) { 
    const currentUrl = window.location.href;
    const encodedReturnUrl = encodeURIComponent(returnUrl || currentUrl);
     
    window.location.href = `/api/challenge?returnUrl=${encodedReturnUrl}`;
  }
  
   
   
  async logout(): Promise<void> {
    try {
      window.location.href = `/api/logoff`;
    } catch (error) {
      console.error('Logout error:', error);
      throw error;
    }
  }

  
 
  async isAuthenticated(): Promise<boolean> {
    return (await this.getCurrentUser())?.isLoggedIn || false;
  }
}

export const authService = new AuthService();
