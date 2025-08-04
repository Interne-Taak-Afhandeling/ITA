import { ref, type Ref } from "vue";

interface cacheItems {
  [key: string]: Ref<string | undefined>;
}

const cache: cacheItems = {};

export function useActorFilter(cacheKey: string) {
  if (!cache[cacheKey]) {
    cache[cacheKey] = ref<string>("");
  }

  return {
    filterValue: cache[cacheKey]
  };
}
