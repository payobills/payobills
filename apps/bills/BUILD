files(
  name = "src",
  sources=[
    "src/**/*"
  ]
)

docker_image(
    name="docker",
    repository="ghcr.io/payobills/bills",
    dependencies=[
      ":src"
    ],
    extra_build_args=[
      'SVC=payobills.bills',
      'WORKSPACE=apps/bills/src'
    ]
)

relocated_files(
  name='helm_relocated_files',
  files_targets=['common/k8s'],
  src="common/k8s",
  dest="apps/bills"
)

helm_chart(
  name = 'helm',
  chart = "Chart.yaml",
  dependencies = [":helm_relocated_files"],
  sources = ['values.yaml'],
  output_path = "apps/bills/"
)

