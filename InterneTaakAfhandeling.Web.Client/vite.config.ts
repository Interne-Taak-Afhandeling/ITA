import { fileURLToPath, URL } from "node:url";

import { defineConfig, HttpProxy } from "vite";
import vue from "@vitejs/plugin-vue";
import { env } from "process";

const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
    ? env.ASPNETCORE_URLS.split(";")[0]
    : "http://localhost:8080";
// Proxy configuration with different options per category
const proxyConfig: Record<string, { endpoints: string[]; options: HttpProxy.ServerOptions }> = {
  api: {
    endpoints: ["/api"],
    options: {
      target,
      secure: false,
      headers: {
        "X-Forwarded-Proto": "http"
      }
    }
  },
  auth: {
    endpoints: ["/signin-oidc", "/signout-callback-oidc"],
    options: {
      target,
      secure: false,
      headers: {
        "X-Forwarded-Proto": "http"
      }
    }
  }
};

// Create proxy entries from the configuration
const proxyEntries = Object.entries(proxyConfig).flatMap(([, config]) =>
  config.endpoints.map((endpoint) => [endpoint, config.options])
);

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url))
    }
  },
  server: {
    proxy: Object.fromEntries(proxyEntries),
    port: 56175
  },
  build: {
    assetsInlineLimit: 0
  }
});
