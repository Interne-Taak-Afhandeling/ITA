import { createRouter, createWebHistory } from "vue-router";
import DashboardView from "@/views/DashboardView.vue";
import AfdelingscontactenView from "@/views/AfdelingscontactenView.vue";
import HistorieView from "@/views/HistorieView.vue";
import LoginView from "@/views/LoginView.vue";
import ForbiddenView from "@/views/ForbiddenView.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "dashboard",
      component: DashboardView,
      meta: {
        title: "Dashboard", 
        requiresAdmin: true
      }
    },
    {
      path: "/afdelingscontacten",
      name: "afdelingscontacten",
      component: AfdelingscontactenView,
      meta: {
        title: "Afdelingscontacten", 
        requiresAdmin: true
      }
    },
    {
      path: "/historie",
      name: "historie",
      component: HistorieView,
      meta: {
        title: "Historie", 
        requiresAdmin: true
      }
    },
    {
      path: "/login",
      name: "login",
      component: LoginView,
      meta: {
        title: "Inloggen"
      }
    },
    {
      path: "/forbidden",
      name: "forbidden",
      component: ForbiddenView,
      meta: {
        title: "Toegang Geweigerd",
        requiresAuth: true
      }
    }
  ]
});
 
export default router;
