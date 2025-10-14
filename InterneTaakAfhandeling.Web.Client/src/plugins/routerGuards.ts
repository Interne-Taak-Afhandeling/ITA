import type { App } from "vue";
import type { Router } from "vue-router";
import type { RouteLocationNormalized, NavigationGuardNext } from "vue-router";
import { useAuthStore } from "@/stores/auth";

function authGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
) {
  const authStore = useAuthStore();
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);

  if (requiresAuth && !authStore.isAuthenticated) {
    return next({
      name: "login",
      query: { returnUrl: to.fullPath }
    });
  }

  return next();
}

function itaAccessGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
) {
  const authStore = useAuthStore();
  const requiresITAAccess = to.matched.some((record) => record.meta.requiresITAAccess);

  return authGuard(to, from, () => {
    if (requiresITAAccess && !authStore.hasITASystemAccess) {
      return next({ name: "forbidden" });
    }

    return next();
  });
}

function functioneelBeheerderAccessGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
) {
  const authStore = useAuthStore();
  const requiresFunctioneelBeheerderAccess = to.matched.some(
    (record) => record.meta.requiresFunctioneelBeheerderAccess
  );

  return authGuard(to, from, () => {
    if (requiresFunctioneelBeheerderAccess && !authStore.hasFunctioneelBeheerderAccess) {
      return next({ name: "forbidden" });
    }

    return next();
  });
}

function titleGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
) {
  const appTitle = document.title.split(" | ").pop() || document.title;
  document.title = `${to.meta?.title ? to.meta.title + " | " : ""}${appTitle}`;

  document.body.setAttribute("tabindex", "-1");
  document.body.focus();
  document.body.removeAttribute("tabindex");

  return next();
}

async function initializeAuthGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
) {
  if (to.name === "login") {
    return next();
  }

  const authStore = useAuthStore();
  if (!authStore.initialized) {
    try {
      await authStore.initialize();
    } catch (error) {
      console.error("Auth initialization failed:", error);
    }
  }

  return next();
}

export default {
  install(app: App, router: Router) {
    router.beforeEach(initializeAuthGuard);
    router.beforeEach(titleGuard);
    router.beforeEach(authGuard);
    router.beforeEach(itaAccessGuard);
    router.beforeEach(functioneelBeheerderAccessGuard);
  }
};
