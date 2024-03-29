subinclude("@pleasings//docker")

svc=split_path(package_name())[-1]

docker_image(
    name="docker",
    image=package_name(),
    dockerfile="//common/docker:svelte-svc.prod.dockerfile",
    srcs=glob(["src"], ["src/node_modules/**", "src/.svelte-kit/**", "src/tests/**"]),
    build_args="--no-cache --build-arg NODE_VERSION=18.14.2"
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

sh_cmd(
    name = "image_scan",
    deps = [":docker"],
    srcs = [":docker_fqn"],
    cmd = '''
    docker run --rm \
      --volume /var/run/docker.sock:/var/run/docker.sock \
      anchore/grype:v0.57.1 \
      `cat $SRCS`
    '''
)

sh_cmd(
    name="prod",
    deps=[":local_k8s"],
    cmd='mkdir plz-out/gen/apps/{svc}/local_k8s; tar xvf plz-out/gen/apps/{svc}/local_k8s.tar.gz --directory plz-out/gen/apps/{svc}/local_k8s; helm upgrade --install --create-namespace -n ${PROJECT_NAME} --values plz-out/gen/apps/{svc}/local_k8s/values.yaml {svc} plz-out/gen/apps/{svc}/local_k8s; rm -r plz-out/gen/apps/{svc}/local_k8s'.format(svc=svc),
    srcs=[":local_k8s"],
)
