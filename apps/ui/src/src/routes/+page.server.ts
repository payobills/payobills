import { OIDC_TENANT_LOGIN_URL_TEMPLATE, OIDC_TENANT_URL, OWN_URL } from '$env/static/private';

/** @type {import('./$types').PageLoad} */
export function load() {

	var loginUrl = OIDC_TENANT_LOGIN_URL_TEMPLATE
		.replace('${OWN_URL}', OWN_URL)
		.replace('${OIDC_TENANT_URL}', OIDC_TENANT_URL)

	return {
		urls: {
			oidcUrl: OIDC_TENANT_URL,
			loginUrl
		}
	};
}