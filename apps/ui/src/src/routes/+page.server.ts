import { redirect } from "@sveltejs/kit";

export function load() {
    throw redirect(
        301,
        "http://localhost:3001/auth/realms/homelab/protocol/openid-connect/auth?client_id=payobills&redirect_uri=http://localhost:3000/callback&response_type=code&grant_type=authorization_code&scope=openid"
    );
}
