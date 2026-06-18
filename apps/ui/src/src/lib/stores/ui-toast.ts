import { writable } from "svelte/store";

export interface Toast {
	id: number;
	message: string;
	variant: "success" | "info" | "error";
}

let nextId = 0;

function createToastStore() {
	const { subscribe, update } = writable<Toast[]>([]);

	return {
		subscribe,
		show(message: string, variant: Toast["variant"] = "success") {
			const id = nextId++;
			update((toasts) => [...toasts, { id, message, variant }]);
			return id;
		},
		dismiss(id: number) {
			update((toasts) => toasts.filter((t) => t.id !== id));
		},
	};
}

export const toasts = createToastStore();
