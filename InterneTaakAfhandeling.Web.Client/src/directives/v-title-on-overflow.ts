import type { ObjectDirective } from "vue";

const observer = new ResizeObserver((x) => {
  x.forEach(({ target }) => {
    if (!(target instanceof HTMLElement)) return;
    const isOverflowing = target.offsetWidth < target.scrollWidth;
    if (isOverflowing && !target.title) {
      target.title = target.textContent || "";
    }
    if (!isOverflowing && target.title) {
      target.title = "";
    }
  });
});

export const vTitleOnOverflow: ObjectDirective<HTMLElement> = {
  mounted(el) {
    observer.observe(el);
  },
  unmounted(el) {
    observer.unobserve(el);
  }
};
