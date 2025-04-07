import { createRouter, createWebHistory } from "vue-router";
import DashboardView from "@/views/DashboardView.vue";
import AfdelingscontactenView from "@/views/AfdelingscontactenView.vue";
import HistorieView from "@/views/HistorieView.vue";
import LoginView from "@/views/LoginView.vue";
import { useAuthStore } from "@/stores/auth";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "dashboard",
      component: DashboardView,
      meta: {
        title: "Dashboard",
        requiresAuth: true
      }
    },
    {
      path: "/afdelingscontacten",
      name: "afdelingscontacten",
      component: AfdelingscontactenView,
      meta: {
        title: "Afdelingscontacten",
        requiresAuth: true
      }
    },
    {
      path: "/historie",
      name: "historie",
      component: HistorieView,
      meta: {
        title: "Historie",
        requiresAuth: true
      }
    },
    {
      path: "/login",
      name: "login",
      component: LoginView,
      meta: {
        title: "Inloggen"
      }
    }
  ]
});

const title = document.title;

router.beforeEach(async (to, from, next) => {
  // Set document title
  document.title = `${to.meta?.title ? to.meta.title + " | " : ""}${title}`;
  document.body.setAttribute("tabindex", "-1");
  document.body.focus();
  document.body.removeAttribute("tabindex");

  // Handle auth guard
  const authStore = useAuthStore();
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);

  if (requiresAuth && !authStore.isAuthenticated) {
    // Redirect to login with the intended destination
    return next({ 
      name: 'login', 
      query: { returnUrl: to.fullPath }
    });
  }

  // Handle admin routes if needed
  const requiresAdmin = to.matched.some(record => record.meta.requiresAdmin);
  if (requiresAdmin && !authStore.isAdmin) {
    return next({ name: 'dashboard' });
  }

  next();
});

export default router;
