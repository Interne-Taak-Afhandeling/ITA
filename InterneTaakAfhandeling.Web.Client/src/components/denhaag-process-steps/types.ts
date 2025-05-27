export type StepStatus = "checked" | "not-checked" | "current" | "warning" | "error" | "default";

export interface StepProps {
  id: string;
  title: string;
  steps?: Omit<StepProps, "id">[];
  status?: StepStatus;
  collapsible?: boolean;
}
