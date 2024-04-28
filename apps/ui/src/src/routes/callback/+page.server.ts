import { OIDC_TENANT_URL, OWN_URL } from '$env/static/private';

/** @type {import('./$types').PageLoad} */
export function load() {
	return {
		urls: {
			oidcUrl: OIDC_TENANT_URL,
			ownUrl: OWN_URL
		}
	};
}