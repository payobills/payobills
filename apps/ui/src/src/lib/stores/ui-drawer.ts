import { writable } from "svelte/store";

export const uiDrawer = writable<{ content: any | null, onClose: () => any }>({

    content: null,
    onClose: () => { }
});
