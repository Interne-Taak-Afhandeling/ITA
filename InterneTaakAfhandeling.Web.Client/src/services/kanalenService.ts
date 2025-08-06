import { get, post } from "@/utils/fetchWrapper";

export interface Kanaal {
  id: number;
  naam: string;
}

export const kanalenService = {
  getKanalen: (): Promise<Kanaal[]> => {
    return get<Kanaal[]>("/api/kanalen");
  },
  createKanaal: (naam: string): Promise<Kanaal> => {
    return post<Kanaal>("/api/kanalen", { naam });
  }
};
