import { ref, type Ref, watch } from "vue";

interface cacheItems {
  [key: string]: Ref<string | undefined>;
}

const cache: cacheItems = {};

export function useActorFilter(cacheKey: string) {
  if (!cache[cacheKey]) {
    cache[cacheKey] = ref<string>("");
  }

  watch(cache[cacheKey], (newValue: string | undefined) => {
    cache[cacheKey].value = newValue;
  });

  return {
    filterValue: cache[cacheKey]
  };
}
