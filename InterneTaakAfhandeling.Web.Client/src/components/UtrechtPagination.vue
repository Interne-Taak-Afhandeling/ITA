<template>
  <nav class="utrecht-pagination" v-if="totalPages > 1">
    <span class="utrecht-pagination__before">
      <button
        v-if="hasPreviousPage"
        @click="goToPreviousPage"
        :disabled="isLoading"
        class="utrecht-pagination__relative-link utrecht-pagination__relative-link--prev"
        aria-label="Ga naar vorige pagina"
      >
        Vorige
      </button>
      <span
        v-else
        class="utrecht-pagination__relative-link utrecht-pagination__relative-link--prev utrecht-pagination__relative-link--disabled"
        aria-label="Geen vorige pagina beschikbaar"
      >
        Vorige
      </span>
    </span>

    <span role="group" class="utrecht-pagination__pages" aria-label="Paginanummers">
      <button
        v-for="pageNum in visiblePages"
        :key="pageNum"
        @click="goToPage(pageNum)"
        :disabled="isLoading"
        :class="[
          'utrecht-pagination__page-link',
          pageNum === currentPage && 'utrecht-pagination__page-link--current'
        ]"
        :aria-current="pageNum === currentPage ? 'page' : undefined"
        :aria-label="`Ga naar pagina ${pageNum}`"
      >
        {{ pageNum }}
      </button>
    </span>

    <span class="utrecht-pagination__after">
      <button
        v-if="hasNextPage"
        @click="goToNextPage"
        :disabled="isLoading"
        class="utrecht-pagination__relative-link utrecht-pagination__relative-link--next"
        aria-label="Ga naar volgende pagina"
      >
        Volgende
      </button>
      <span
        v-else
        class="utrecht-pagination__relative-link utrecht-pagination__relative-link--next utrecht-pagination__relative-link--disabled"
        aria-label="Geen volgende pagina beschikbaar"
      >
        Volgende
      </span>
    </span>
  </nav>
</template>

<script setup lang="ts">
interface Props {
  currentPage: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  visiblePages: number[];
  isLoading?: boolean;
}

defineProps<Props>();

const emit = defineEmits<{
  goToPage: [page: number];
  goToPreviousPage: [];
  goToNextPage: [];
}>();

const goToPage = (page: number) => {
  emit("goToPage", page);
};

const goToPreviousPage = () => {
  emit("goToPreviousPage");
};

const goToNextPage = () => {
  emit("goToNextPage");
};
</script>
