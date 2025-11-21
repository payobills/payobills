import { writable } from "svelte/store";

export const nav = writable<{ isOpen: boolean | null }>({
    isOpen: false,
});

