subinclude("@pleasings//docker")

svc=split_path(package_name())[-1]

docker_image(
    name="docker",
    image=package_name(),
    dockerfile="//common/docker:dotnet-svc.prod.dockerfile",
    srcs=glob(["src"], ["src/bin/**", "src/obj/**"]),
    build_args='--build-arg DOTNET_VERSION=6.0 --build-arg SVC=${PROJ}.{svc}'.format(svc=svc)
)

tarball(
  name="local_k8s",
  srcs=["//common/k8s"] + glob("k8s/**")
)

sh_cmd(
    name="local",
    deps=[":local_k8s"],
    cmd='mkdir plz-out/gen/apps/{svc}/local_k8s; tar xvf plz-out/gen/apps/{svc}/local_k8s.tar.gz --directory plz-out/gen/apps/{svc}/local_k8s; helm upgrade --install --create-namespace -n ${PROJECT_NAME}-dev --set helpers.PROJECT_ROOT=${PROJECT_ROOT} --values plz-out/gen/apps/{svc}/local_k8s/values.local.yaml {svc} plz-out/gen/apps/{svc}/local_k8s; rm -r plz-out/gen/apps/{svc}/local_k8s'.format(svc=svc),
    srcs=[":local_k8s"],
)
