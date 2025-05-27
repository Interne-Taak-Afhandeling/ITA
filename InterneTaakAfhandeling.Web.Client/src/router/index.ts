import { createRouter, createWebHistory } from "vue-router";
import DashboardView from "@/views/DashboardView.vue";
import AlleContactverzoekenView from "@/views/AlleContactverzoekenView.vue"; 
import HistorieView from "@/views/HistorieView.vue";
import LoginView from "@/views/LoginView.vue";
import ForbiddenView from "@/views/ForbiddenView.vue";
import ContactverzoekDetailView from "@/views/ContactverzoekDetailView.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "dashboard",
      component: DashboardView,
      meta: {
        title: "Dashboard",
        requiresITAAccess: true
      }
    },
    {
      path: "/alle-contactverzoeken", 
      name: "alleContactverzoeken", 
      component: AlleContactverzoekenView, 
      meta: {
        title: "Alle contactverzoeken", 
        requiresITAAccess: true
      }
    },
    {
      path: "/historie",
      name: "historie",
      component: HistorieView,
      meta: {
        title: "Historie",
        requiresITAAccess: true
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
    },
    {
      path: "/contactverzoek/:number",
      name: "contactverzoekDetail",
      component: ContactverzoekDetailView,
      meta: {
        title: "Contactverzoek",
        requiresITAAccess: true
      }
    }
  ]
});

export default router;