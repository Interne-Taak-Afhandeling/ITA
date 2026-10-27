import { ref, computed, watch } from "vue";
import { useRouter, type RouteLocationNormalizedGeneric } from "vue-router";

const previousRoute = ref<RouteLocationNormalizedGeneric>();

const DASHBOARD_ROUTE = {
  text: "Terug naar Dashboard",
  route: { name: "dashboard" }
};

export function setupPreviousRoute() {
  const { currentRoute } = useRouter();
  watch(currentRoute, (_, old) => {
    previousRoute.value = old;
  });
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
