# payobills
A self-hosted (Bring and Keep your own Data) bill management app - very much WIP

some screen shots...

Home Timeline view | Adding a bill manually
--- | ---
![Screenshot of the current version of the App](img-assets/timeline-view.png) | ![Screenshot of adding a bill manually page](img-assets/add-bill-view.png)

## why create such an app?
- Help me with all my bill related needs - save time and pay bills together.
- Reminders about which bills I need to pay.
- Analytics from all my financial data, locally.
- Help me with all my bill related needs.
- Learning Kubernetes
- Contributing to the Self-Hosted open source community


## project setup (headed towards this setup)
- Each microservice in a folder in `apps` directory
- A build system to help managing deployment and running the services
- Code reuse with packages 

---


-- wip --

CI=true docker buildx build --no-cache --builder=inspiring_gagarin --build-arg NODE_VERSION=18.14.2  -t ghcr.io/payobills/apps/ui:v0.2.5-alpha.0 -f ../../../common/docker/node-svc.prod.dockerfile --platform linux/arm64 --load ..

export DOTNET_VERSION=7.0 svc=API DOTNET_RUNTIME_ID=linux-x64 DOTNET_RUNTIME_VERSION=7.0.17-bookworm-slim && docker buildx build --builder=inspiring_gagarin --build-arg DOTNET_VERSION=${DOTNET_VERSION} --build-arg SVC=${svc} --build-arg ARCH=${DOTNET_RUNTIME_ID} --build-arg DOTNET_RUNTIME_VERSION=${DOTNET_RUNTIME_VERSION} -t ghcr.io/payobills/apps/bills:v0.2.5-alpha.1 -f ../../../common/docker/dotnet-svc.prod.dockerfile --platform linux/arm64,linux/amd64  --push ..

export svc=bills; export PROJECT_NAME=payobills; export KUBECONFIG=~/.homelab-pi/config/pi.dory-char.ts.net.kubeconfig; helm upgrade --install  --create-namespace -n "${PROJECT_NAME}" --set helpers.PROJECT_ROOT='/app'      --values "apps/bills/k8s/values.yaml" "${svc}" "plz-out/gen/apps/${svc}/chart"