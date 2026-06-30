import { useRouter } from "vue-router";

export function useRowNavigation() {
  const router = useRouter();

  // Track where the press started so click can tell a drag from a tap.
  // null = no recorded start, so skip the drag check (e.g. touch/pointer).
  let start: { x: number; y: number } | null = null;

  const onRowMouseDown = (e: MouseEvent) => {
    start = { x: e.clientX, y: e.clientY };
  };

  const navigateOnRowClick = (e: MouseEvent, path: string) => {
    // Let clicks on links behave as links, not row navigation.
    if ((e.target as HTMLElement).closest("a")) return;

    // Skip nav if the user was drag-selecting text (pointer moved >5px).
    if (start) {
      const dx = Math.abs(e.clientX - start.x);
      const dy = Math.abs(e.clientY - start.y);
      start = null;
      if (dx > 5 || dy > 5) return;
    }

    router.push(path);
  };

  return { onRowMouseDown, navigateOnRowClick };
}
