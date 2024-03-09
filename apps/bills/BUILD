subinclude("@pleasings//docker")

svc='API'

docker_image(
    name="docker",
    image=package_name(),
    dockerfile="//common/docker:dotnet-svc.prod.dockerfile",
    srcs=glob(["src"], ["src/bin/**", "src/obj/**"]),
    build_args='--build-arg DOTNET_VERSION=${DOTNET_VERSION} --build-arg SVC={svc} --build-arg ARCH=${DOTNET_RUNTIME_ID} --build-arg DOTNET_RUNTIME_VERSION=${DOTNET_RUNTIME_VERSION}'.format(svc=svc)
)

genrule(
  name="chart_yaml",
  srcs = ["k8s/Chart.yaml"],
  pass_env = ["VERSION"],
  cmd = [
    "cat $SRCS >> $OUT",
    "echo version: \"${VERSION}\" >> $OUT",
    "echo appVersion: \"${VERSION}\" >> $OUT"
  ],
  out = "Chart.yaml"
)

genrule(
  name="version",
  pass_env = ["VERSION"],
  cmd = [
    "echo \"${VERSION}\" >> $@",
  ],
  out = "VERSION"
)

tarball(
  name="helm_tarball",
  subdir="chart",
  out='{svc}.tgz'.format(svc=split_path(package_name())[-1]),
  srcs=["//common/k8s"] + glob("k8s/values.yaml") + [":chart_yaml"]
)

sh_cmd(
    name="local",
    deps=[":helm_tarball"],
    cmd='''
    mkdir plz-out/gen/apps/{svc}/helm_tarball
    tar xvf plz-out/gen/apps/{svc}/helm_tarball.tar.gz --directory plz-out/gen/apps/{svc}/helm_tarball
    helm upgrade --install \
      --create-namespace -n ${PROJECT_NAME}-dev \
      --set helpers.PROJECT_ROOT=${PROJECT_ROOT} \
      --values plz-out/gen/apps/{svc}/helm_tarball/values.local.yaml \
      {svc} plz-out/gen/apps/{svc}/helm_tarball
    rm -r plz-out/gen/apps/{svc}/helm_tarball
    '''.format(svc=svc),
    srcs=[":helm_tarball"],
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
    deps=[":helm_tarball"],
    cmd='mkdir plz-out/gen/apps/{svc}/helm_tarball; tar xvf plz-out/gen/apps/{svc}/helm_tarball.tar.gz --directory plz-out/gen/apps/{svc}/helm_tarball; helm upgrade --install --create-namespace -n ${PROJECT_NAME} --set helpers.PROJECT_ROOT=${PROJECT_ROOT} --values plz-out/gen/apps/{svc}/helm_tarball/values.yaml {svc} plz-out/gen/apps/{svc}/helm_tarball; rm -r plz-out/gen/apps/{svc}/helm_tarball'.format(svc=svc),
    srcs=[":helm_tarball"],
)
