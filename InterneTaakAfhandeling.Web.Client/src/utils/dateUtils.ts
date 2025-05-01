export type DateLike = string | number | Date | null | undefined;

const padZero = (v: string | number, count: number) => {
  v = v.toString();
  while (v.length < count) {
    v = "0" + v;
  }
  return v;
};

export const parseValidDate = (date: DateLike) => {
  if (!date) return undefined;
  date = new Date(date);
  if (date instanceof Date && !isNaN(date.getTime())) return date;
  return date;
};

export const formatNlDate = (date: string | number | Date | null | undefined) => {
  date = parseValidDate(date);
  if (!date) {
    return undefined;
  }
  return new Date(date).toLocaleString("nl-NL", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric"
  });
};

export const formatIsoDate = (date: DateLike) => {
  date = parseValidDate(date);
  if (!date) return undefined;
  const year = padZero(date.getFullYear(), 4),
    month = padZero(date.getMonth() + 1, 2),
    day = padZero(date.getDate(), 2);
  return [year, month, day].join("-");
};
