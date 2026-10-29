import { ref, watch } from "vue";
import { useRouter, type RouteLocationNormalizedGeneric } from "vue-router";
import {
  setWerklijstBevestiging,
  type WerklijstBevestigingData
} from "@/composables/use-werklijst-bevestiging";

const lastWerklijstRoute = ref<RouteLocationNormalizedGeneric>();

const WERKLIJST_ROUTE_NAMES = ["dashboard", "afdelingsContacten", "alleContactverzoeken"];

export function setupLastWerklijstRoute() {
  const { currentRoute } = useRouter();
  // immediate: true -> also remember the werklijst the app was opened on
  watch(
    currentRoute,
    (current) => {
      if (WERKLIJST_ROUTE_NAMES.includes(current.name as string)) {
        lastWerklijstRoute.value = current;
      }
    },
    { immediate: true }
  );
}

export function useReturnToWerklijst() {
  const router = useRouter();

  function returnToWerklijst(bevestiging: WerklijstBevestigingData) {
    const target = lastWerklijstRoute.value ?? router.resolve({ name: "dashboard" });
    setWerklijstBevestiging(target.name, bevestiging);
    return router.push(target.fullPath);
  }

  return { returnToWerklijst };
}
