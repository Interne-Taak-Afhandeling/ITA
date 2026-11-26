import { ref } from "vue";
import { get } from "@/utils/fetchWrapper";

export interface MedewerkerData {
  naam: string;
  identificatie: string;
  afdelingen: { afdelingnaam: string }[];
  groepen: { groepsnaam: string }[];
}

const MIN_QUERY_LENGTH = 2;
const DEBOUNCE_MS = 300;

/**
 * Debounced server-side search for medewerkers in the objectenregister.
 * Older requests are aborted when a new search starts.
 */
export function useMedewerkerSearch() {
  const isLoading = ref(false);
  const results = ref<MedewerkerData[]>([]);

  let debounceTimer: ReturnType<typeof setTimeout> | null = null;
  let abortController: AbortController | null = null;

  function search(query: string) {
    if (debounceTimer) clearTimeout(debounceTimer);
    if (abortController) abortController.abort();
    if (!query || query.length < MIN_QUERY_LENGTH) {
      results.value = [];
      isLoading.value = false;
      return;
    }
    isLoading.value = true;
    debounceTimer = setTimeout(async () => {
      const controller = new AbortController();
      abortController = controller;
      try {
        results.value = await get<MedewerkerData[]>(
          "/api/medewerkers",
          { search: query },
          { signal: controller.signal }
        );
      } catch (e) {
        if (e instanceof DOMException && e.name === "AbortError") return;
        results.value = [];
      } finally {
        // an aborted request must not end the loading state of the newer one
        if (abortController === controller) isLoading.value = false;
      }
    }, DEBOUNCE_MS);
  }

  return { isLoading, results, search };
}
