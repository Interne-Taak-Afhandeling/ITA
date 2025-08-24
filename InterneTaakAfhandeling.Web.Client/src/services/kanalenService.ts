import { get, post, put, del } from "@/utils/fetchWrapper";

export interface Kanaal {
  id: string;
  naam: string;
}

export const kanalenService = {
  getKanalen: (): Promise<Kanaal[]> => {
    return get<Kanaal[]>("/api/kanalen");
  },
  getKanaalById: (id: string): Promise<Kanaal> => {
    return get<Kanaal>(`/api/kanaal/${id}`);
  },
  createKanaal: (naam: string): Promise<Kanaal> => {
    return post<Kanaal>("/api/kanaal", { naam });
  },
  editKanaal: (id: string, naam: string): Promise<Kanaal> => {
    return put<Kanaal>(`/api/kanaal/${id}`, { naam });
  },
  deleteKanaal: (id: string): Promise<void> => {
    return del<void>(`/api/kanaal/${id}`);
  }
};
