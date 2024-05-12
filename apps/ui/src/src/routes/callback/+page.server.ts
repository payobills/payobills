import { env } from '$env/dynamic/private';

/** @type {import('./$types').PageLoad} */
export function load() {
	return {
		urls: {
			oidcUrl: env.OIDC_TENANT_URL,
			ownUrl: env.OWN_URL
		}
	};
}