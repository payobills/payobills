import { get, writable } from "svelte/store";

export const tryLoadEnv = async (): Promise<boolean> => {
	if (get(envStore) !== null) return false;

	try {
		const envResponse = await fetch("/env");
		const response = await envResponse.json();

		envStore.set(response);
		// console.log('Loaded environment vars');

		return true;
	} catch (error) {
		console.error("Failed to load environment vars:", error);
	}

	return false;
};

export type EnvStore = {
	INJECTED_OIDC_TENANT_LOGIN_URL_TEMPLATE: string;
	INJECTED_OWN_URL: string;
	INJECTED_OIDC_TENANT_URL: string;
	INJECTED_OIDC_CLIENT_ID: string;
};

export const envStore = writable<EnvStore | null>(null);
