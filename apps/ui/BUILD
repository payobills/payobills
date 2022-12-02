files(
  name = "src",
  sources=[
    "src/**/*"
  ]
)

docker_image(
    name="docker",
    repository="ghcr.io/payobills/ui",
    dependencies=[
      ":src"
    ],
    extra_build_args=[
      'SVC=payobills.ui',
      'WORKSPACE=apps/ui/src'
    ]
)

relocated_files(
  name='helm_relocated_files',
  files_targets=['common/k8s'],
  src="common/k8s",
  dest="apps/ui"
)

helm_chart(
  name = 'helm',
  chart = "Chart.yaml",
  dependencies = [":helm_relocated_files"],
  sources = ['values.yaml'],
  output_path = "apps/ui/"
)

