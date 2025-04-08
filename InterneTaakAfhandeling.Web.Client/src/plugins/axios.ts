import axios from 'axios';
import { authService } from '@/services/auth';

// Add response interceptor to handle 401 errors globally
axios.interceptors.response.use(
  response => response,  
  error => { 
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      console.log('Global interceptor: Received 401 response, redirecting to login');
      authService.login();
      return new Promise(() => {});
    }
     
    return Promise.reject(error);
  }
);

export default axios;
