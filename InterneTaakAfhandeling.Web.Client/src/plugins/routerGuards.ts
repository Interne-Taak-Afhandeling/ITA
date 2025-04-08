import type { App } from 'vue';
import type { Router } from 'vue-router';
import type { RouteLocationNormalized, NavigationGuardNext } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

 
export function authGuard(to: RouteLocationNormalized, from: RouteLocationNormalized, next: NavigationGuardNext) {
  const authStore = useAuthStore();
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);

  if (requiresAuth && !authStore.isAuthenticated) {
     return next({ 
      name: 'login', 
      query: { returnUrl: to.fullPath }
    });
  }

  return next();
}

 
export function adminGuard(to: RouteLocationNormalized, from: RouteLocationNormalized, next: NavigationGuardNext) {
  const authStore = useAuthStore();
  const requiresAdmin = to.matched.some(record => record.meta.requiresAdmin);
  
  if (requiresAdmin && !authStore.isAdmin) {
    return next({ name: 'dashboard' });
  }

  return next();
}

 
export function titleGuard(to: RouteLocationNormalized, from: RouteLocationNormalized, next: NavigationGuardNext) {
  const appTitle = document.title.split(' | ').pop() || document.title;
  document.title = `${to.meta?.title ? to.meta.title + " | " : ""}${appTitle}`;
  
   document.body.setAttribute("tabindex", "-1");
  document.body.focus();
  document.body.removeAttribute("tabindex");
  
  return next();
}
 
export const RouterGuardsPlugin = {
  install(app: App, router: Router) {
    router.beforeEach(titleGuard);
    router.beforeEach(authGuard);
    router.beforeEach(adminGuard);
  }
};

export default RouterGuardsPlugin;
