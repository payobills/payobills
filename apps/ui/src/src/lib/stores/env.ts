import { get, writable, type Writable } from "svelte/store";
import { CONSTANTS } from "../../constants";



export const tryLoadEnvUrls = async (): Promise<boolean> => {

    if (get(envStore) !== null) return false

    try {
        const envResponse = await fetch('/env')
        const response = await envResponse.json();
        const { oidcUrl, loginUrl, filesBaseUrl } = response.data.urls;

        envStore.set({ oidcUrl, loginUrl, filesBaseUrl });
        return true
    } catch (error) {
        console.error('Failed to load environment URLs:', error);
    }

    return false
}

export type EnvStore = {
    oidcUrl: string
    loginUrl: string
    filesBaseUrl: string
}

export const envStore = writable<EnvStore | null>(null)
