import { fileURLToPath, URL } from "node:url";

import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import fs from "fs";
import path from "path";
import child_process from "child_process";
import { env } from "process";
import vueDevTools from 'vite-plugin-vue-devtools'

const baseFolder =
  env.APPDATA !== undefined && env.APPDATA !== ""
    ? `${env.APPDATA}/ASP.NET/https`
    : `${env.HOME}/.aspnet/https`;

const certificateName = "InterneTaakAfhandeling.Web.Client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(baseFolder)) {
  fs.mkdirSync(baseFolder, { recursive: true });
}

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
  if (
    0 !==
    child_process.spawnSync(
      "dotnet",
      ["dev-certs", "https", "--export-path", certFilePath, "--format", "Pem", "--no-password"],
      { stdio: "inherit" },
    ).status
  ) {
    throw new Error("Could not create certificate.");
  }
}

const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
    ? env.ASPNETCORE_URLS.split(";")[0]
    : "https://localhost:7272";
    const proxyCalls = ["/api", "/signin-oidc", "/signout-callback-oidc"];
// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue(),vueDevTools()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  server: {
    proxy:  Object.fromEntries(
      proxyCalls.map((key) => [
        key,
        {
            target,
          secure: false
        }
      ])
    ),
    port: 56175,
    https: {
      key: fs.readFileSync(keyFilePath),
      cert: fs.readFileSync(certFilePath),
    },
  },
});
