docker build -t payobills-bills:0.1.0-alpha.2 -f ../../../common/docker/dotnet-svc.prod.dockerfile --build-arg SVC=payobills.bills --build-arg DOTNET_VERSION=6.0 .

helm upgrade --install -n payobills \
    --set image.tag=0.1.0-alpha.2 \
    bills ../k8s

k port-forward -n payobills svc/bills-k8s 8080:80