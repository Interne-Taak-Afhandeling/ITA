import { ref, computed } from "vue";

export interface PaginationResponse<T = any> {
  count: number;
  next?: string;
  previous?: string;
  results: T[];
}

export interface PaginationOptions {
  initialPage?: number;
  initialPageSize?: number;
  maxVisiblePages?: number;
}

export function usePagination<T = any>(
  fetchFunction: (page: number, pageSize: number) => Promise<PaginationResponse<T>>,
  options: PaginationOptions = {}
) {
  const { initialPage = 1, initialPageSize = 20, maxVisiblePages = 5 } = options;

  const isLoading = ref(false);
  const error = ref<string | null>(null);
  const results = ref<T[]>([]);
  const totalCount = ref(0);
  const currentPage = ref(initialPage);
  const pageSize = ref(initialPageSize);
  const hasNextPage = ref(false);
  const hasPreviousPage = ref(false);

  const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value));

  const visiblePages = computed(() => {
    const total = totalPages.value;
    const current = currentPage.value;
    const maxVisible = maxVisiblePages;
    const pages: number[] = [];

    if (total <= maxVisible) {
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      let start = Math.max(1, current - Math.floor(maxVisible / 2));
      let end = Math.min(total, current + Math.floor(maxVisible / 2));

      if (end - start + 1 < maxVisible) {
        if (start === 1) {
          end = Math.min(total, start + maxVisible - 1);
        } else {
          start = Math.max(1, end - maxVisible + 1);
        }
      }

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
    }

    return pages;
  });

  const itemRange = computed(() => {
    if (totalCount.value === 0) return undefined;
    const start = (currentPage.value - 1) * pageSize.value + 1;
    const end = Math.min(currentPage.value * pageSize.value, totalCount.value);
    return { start, end };
  });

  const fetchData = async (page: number = currentPage.value) => {
    if (isLoading.value) return;

    isLoading.value = true;
    error.value = null;

    try {
      const response = await fetchFunction(page, pageSize.value);

      results.value = response.results;
      totalCount.value = response.count;
      currentPage.value = page;
      hasNextPage.value = !!response.next;
      hasPreviousPage.value = !!response.previous;
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : "Er is een fout opgetreden bij het ophalen van gegevens";
      error.value = message;
      console.error("Pagination fetch error:", err);
    } finally {
      isLoading.value = false;
    }
  };

  const goToPage = (page: number) => {
    if (page !== currentPage.value && !isLoading.value && page >= 1 && page <= totalPages.value) {
      fetchData(page);
    }
  };

  const goToNextPage = () => {
    if (hasNextPage.value && !isLoading.value) {
      goToPage(currentPage.value + 1);
    }
  };

  const goToPreviousPage = () => {
    if (hasPreviousPage.value && !isLoading.value && currentPage.value > 1) {
      goToPage(currentPage.value - 1);
    }
  };

  const reset = () => {
    currentPage.value = initialPage;
    results.value = [];
    totalCount.value = 0;
    hasNextPage.value = false;
    hasPreviousPage.value = false;
    error.value = null;
  };

  const setPageSize = (newPageSize: number) => {
    pageSize.value = newPageSize;
    currentPage.value = 1;
    fetchData(1);
  };

  return {
    // State
    isLoading,
    error,
    results,
    totalCount,
    currentPage,
    pageSize,
    hasNextPage,
    hasPreviousPage,

    // Computed
    totalPages,
    visiblePages,
    itemRange,

    // Methods
    fetchData,
    goToPage,
    goToNextPage,
    goToPreviousPage,
    reset,
    setPageSize
  };
}
