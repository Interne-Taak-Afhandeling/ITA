import { createRouter, createWebHistory } from "vue-router";
import DashboardView from "@/views/DashboardView.vue";
import AfdelingscontactenView from "@/views/AfdelingscontactenView.vue";
import HistorieView from "@/views/HistorieView.vue";
import TestView from "@/views/TestView.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "dashboard",
      component: DashboardView,
      meta: {
        title: "Dashboard"
      }
    },
    {
      path: "/afdelingscontacten",
      name: "afdelingscontacten",
      component: AfdelingscontactenView,
      meta: {
        title: "Afdelingscontacten"
      }
    },
    {
      path: "/historie",
      name: "historie",
      component: HistorieView,
      meta: {
        title: "Historie"
      }
    },
    {
      path: "/test",
      name: "test",
      component: TestView,
      meta: {
        title: "Database Test"
      }
    }
  ]
});

const title = document.title;

router.beforeEach((to) => {
  document.title = `${to.meta?.title ? to.meta.title + " | " : ""}${title}`;
  document.body.setAttribute("tabindex", "-1");
  document.body.focus();
  document.body.removeAttribute("tabindex");
});

export default router;
