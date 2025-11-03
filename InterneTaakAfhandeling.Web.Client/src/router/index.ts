import { createRouter, createWebHistory } from "vue-router";
import DashboardView from "@/views/DashboardView.vue";
import AllContactverzoekenView from "@/views/AllContactverzoekenView.vue";
import LoginView from "@/views/LoginView.vue";
import ForbiddenView from "@/views/ForbiddenView.vue";
import ContactverzoekDetailView from "@/views/ContactverzoekDetailView.vue";
import HistorieView from "@/views/HistorieView.vue";
import AfdelingsContactenView from "@/views/AfdelingsContactenView.vue";
import BeheerView from "@/views/beheer/BeheerLayout.vue";
import KanalenView from "@/views/beheer/Kanalen/KanalenView.vue";
import AfdelingsContactenHistorieView from "@/views/AfdelingsContactenHistorieView.vue";
import KanaalView from "@/views/beheer/Kanalen/KanaalView.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "dashboard",
      component: DashboardView,
      meta: {
        title: "Mijn werkvoorraad",
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
        title: "Mijn historie",
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
        title: "Afdelingswerkvoorraad",
        requiresITAAccess: true
      }
    },
    {
      path: "/afdelings-contacten-historie",
      name: "afdelingsContactenHistorie",
      component: AfdelingsContactenHistorieView,
      meta: {
        title: "Afdelingshistorie",
        requiresITAAccess: true
      }
    },
    {
      path: "/beheer",
      name: "beheer",
      component: BeheerView,
      meta: {
        title: "Beheer",
        requiresITAAccess: true,
        requiresFunctioneelBeheerderAccess: true
      },
      children: [
        {
          path: "kanalen",
          name: "kanalen",
          component: KanalenView,
          meta: {
            title: "Kanalen",
            requiresITAAccess: true,
            requiresFunctioneelBeheerderAccess: true
          }
        },
        {
          path: "kanaal/:id?",
          name: "kanaal",
          component: KanaalView,
          meta: {
            title: "Kanaal",
            requiresITAAccess: true,
            requiresFunctioneelBeheerderAccess: true
          }
        }
      ]
    }
  ]
});

export default router;
