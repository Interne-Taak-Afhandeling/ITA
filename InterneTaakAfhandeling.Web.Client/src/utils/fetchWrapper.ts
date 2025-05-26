import { useAuthStore } from "@/stores/auth";

interface FetchOptions extends RequestInit {
  skipAuthCheck?: boolean;
}

export async function fetchWrapper<T = unknown>(
  url: string,
  options: FetchOptions = {}
): Promise<T> {
  const { skipAuthCheck = false, ...fetchOptions } = options;

  const headers = new Headers((fetchOptions.headers as HeadersInit) || {});

  if (!headers.has("Content-Type") && !(fetchOptions.body instanceof FormData)) {
    headers.set("Content-Type", "application/json");
  }
  fetchOptions.headers = headers;

  try {
    const response = await fetch(url, fetchOptions);

    if (response.ok) {
      const contentType = response.headers.get("content-type");
      return contentType?.includes("application/json")
        ? ((await response.json()) as T)
        : ((await response.text()) as unknown as T);
    }

    if (response.status === 401 && !skipAuthCheck) {
      const authStore = useAuthStore();
      if (authStore) {
        await authStore.login();
        return Promise.reject(new Error("Authentication required"));
      }
    }

    let errorMessage = `Request failed with status ${response.status}`;
    try {
      const errorData = await response.json();
      errorMessage = errorData.message || errorData.error || errorMessage;
    } catch {
      errorMessage = response.statusText || errorMessage;
    }

    throw new Error(errorMessage);
  } catch (error) {
    throw error;
  }
}
function toQueryString(params: Record<string, unknown>): string {
  const searchParams = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null) {
      searchParams.append(key, String(value));
    }
  });

  return searchParams.toString();
}

export const get = <T = unknown>(
  url: string,
  query?: Record<string, unknown>,
  options: FetchOptions = {}
): Promise<T> => {
  const queryString = query ? `?${toQueryString(query)}` : "";
  return fetchWrapper<T>(`${url}${queryString}`, { method: "GET", ...options });
};

export const post = <T = unknown>(
  url: string,
  data: unknown,
  options: FetchOptions = {}
): Promise<T> => fetchWrapper<T>(url, { method: "POST", body: JSON.stringify(data), ...options });

export const put = <T = unknown>(
  url: string,
  data: unknown,
  options: FetchOptions = {}
): Promise<T> => fetchWrapper<T>(url, { method: "PUT", body: JSON.stringify(data), ...options });

export const del = <T = unknown>(url: string, options: FetchOptions = {}): Promise<T> =>
  fetchWrapper<T>(url, { method: "DELETE", ...options });

export const patch = <T = unknown>(
  url: string,
  data: unknown,
  options: FetchOptions = {}
): Promise<T> => fetchWrapper<T>(url, { method: "PATCH", body: JSON.stringify(data), ...options });
