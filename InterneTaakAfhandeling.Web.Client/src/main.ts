import "./assets/main.scss";

import { createApp } from "vue";
import App from "./App.vue";
import { registerComponents } from "@/components/register";
import { loadThemeResources } from "./resources";

const app = createApp(App);

registerComponents(app);

(async () => {
  // Load external theme resources before app mounts to prevent layout shifts
  await loadThemeResources(app);

  // Load router after theme, to be able to use theme settings
  const { default: router } = await import("./router");

  app.use(router);

  app.mount("#app");
})();
