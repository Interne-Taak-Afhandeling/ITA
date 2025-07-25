import { createRouter, createWebHistory } from "vue-router";
import DashboardView from "@/views/DashboardView.vue";
import AllContactverzoekenView from "@/views/AllContactverzoekenView.vue";
import LoginView from "@/views/LoginView.vue";
import ForbiddenView from "@/views/ForbiddenView.vue";
import ContactverzoekDetailView from "@/views/ContactverzoekDetailView.vue";
import HistorieView from "@/views/HistorieView.vue";
import AfdelingsContactenView from "@/views/AfdelingsContactenView.vue";
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
      component: AllContactverzoekenView,
      meta: {
        title: "Alle contactverzoeken",
        requiresITAAccess: true
      }
    },
    //Historie
    {
      path: "/historie",
      name: "historie",
      component: HistorieView,
      meta: {
        title: "historie",
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
    },
    {
      path: "/afdelings-contacten",
      name: "afdelingsContacten",
      component: AfdelingsContactenView,
      meta: {
        title: "Afdelings Contacten",
        requiresITAAccess: true
      }
    }
  ]
});

export default router;
