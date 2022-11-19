subinclude("@pleasings//docker")

svc=split_path(package_name())[-1]

docker_image(
    name="docker",
    image=package_name(),
    dockerfile="//common/docker:dotnet-svc.prod.dockerfile",
    srcs=glob(["src"], ["src/bin/**", "src/obj/**"]),
    build_args='--build-arg DOTNET_VERSION=6.0 --build-arg SVC=${PROJ}.{svc}'.format(svc=svc)
)

filegroup(
  name="local_k8s",
  srcs=["//common/k8s"] + glob("k8s/**")
)

helm_upgrade_cmd_str='cd plz-out/gen/apps/{svc}/local_k8s; helm upgrade --create-namespace -n ${PROJ}-dev --set helpers.PROJECT_ROOT=${PROJECT_ROOT} --set image.repository=mcr.microsoft.com/dotnet/sdk --set image.tag=6.0 --set image.pullPolicy=IfNotPresent --values values.local.yaml {svc} .'.format(svc=svc)
sh_cmd(
    name="local",
    deps=[":local_k8s"],
    cmd=helm_upgrade_cmd_str,
    srcs=[":local_k8s"],
)

