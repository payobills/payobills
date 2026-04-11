import { writable } from "svelte/store";

export const uiDrawer = writable<{
	content: any | null;
	isFullScreen: boolean;
	onClose: () => any;
}>({
	content: null,
	isFullScreen: false,
	onClose: () => {},
});
