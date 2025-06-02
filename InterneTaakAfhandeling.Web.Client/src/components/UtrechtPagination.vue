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
  emit('goToPage', page);
};

const goToPreviousPage = () => {
  emit('goToPreviousPage');
};

const goToNextPage = () => {
  emit('goToNextPage');
};
</script>

<style lang="scss">
.utrecht-pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--utrecht-space-inline-xs, 0.25rem);
  margin: var(--utrecht-space-block-md, 1rem) 0;
  font-family: var(--utrecht-document-font-family, inherit);
  font-size: var(--utrecht-document-font-size, 1rem);
  line-height: var(--utrecht-document-line-height, 1.5);
}

.utrecht-pagination__before,
.utrecht-pagination__after {
  display: flex;
  align-items: center;
}

.utrecht-pagination__pages {
  display: flex;
  align-items: center;
  gap: var(--utrecht-space-inline-xs, 0.25rem);
  margin: 0 var(--utrecht-space-inline-sm, 0.5rem);
}

.utrecht-pagination__relative-link,
.utrecht-pagination__page-link {
  all: unset;
  box-sizing: border-box;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: var(--utrecht-button-min-block-size, 2.5rem);
  min-width: var(--utrecht-button-min-inline-size, 2.5rem);
  padding-block: var(--utrecht-button-padding-block-start, 0.5rem);
  padding-inline: var(--utrecht-button-padding-inline-start, 0.75rem);
  border: var(--utrecht-button-border-width, 1px) solid;
  border-color: var(--utrecht-button-border-color, var(--utrecht-color-grey-50));
  border-radius: var(--utrecht-button-border-radius, 0.25rem);
  background-color: var(--utrecht-button-background-color, var(--utrecht-color-white));
  color: var(--utrecht-button-color, var(--utrecht-color-grey-90));
  font-family: var(--utrecht-button-font-family, inherit);
  font-size: var(--utrecht-button-font-size, inherit);
  font-weight: var(--utrecht-button-font-weight, 400);
  line-height: var(--utrecht-button-line-height, normal);
  text-decoration: none;
  cursor: pointer;
  touch-action: manipulation;
  user-select: none;
  transition: 
    background-color 0.2s ease,
    border-color 0.2s ease,
    color 0.2s ease;

  &:hover:not(.utrecht-pagination__relative-link--disabled) {
    background-color: var(--utrecht-button-hover-background-color, var(--utrecht-color-grey-5));
    border-color: var(--utrecht-button-hover-border-color, var(--utrecht-color-grey-60));
    color: var(--utrecht-button-hover-color, var(--utrecht-color-grey-90));
  }

  &:focus {
    outline: var(--utrecht-focus-outline-width, 2px) solid var(--utrecht-focus-outline-color, var(--utrecht-color-green-30));
    outline-offset: var(--utrecht-focus-outline-offset, 2px);
  }

  &:active:not(.utrecht-pagination__relative-link--disabled) {
    background-color: var(--utrecht-button-active-background-color, var(--utrecht-color-grey-10));
    border-color: var(--utrecht-button-active-border-color, var(--utrecht-color-grey-70));
  }
}

.utrecht-pagination__relative-link--disabled {
  opacity: var(--utrecht-button-disabled-opacity, 0.5);
  cursor: not-allowed;
  background-color: var(--utrecht-button-disabled-background-color, var(--utrecht-color-grey-5));
  border-color: var(--utrecht-button-disabled-border-color, var(--utrecht-color-grey-30));
  color: var(--utrecht-button-disabled-color, var(--utrecht-color-grey-50));
  
  &:hover {
    background-color: var(--utrecht-button-disabled-background-color, var(--utrecht-color-grey-5));
    border-color: var(--utrecht-button-disabled-border-color, var(--utrecht-color-grey-30));
    color: var(--utrecht-button-disabled-color, var(--utrecht-color-grey-50));
  }
}

.utrecht-pagination__page-link--current {
  background-color: var(--utrecht-button-primary-action-background-color, var(--utrecht-color-green-30));
  border-color: var(--utrecht-button-primary-action-border-color, var(--utrecht-color-green-30));
  color: var(--utrecht-button-primary-action-color, var(--utrecht-color-white));
  cursor: default;

  &:hover {
    background-color: var(--utrecht-button-primary-action-hover-background-color, var(--utrecht-color-green-40));
    border-color: var(--utrecht-button-primary-action-hover-border-color, var(--utrecht-color-green-40));
    color: var(--utrecht-button-primary-action-hover-color, var(--utrecht-color-white));
  }

  &:focus {
    outline: var(--utrecht-focus-outline-width, 2px) solid var(--utrecht-focus-outline-color, var(--utrecht-color-green-50));
    outline-offset: var(--utrecht-focus-outline-offset, 2px);
  }
}
</style>