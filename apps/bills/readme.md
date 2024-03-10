# bills service

docker build -t payobills-bills:0.1.0-alpha.2 -f ../../../common/docker/dotnet-svc.prod.dockerfile --build-arg SVC=payobills.bills --build-arg DOTNET_VERSION=6.0 .

helm upgrade --install -n payobills \
    --set image.tag=0.1.0-alpha.2 \
    bills ../k8s

k port-forward -n payobills svc/bills-k8s 8080:80

docker build --build-arg DOTNET_VERSION=6.0 -t dotnet-dev:latest -f common/docker/dotnet.dev.dockerfile .

helm upgrade --create-namespace --install -n payobills-dev \
    --set image.tag=latest \
    --set image.repository=dotnet-dev \
    --set helpers.PROJECT_ROOT=`pwd` --set service.type=NodePort \
    --values apps/bills/k8s/values.local.yaml \
    bills apps/bills/k8s
