import { writable } from "svelte/store";

export const nav = writable<{ isOpen: boolean | null; title: string }>({
	isOpen: false,
	title: "payobills",
	link: "",
});
