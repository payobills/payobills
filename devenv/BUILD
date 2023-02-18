subinclude("@pleasings//k8s")

devenv_items = [
  "sonarqube",
  "sonarqube-expose",
]

local_dev_items = [
  "sonarqube-expose",
]

# externals

sh_cmd(
  name="sonarqube",
  cmd='''
  helm upgrade --create-namespace --install -n ${PROJECT_NAME}-dev --repo https://SonarSource.github.io/helm-chart-sonarqube sonarqube sonarqube
  ''',
)

# ---

sh_cmd(
  name="sonarqube_cleanup",
  cmd='helm uninstall -n ${PROJECT_NAME}-dev sonarqube',
)

for devenv_item in local_dev_items:
  tarball(
    name="{devenv_item}_k8s".format(devenv_item=devenv_item),
    srcs=["//common/k8s"] + [
      "{devenv_item}/Chart.yaml".format(devenv_item=devenv_item),
      "{devenv_item}/values.local.yaml".format(devenv_item=devenv_item),
    ]
  )

  sh_cmd(
    name="{devenv_item}_cleanup".format(devenv_item=devenv_item),
    deps=[":{devenv_item}_k8s".format(devenv_item=devenv_item)],
    cmd='''
    helm uninstall -n ${PROJECT_NAME}-dev {devenv_item}
    '''.format(devenv_item=devenv_item),
  )

  sh_cmd(
    name="{devenv_item}".format(devenv_item=devenv_item),
    deps=[":{devenv_item}_k8s".format(devenv_item=devenv_item)],
    cmd='''
    echo Running {devenv_item}
    rm -r plz-out/gen/devenv/{devenv_item}_k8s 2> /dev/null
    mkdir plz-out/gen/devenv/{devenv_item}_k8s
    tar xvf plz-out/gen/devenv/{devenv_item}_k8s.tar.gz --directory plz-out/gen/devenv/{devenv_item}_k8s
    helm upgrade --install \
      --create-namespace -n ${PROJECT_NAME}-dev \
      --set helpers.PROJECT_ROOT=${PROJECT_ROOT} \
      --values plz-out/gen/devenv/{devenv_item}_k8s/values.local.yaml \
      {devenv_item} plz-out/gen/devenv/{devenv_item}_k8s
    '''.format(devenv_item=devenv_item),
  )

sh_cmd(
    name="devenv",
    cmd='cd ${PROJECT_ROOT} && plz run parallel {labels}'.format(labels=" ".join(["//devenv:{devenv_item}".format(devenv_item=devenv_item) for devenv_item in devenv_items])),
)

sh_cmd(
    name="cleanup",
    cmd='cd ${PROJECT_ROOT} && plz run parallel {labels}'.format(labels=" ".join(["//devenv:{devenv_item}_cleanup".format(devenv_item=devenv_item) for devenv_item in devenv_items])),
)
