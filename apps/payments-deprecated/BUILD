subinclude("@pleasings//docker")

svc=split_path(package_name())[-1]

docker_image(
    name="docker",
    image=package_name(),
    dockerfile="//common/docker:node-svc.prod.dockerfile",
    srcs=glob(["src"], ["src/bin/**", "src/obj/**"]),
    build_args='--build-arg NODE_VERSION=18.11.0'.format(svc=svc)
)

tarball(
  name="local_k8s",
  srcs=["//common/k8s"] + ["k8s/Chart.yaml", "k8s/values.local.yaml" ]
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
    echo 'docker buildx build --platform "${PLATFORMS}" --build-arg NODE_VERSION="${NODE_VERSION}" -t ghcr.io/payobills/apps/payments:${IMAGE_TAG_VERSION} -f common/docker/node-svc.prod.dockerfile ${EXTRA_ARGS} apps/payments' >> $OUT
  '''.format(svc=svc),
  out="docker-buildx.sh"
)

sh_cmd(
    name="local",
    deps=[":local_k8s"],
    cmd='''
    mkdir plz-out/gen/apps/{svc}/local_k8s
    tar xvf plz-out/gen/apps/{svc}/local_k8s.tar.gz --directory plz-out/gen/apps/{svc}/local_k8s
    helm upgrade --install \
      --wait \
      --create-namespace -n ${PROJECT_NAME}-dev \
      --set helpers.PROJECT_ROOT=${PROJECT_ROOT} \
      --values plz-out/gen/apps/{svc}/local_k8s/values.local.yaml \
      {svc} plz-out/gen/apps/{svc}/local_k8s
    rm -r plz-out/gen/apps/{svc}/local_k8s
    '''.format(svc=svc),
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

