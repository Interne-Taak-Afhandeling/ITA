import "./assets/main.scss";

import { createApp } from "vue";
import App from "./App.vue";
import { registerComponents } from "@/components/register";
import { loadThemeResources } from "./resources";
import { pinia } from "./stores";
import "@/plugins/axios";

const app = createApp(App);

app.use(pinia);
registerComponents(app);

(async () => {
  // Load external theme resources before app mounts to prevent layout shifts
  await loadThemeResources(app);

  // Load router after theme, to be able to use theme settings
  const { default: router } = await import("./router");
     
  app.use(router);

  // Initialize auth store
  const { useAuthStore } = await import("./stores/auth");
  const authStore = useAuthStore();
  await authStore.initialize();

  app.mount("#app");
})();
