export type DateLike = string | number | Date | null | undefined;

export const parseValidDate = (date: DateLike) => {
  if (!date) return undefined;
  date = new Date(date);
  const time = date.getTime();
  if (date instanceof Date && !isNaN(time)) return date;
  return undefined;
};

export const formatNlDateTime = (date: string | number | Date | null | undefined) => {
  date = parseValidDate(date);
  if (!date) return undefined;
  const dateStr = date.toLocaleString("nl-NL", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric"
  });
  const time = date.toLocaleString("nl-NL", {
    hour: "2-digit",
    minute: "2-digit"
  });
  return `${dateStr} ${time}`;
};

export const formatIsoDateTime = (date: DateLike) => {
  return parseValidDate(date)?.toISOString();
};
