subinclude("@pleasings//docker")

svc=split_path(package_name())[-1]

docker_image(
    name="docker",
    image=package_name(),
    dockerfile="//common/docker:svelte-svc.prod.dockerfile",
    srcs=glob(["src"], ["src/node_modules/**", "src/.svelte-kit/**", "src/tests/**"]),
    build_args="--no-cache --build-arg NODE_VERSION=18.11.0"
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
