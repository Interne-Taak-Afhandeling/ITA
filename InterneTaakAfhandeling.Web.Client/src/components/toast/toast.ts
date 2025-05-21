import { reactive } from "vue";
import type { AlertType } from "@/components/UtrechtAlert.vue";

export interface ToastParams {
  text: string;
  type?: AlertType;
  timeout?: number;
  dismissible?: boolean;
}

interface Message {
  text: string;
  type: AlertType;
}

const _messages = reactive<Message[]>([]);

export const messages = _messages as readonly Message[];

export const toast = {
  add(params: ToastParams | string) {
    const m: Message =
      typeof params === "string"
        ? {
            text: params,
            type: "ok" as AlertType
          }
        : {
            text: params.text,
            type: params.type || "ok"
          };

    _messages.push(m);
    const defaultTimeout = m.type === "error" ? 10000 : 2000;
    const timeout = typeof params === "string" ? defaultTimeout : params.timeout || defaultTimeout;
    setTimeout(() => this.remove(m), timeout);
  },
  remove(m: Message) {
    const index = _messages.indexOf(m);
    if (index !== -1) _messages.splice(index, 1);
  }
};
