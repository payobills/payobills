export const prerender = import.meta.env.PUBLIC_APP_MODE === 'APP'

import { env } from '$env/dynamic/private';

/** @type {import('./$types').PageLoad} */
export const load = import.meta.env.PUBLIC_APP_MODE !== 'APP' ? (() => {

	var loginUrl = env.OIDC_TENANT_LOGIN_URL_TEMPLATE
		.replace('${OWN_URL}', env.OWN_URL)
		.replace('${OIDC_CLIENT_ID}', env.OIDC_CLIENT_ID)
		.replace('${OIDC_TENANT_URL}', env.OIDC_TENANT_URL)

	return {
		urls: {
			oidcUrl: env.OIDC_TENANT_URL,
			loginUrl,
		}
	};
}) : () => {
	return {
		rootRoute: import.meta.env.PUBLIC_APP_MODE === 'APP' ? '/app' : '/timeline'
	}
};
