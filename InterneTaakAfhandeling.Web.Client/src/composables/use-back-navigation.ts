import { ref, computed } from "vue";
import { useRouter } from "vue-router";

const previousRoute = ref<string | null>(null);

export function useBackNavigation() {
  const router = useRouter();

  const setPreviousRoute = (routeName: string) => {
    previousRoute.value = routeName;
  };

  const backButtonInfo = computed(() => {
    switch (previousRoute.value) {
      case "dashboard":
        return {
          text: "Terug naar Dashboard",
          route: { name: "dashboard" }
        };
      case "alleContactverzoeken":
        return {
          text: "Terug naar Contactverzoeken",
          route: { name: "alleContactverzoeken" }
        };
      default:
        return {
          text: "Terug",
          route: { name: "dashboard" }
        };
    }
  });

  const goBack = () => {
    if (backButtonInfo.value.route) {
      router.push(backButtonInfo.value.route);
    } else {
      router.push({ name: "dashboard" });
    }
  };

  return {
    setPreviousRoute,
    backButtonInfo,
    goBack
  };
}
