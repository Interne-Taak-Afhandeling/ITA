import { useRouter } from "vue-router";

export function useRowNavigation() {
  const router = useRouter();

  // Track where the press started so click can tell a drag from a tap.
  let x = 0,
    y = 0;

  const onRowMouseDown = (e: MouseEvent) => {
    x = e.clientX;
    y = e.clientY;
  };

  const navigateOnRowClick = (e: MouseEvent, path: string) => {
    // Let clicks on links behave as links, not row navigation.
    if ((e.target as HTMLElement).closest("a")) return;

    // Skip nav if the user was drag-selecting text (pointer moved >5px).
    if (Math.abs(e.clientX - x) > 5 || Math.abs(e.clientY - y) > 5) return;

    router.push(path);
  };

  return { onRowMouseDown, navigateOnRowClick };
}
