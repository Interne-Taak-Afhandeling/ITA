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

<style lang="scss">
@import "@utrecht/pagination-css";

.utrecht-pagination {
  display: flex;
  justify-content: center;
  gap: 1rem;
}

.utrecht-pagination__pages {
  display: flex;

  gap: var(--utrecht-space-inline-xs, 0.25rem);
}

// .utrecht-pagination__relative-link,
// .utrecht-pagination__page-link {
//   all: unset;
//   box-sizing: border-box;
//   display: inline-flex;

//   padding-block: var(--utrecht-button-padding-block-start);
//   padding-inline: var(--utrecht-button-padding-inline-start);
//   border: var(--utrecht-button-border-width) solid;
//   border-color: var(--utrecht-button-border-color);
//   border-radius: var(--utrecht-button-border-radius);
//   background-color: var(--utrecht-button-background-color);
//   color: var(--utrecht-button-color);
//   font-family: var(--utrecht-button-font-family);
//   font-size: var(--utrecht-button-font-size);
//   font-weight: var(--utrecht-button-font-weight);
//   line-height: var(--utrecht-button-line-height);
//   text-decoration: none;
//   cursor: pointer;
//   touch-action: manipulation;
//   user-select: none;
//   transition:
//     background-color 0.2s ease,
//     border-color 0.2s ease,
//     color 0.2s ease;
// }

// .utrecht-pagination__relative-link--disabled,
// .utrecht-pagination__page-link:disabled {
//   opacity: var(--utrecht-button-disabled-opacity);
//   cursor: not-allowed;
//   background-color: var(--utrecht-button-disabled-background-color);
//   border-color: var(--utrecht-button-disabled-border-color);
//   color: var(--utrecht-button-disabled-color);
// }

// .utrecht-pagination__page-link--current,
// .utrecht-pagination__page-link:hover,
// .utrecht-pagination__relative-link:hover {
//   background-color: var(--utrecht-button-primary-action-background-color);
//   border-color: var(--utrecht-button-primary-action-border-color);
//   color: var(--utrecht-button-primary-action-color);
//   cursor: default;
// }
</style>
