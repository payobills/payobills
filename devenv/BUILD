subinclude("@pleasings//k8s")

devenv_items = [
  "sonarqube",
  "sonar-scanner-cli",
  "sonarqube-expose",
  "nocodb"
]

local_dev_items = [
  "sonarqube-expose",
  "sonar-scanner-cli",
  "nocodb"
]

# externals

sh_cmd(
  name="sonarqube",
  cmd='''
  helm upgrade --create-namespace --install -n ${PROJECT_NAME}-dev --repo https://SonarSource.github.io/helm-chart-sonarqube sonarqube sonarqube
  ''',
)

sh_cmd(
  name="sonarqube_cleanup",
  cmd='helm uninstall -n ${PROJECT_NAME}-dev sonarqube',
)

for i in local_dev_items:
  tarball(
    name="{i}_k8s".format(i=i),
    srcs=["//common/k8s"] + [
      "{i}/Chart.yaml".format(i=i),
      "{i}/values.local.yaml".format(i=i),
    ]
  )

  sh_cmd(
    name="{i}_cleanup".format(i=i),
    deps=[":{i}_k8s".format(i=i)],
    cmd='''
    helm uninstall -n ${PROJECT_NAME}-dev {i}
    '''.format(i=i),
  )

  sh_cmd(
    name="{i}".format(i=i),
    deps=[":{i}_k8s".format(i=i)],
    cmd='''
    echo Running {i}
    rm -r plz-out/gen/devenv/{i}_k8s 2> /dev/null
    mkdir plz-out/gen/devenv/{i}_k8s
    tar xvf plz-out/gen/devenv/{i}_k8s.tar.gz --directory plz-out/gen/devenv/{i}_k8s
    helm upgrade --dry-run --debug --install \
      --create-namespace -n ${PROJECT_NAME}-dev \
      --set helpers.PROJECT_ROOT=${PROJECT_ROOT} \
      --values plz-out/gen/devenv/{i}_k8s/values.local.yaml \
      {i} plz-out/gen/devenv/{i}_k8s
    '''.format(i=i),
  )

sh_cmd(
    name="devenv",
    cmd='cd ${PROJECT_ROOT} && plz run parallel {labels}'.format(labels=" ".join(["//devenv:{devenv_item}".format(devenv_item=devenv_item) for devenv_item in devenv_items])),
)

sh_cmd(
    name="cleanup",
    cmd='cd ${PROJECT_ROOT} && plz run parallel {labels}'.format(labels=" ".join(["//devenv:{devenv_item}_cleanup".format(devenv_item=devenv_item) for devenv_item in devenv_items])),
)
