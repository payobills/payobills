import { env } from '$env/dynamic/private';
import { json } from '@sveltejs/kit';

export async function GET() {
    var loginUrl = env.OIDC_TENANT_LOGIN_URL_TEMPLATE
        .replace('${OWN_URL}', env.OWN_URL)
        .replace('${OIDC_CLIENT_ID}', env.OIDC_CLIENT_ID)
        .replace('${OIDC_TENANT_URL}', env.OIDC_TENANT_URL)

    const envData = {
        data: {
            urls: {
                oidcUrl: env.OIDC_TENANT_URL,
                loginUrl,
                filesBaseUrl: env.FILES_BASE_URL,
            }
        }
    };

    return json(envData);
}