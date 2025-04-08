import axios from 'axios';
import type { User } from '@/types/user';


 
class AuthService {
 
  async getCurrentUser(): Promise<User | null> {
    try {
      const response = await axios.get(`/api/me`);
      
      if (response.data) {
        const userData: User = {
          isLoggedIn: response.data.isLoggedIn,
          email: response.data.email || '',
          name: response.data.name || '',
          roles: response.data.roles || [],
          isAdmin: response.data.isAdmin || false
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
  
   
   
  async logout(returnUrl: string = '/'): Promise<void> {
    try {
      const encodedReturnUrl = encodeURIComponent(returnUrl);
      window.location.href = `/api/logoff?returnUrl=${encodedReturnUrl}`;
      
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
