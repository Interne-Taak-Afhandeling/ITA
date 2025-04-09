import { inject, type App } from "vue";
import { sanitizeSvg } from "./helpers";
import { get } from '@/utils/fetchWrapper';

export type Resources = Partial<{
  theme: string;
  logoUrl: string;
  faviconUrl: string;
  tokensUrl: string;
}>;

export const injectResources = () => inject<Resources | null>("resources", null);

const getResources = async (): Promise<Resources> => {
  try {
    return await get<Resources>("/api/environment/resources");
  } catch {
    return {};
  }
};

const setTheme = (theme?: string) => theme && document.body.classList.add(theme);

const setIcon = (url?: string) =>
  url && ((document.querySelector("link[rel~='icon']") as HTMLLinkElement).href = url);

const appendSvgTemplate = async (url: string): Promise<{ url: string }> => {
  try {
    const response = await fetch(url);
    const svg = await response.text();

    document.body.appendChild(
      Object.assign(document.createElement("template"), {
        id: btoa(url),
        innerHTML: sanitizeSvg(svg)
      })
    );

    return Promise.resolve({ url });
  } catch {
    return Promise.reject({ url });
  }
};

const linkResource = async (url: string) =>
  new Promise<{ url: string }>((resolve, reject) => {
    const link = document.createElement("link");

    link.rel = url.endsWith(".css") ? "stylesheet" : "preload";

    if (link.rel === "preload") {
      link.as = "image";
    }

    link.href = url;
    link.crossOrigin = "anonymous";

    link.onload = () => resolve({ url });
    link.onerror = () => reject({ url });

    document.head.appendChild(link);
  });

const loadResources = async (sources: (string | undefined)[]) => {
  const promises = sources
    .filter((url): url is string => typeof url === "string" && url.trim() !== "")
    .map((url) => (url.endsWith(".svg") ? appendSvgTemplate(url) : linkResource(url)));

  const results = await Promise.allSettled(promises);

  const rejected = results.filter(
    (result): result is PromiseRejectedResult => result.status === "rejected"
  );

  if (rejected.length) throw new Error(rejected.map((r) => r.reason.url).join(", "));
};

export const loadThemeResources = async (app: App): Promise<void> => {
  // First fetch the references to external resources from the API
  const resources = await getResources();

  try {
    // Then load the external resources if provided: theme tokens, logo
    // (this is done before mounting the app to prevent layout shifts)
    // Tokens will be loaded directly (as unlayered css, to be sure it takes precedence over the layered project css)
    // Images will be preloaded, waiting to be referenced from the app
    // Svgs will be fetched and appended as a template for further referencing
    await loadResources([resources.tokensUrl, resources.logoUrl]);

    // Replace the provided favicon link
    setIcon(resources.faviconUrl);

    // Apply the associated theme class to the root element of the app
    setTheme(resources.theme);

    // Provide references to the loaded resources to the app
    app.provide("resources", resources);
  } catch (error) {
    // Log an error if any external resources fail to load
    console.error(`One or more external resources failed to load. ${error}`);
  }
};
