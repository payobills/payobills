subinclude("@pleasings//docker")

svc=split_path(package_name())[-1]

docker_image(
    name="docker",
    image=package_name(),
    dockerfile="//common/docker:dotnet-svc.prod.dockerfile",
    srcs=glob(["src"], ["src/bin/**", "src/obj/**"]),
    build_args='--build-arg DOTNET_VERSION=6.0 --build-arg SVC=${PROJECT_NAME}.{svc} --build-arg ARCH=${DOTNET_RUNTIME_ID} --build-arg DOTNET_RUNTIME_VERSION=${DOTNET_RUNTIME_VERSION}'.format(svc=svc)
)

tarball(
  name="package",
  srcs=["//common/k8s"] + glob("k8s/**")
)

tarball(
  name="package_prod",
  srcs=["//common/k8s"] + ["k8s/Chart.yaml"],
  subdir="bills",
  gzip=True
)

sh_cmd(
    name="local",
    deps=[":package"],
    cmd='''
    mkdir plz-out/gen/apps/{svc}/package
    tar xvf plz-out/gen/apps/{svc}/package.tar.gz --directory plz-out/gen/apps/{svc}/package
    helm upgrade --install \
      --create-namespace -n ${PROJECT_NAME}-dev \
      --set helpers.PROJECT_ROOT=${PROJECT_ROOT} \
      --values plz-out/gen/apps/{svc}/package/values.local.yaml \
      {svc} plz-out/gen/apps/{svc}/package
    rm -r plz-out/gen/apps/{svc}/package
    '''.format(svc=svc),
    srcs=[":package"],
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
    deps=[":package"],
    cmd='mkdir plz-out/gen/apps/{svc}/package; tar xvf plz-out/gen/apps/{svc}/package.tar.gz --directory plz-out/gen/apps/{svc}/package; helm upgrade --install --create-namespace -n ${PROJECT_NAME} --set helpers.PROJECT_ROOT=${PROJECT_ROOT} --values plz-out/gen/apps/{svc}/package/values.yaml {svc} plz-out/gen/apps/{svc}/package; rm -r plz-out/gen/apps/{svc}/package'.format(svc=svc),
    srcs=[":package"],
)
