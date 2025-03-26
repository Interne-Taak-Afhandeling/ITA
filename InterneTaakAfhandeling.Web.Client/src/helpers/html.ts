import DOMPurify from "dompurify";

export function sanitizeSvg(dirtySvg: string) {
  return DOMPurify.sanitize(dirtySvg, { USE_PROFILES: { svg: true, svgFilters: true } });
}
