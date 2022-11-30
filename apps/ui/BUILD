files(
  name = "src",
  sources=[
    "src/**/*"
  ]
)

docker_image(
    name="ui",
    dependencies=[
      ":src"
    ],
    extra_build_args=[
      'SVC=payobills.ui',
      'WORKSPACE=apps/ui/src'
    ]
)
