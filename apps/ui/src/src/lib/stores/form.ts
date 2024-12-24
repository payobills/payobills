import { writable, type Writable } from "svelte/store";

export type FormStore<T> = {
    data: T | null,
    isDirty: boolean
}

const cache : Map<string, any> = new Map<string, any>();
export const formStoreGenerator = <T>(key: string): Writable<FormStore<T>> => {
    const data: Writable<FormStore<T>> = cache.get(key);

    if (!data) {
        const newStore = writable<FormStore<T>>()
        cache.set(key, newStore);
        return newStore;
    }

    return data as Writable<FormStore<T>>;
}
