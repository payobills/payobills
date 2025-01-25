subinclude("@pleasings//docker")

app_name = split_path(package_name())[-1] 
svc='API'

docker_image(
    name="docker",
    image=package_name(),
    dockerfile="//common/docker:dotnet-svc.prod.dockerfile",
    srcs=glob(["src"], ["src/bin/**", "src/obj/**"]),
    build_args='--build-arg DOTNET_VERSION=${DOTNET_VERSION} --build-arg SVC={svc} --build-arg ARCH=${DOTNET_RUNTIME_ID} --build-arg DOTNET_RUNTIME_VERSION=${DOTNET_RUNTIME_VERSION}'.format(svc=svc)
)

genrule(
  name="docker_buildx",
  binary = True,
  pass_env=["VERSION", "DOTNET_VERSION", "DOTNET_RUNTIME_VERSION"],
  cmd='''
    echo '#!/bin/sh' > $OUT
    echo 'IMAGE_TAG_VERSION="${VERSION:-latest}"' >> $OUT
    echo 'PLATFORMS="${PLATFORMS:-linux/arm64,linux/amd64}"' >> $OUT
    echo 'EXTRA_ARGS="${EXTRA_ARGS}"' >> $OUT
    echo 'docker buildx build --platform "${PLATFORMS}" --build-arg DOTNET_VERSION=${DOTNET_VERSION} --build-arg SVC={svc} --build-arg DOTNET_RUNTIME_VERSION=${DOTNET_RUNTIME_VERSION} -t ghcr.io/payobills/apps/bills:${IMAGE_TAG_VERSION} -f common/docker/dotnet-svc.prod.dockerfile ${EXTRA_ARGS} apps/bills' >> $OUT
  '''.format(svc=svc),
  out="docker-buildx.sh"
)

# sh_cmd(
#     name="local",
#     deps=[":helm_tarball"],
#     cmd='''
#     tar \
#       xvf \
#       plz-out/gen/apps/{svc}/bills.tgz \
#       --directory plz-out/gen/apps/{svc}/helm_tarball
#     helm upgrade --install \
#       --create-namespace -n ${PROJECT_NAME}-dev \
#       --set helpers.PROJECT_ROOT=${PROJECT_ROOT} \
#       --values plz-out/gen/apps/{svc}/helm_tarball/values.local.yaml \
#       {svc} plz-out/gen/apps/{svc}/helm_tarball
#     rm -r plz-out/gen/apps/{svc}/helm_tarball
#     '''.format(svc='bills'),
#     srcs=[":helm_tarball"],
# )

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
  subdir=app_name,
  out='{app_name}.tgz'.format(app_name=app_name),
  srcs=["//common/k8s"] + glob("k8s/values.yaml") + [":chart_yaml"]
)

sh_cmd(
    name="local",
    deps=[":helm_tarball"],
    cmd='''
    tar \
      xvf \
      plz-out/gen/apps/{svc}/bills.tgz \
      --directory plz-out/gen/apps/{svc}/helm_tarball
    helm upgrade --install \
      --create-namespace -n ${PROJECT_NAME}-dev \
      --set helpers.PROJECT_ROOT=${PROJECT_ROOT} \
      --values plz-out/gen/apps/{svc}/helm_tarball/values.local.yaml \
      {svc} plz-out/gen/apps/{svc}/helm_tarball
    rm -r plz-out/gen/apps/{svc}/helm_tarball
    '''.format(svc='bills'),
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
    cmd='''
    tar \
      xvf \
      plz-out/gen/apps/{svc}/bills.tgz \
      --directory plz-out/gen/apps/{svc}
    helm upgrade \
      --install \
      --create-namespace -n payobills-dev \
      --values apps/{svc}/values.yaml \
      {svc} plz-out/gen/apps/{svc}/k8s/values.yaml
    rm plz-out/gen/apps/{svc}/bills.tgz
    '''.format(svc=split_path(package_name())[-1], project_name='API'),
    srcs=[":helm_tarball"],
)
