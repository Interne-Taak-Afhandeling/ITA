import { ref, computed, watch } from "vue";
import { useRouter, type RouteLocationNormalizedGeneric } from "vue-router";
import {
  setWerklijstBevestiging,
  type WerklijstBevestiging
} from "@/composables/use-werklijst-bevestiging";

const previousRoute = ref<RouteLocationNormalizedGeneric>();
const lastWerklijstRoute = ref<RouteLocationNormalizedGeneric>();

const DASHBOARD_ROUTE = {
  text: "Terug naar Dashboard",
  route: { name: "dashboard" }
};

const WERKLIJST_ROUTE_NAMES = ["dashboard", "afdelingsContacten", "alleContactverzoeken"];

export function setupPreviousRoute() {
  const { currentRoute } = useRouter();
  watch(currentRoute, (_, old) => {
    previousRoute.value = old;
  });
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

export function useBackNavigation() {
  const backButtonInfo = computed(() => {
    if (!previousRoute.value) return DASHBOARD_ROUTE;
    return {
      route: previousRoute.value,
      text: previousRoute.value.meta.title
        ? `Terug naar ${previousRoute.value.meta.title}`
        : "Terug"
    };
  });

  return {
    backButtonInfo
  };
}

export function useReturnToWerklijst() {
  const router = useRouter();

  function returnToWerklijst(bevestiging: WerklijstBevestiging) {
    const target = lastWerklijstRoute.value ?? router.resolve(DASHBOARD_ROUTE.route);
    setWerklijstBevestiging(target.name, bevestiging);
    return router.push(target.fullPath);
  }

  return { returnToWerklijst };
}
