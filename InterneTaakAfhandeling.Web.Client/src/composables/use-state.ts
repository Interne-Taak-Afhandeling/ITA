import { ref, type Ref } from "vue";

interface cacheItems {
  [key: string]: Ref<unknown | undefined>;
}

const cache: cacheItems = {};

export function useState<T>(contextKey: string, itemKey: string) {
  const fullCacheKey = contextKey + itemKey;

  if (!cache[fullCacheKey]) {
    cache[fullCacheKey] = ref<T | undefined>(undefined);
  }

  return {
    filterValue: cache[fullCacheKey] as Ref<T | undefined>
  };
}
