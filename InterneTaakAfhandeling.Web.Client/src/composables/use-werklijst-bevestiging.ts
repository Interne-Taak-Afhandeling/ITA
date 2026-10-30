import { ref } from "vue";
import { useRouter, type RouteRecordNameGeneric } from "vue-router";
import type { AlertType } from "@/components/UtrechtAlert.vue";

export interface WerklijstBevestigingData {
  type: AlertType;
  tekst: string;
}

const bevestiging = ref<WerklijstBevestigingData>();
let getoondOpRouteNaam: RouteRecordNameGeneric | null | undefined;

export function setWerklijstBevestiging(
  routeName: RouteRecordNameGeneric | null,
  value: WerklijstBevestigingData
) {
  getoondOpRouteNaam = routeName;
  bevestiging.value = value;
}

export function clearWerklijstBevestiging() {
  getoondOpRouteNaam = undefined;
  bevestiging.value = undefined;
}

// AC: confirmation stays until the user closes it or navigates to another page
export function clearBevestigingOnNavigate() {
  const router = useRouter();
  router.afterEach((to) => {
    if (bevestiging.value && to.name !== getoondOpRouteNaam) clearWerklijstBevestiging();
  });
}

export function useWerklijstBevestiging() {
  return { bevestiging, clearWerklijstBevestiging };
}
