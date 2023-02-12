subinclude("@pleasings//k8s")

for devenv_item in glob("*"):
  log.info(devenv_item)
  tarball(
    name="{devenv_item}_k8s".format(devenv_item=devenv_item),
    srcs=["//common/k8s"] + [
      "{devenv_item}/Chart.yaml".format(devenv_item=devenv_item),
      "{devenv_item}/values.local.yaml".format(devenv_item=devenv_item),
    ]
  )

  sh_cmd(
    name="{devenv_item}".format(devenv_item=devenv_item),
    deps=[":{devenv_item}_k8s".format(devenv_item=devenv_item)],
    cmd='''
    mkdir plz-out/gen/devenv/{devenv_item}_k8s
    tar xvf plz-out/gen/devenv/{devenv_item}_k8s.tar.gz --directory plz-out/gen/devenv/{devenv_item}_k8s
    helm upgrade --install \
      --create-namespace -n ${PROJECT_NAME}-dev \
      --set helpers.PROJECT_ROOT=${PROJECT_ROOT} \
      --values plz-out/gen/devenv/{devenv_item}_k8s/values.local.yaml \
      {devenv_item} plz-out/gen/devenv/{devenv_item}_k8s
    rm -r plz-out/gen/devenv/{devenv_item}_k8s
    '''.format(devenv_item=devenv_item),
    # srcs=["{devenv_item}_k8s".format(devenv_item=devenv_item)],
  )

sh_cmd(
    name="devenv",
    deps=[":{devenv_item}_k8s".format(devenv_item=devenv_item) for devenv_item in glob("*")],
    cmd='cd ${PROJECT_ROOT} && plz run {labels}'.format(labels=" ".join(["//devenv:{devenv_item}".format(devenv_item=devenv_item) for devenv_item in glob("*")])),
)
