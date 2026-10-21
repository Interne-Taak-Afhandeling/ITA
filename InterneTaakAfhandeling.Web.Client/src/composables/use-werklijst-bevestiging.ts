import { ref } from "vue";
import { useRouter, type RouteRecordNameGeneric } from "vue-router";
import type { AlertType } from "@/components/UtrechtAlert.vue";

export interface WerklijstBevestiging {
  type: AlertType;
  tekst: string;
}

const bevestiging = ref<WerklijstBevestiging>();
let getoondOpRoute: RouteRecordNameGeneric | null | undefined;

export function setWerklijstBevestiging(
  routeName: RouteRecordNameGeneric | null,
  value: WerklijstBevestiging
) {
  getoondOpRoute = routeName;
  bevestiging.value = value;
}

export function clearWerklijstBevestiging() {
  getoondOpRoute = undefined;
  bevestiging.value = undefined;
}

// AC: confirmation stays until the user closes it or navigates to another page
export function setupWerklijstBevestiging() {
  const router = useRouter();
  router.afterEach((to) => {
    if (bevestiging.value && to.name !== getoondOpRoute) clearWerklijstBevestiging();
  });
}

export function useWerklijstBevestiging() {
  return { bevestiging, clearWerklijstBevestiging };
}
