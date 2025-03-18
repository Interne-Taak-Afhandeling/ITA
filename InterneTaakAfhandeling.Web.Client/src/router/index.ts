import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/HomeView.vue";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      component: HomeView,
      meta: {
        title: "Homepage"
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
