import "./assets/main.scss";

import { createApp } from "vue";
import App from "./App.vue";
import { registerComponents } from "@/components/register";
import { loadThemeResources } from "./resources";
import { pinia } from "./stores";

const app = createApp(App);

app.use(pinia);
registerComponents(app);

(async () => {
  // Load external theme resources before app mounts to prevent layout shifts
  await loadThemeResources(app);

  // Load router after theme, to be able to use theme settings
  const { default: router } = await import("./router");
  const { default: routerGuardsPlugin } = await import("./plugins/routerGuards");
     
  app.use(router);
  app.use(routerGuardsPlugin, router);

  app.mount("#app");
})();
