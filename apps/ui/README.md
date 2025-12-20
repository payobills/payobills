# @payobills/ui

<!-- Sample change -->
UI for payobills.

docker build -t payobills-ui:0.1.0-alpha.1 -f ../../common/docker/svelte-svc.prod.dockerfile --build-arg SVC=payobills.ui --build-arg NODE_VERSION=18.12.0 .

helm upgrade --create-namespace --install -n payobills --set service.type=NodePort --set image.tag=0.1.0-alpha.1 ui ./k8s

k port-forward -n payobills svc/ui 8080:80

## changelog

- 0.2.50: fix: amount decimal places when creating transactions manually

## Creating a project

If you're seeing this, you've probably already done this step. Congrats!

```bash
# create a new project in the current directory
npm create svelte@latest

# create a new project in my-app
npm create svelte@latest my-app
```

## Developing

Once you've created a project and installed dependencies with `npm install` (or `pnpm install` or `yarn`), start a development server:

```bash
npm run dev

# or start the server and open the app in a new browser tab
npm run dev -- --open
```

## Building

To create a production version of your app:

```bash
npm run build
```

You can preview the production build with `npm run preview`.

> To deploy your app, you may need to install an [adapter](https://kit.svelte.dev/docs/adapters) for your target environment.

## Local Setup with Docker

```bash
docker run -it -p 5173:80 \
    -e INJECTED_OWN_URL=http://localhost:5173 \
    -e INJECTED_OIDC_TENANT_URL=http://localhost:8084 \
    -e INJECTED_OIDC_CLIENT_ID=payobills \
    -e INJECTED_OIDC_TENANT_LOGIN_URL_TEMPLATE='${INJECTED_OIDC_TENANT_URL}/realms/Homelab-SBX/protocol/openid-connect/auth?client_id=${INJECTED_OIDC_CLIENT_ID}&redirect_uri=${INJECTED_OWN_URL}/callback&response_type=code' \
    ghcr.io/payobills/apps/ui:latest
```

