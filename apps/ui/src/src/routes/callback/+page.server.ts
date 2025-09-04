import { env } from '$env/dynamic/private';

/** @type {import('./$types').PageLoad} */
export function load() {
	return {
		data: {
			oidcClientId: env.OIDC_CLIENT_ID
		},
		urls: {
			oidcUrl: env.OIDC_TENANT_URL,
			ownUrl: env.OWN_URL
		}
	};
}