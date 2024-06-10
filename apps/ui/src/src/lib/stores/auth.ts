import { get, writable, type Writable } from "svelte/store";
import { CONSTANTS } from "../../constants";

export const loadAuthFromLocalStorage = (): boolean => {

    if (get(auth) !== null) return false

    let refreshTokenExpiry = localStorage.getItem(CONSTANTS.REFRESH_TOKEN_EXPIRY_KEY)
    if (refreshTokenExpiry === null) return false
    if (refreshTokenExpiry !== null && new Date(+refreshTokenExpiry).valueOf() > Date.now()) {
        var refreshToken = localStorage.getItem(CONSTANTS.REFRESH_TOKEN_KEY)
        if (refreshToken === '') return false;

        auth.set({
            refreshToken: refreshToken as string,
            refreshTokenExpiry: +refreshTokenExpiry
        });
        return true
    }

    return false
}

export type AuthStore = {
    refreshToken: string
    refreshTokenExpiry: number
}

export const auth = writable<AuthStore | null>(null)
