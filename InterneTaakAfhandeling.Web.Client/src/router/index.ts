import { createRouter, createWebHistory } from "vue-router";
import { authGuard, adminGuard, titleGuard } from "@/plugins/routerGuards";
import DashboardView from "@/views/DashboardView.vue";
import AfdelingscontactenView from "@/views/AfdelingscontactenView.vue";
import HistorieView from "@/views/HistorieView.vue";
import LoginView from "@/views/LoginView.vue";

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

router.beforeEach(titleGuard);
router.beforeEach(authGuard);
router.beforeEach(adminGuard);

export default router;
