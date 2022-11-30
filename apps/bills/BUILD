files(
  name = "src",
  sources=[
    "src/**/*"
  ]
)

docker_image(
    name="bills",
    dependencies=[
      ":src"
    ],
    extra_build_args=[
      'SVC=payobills.bills',
      'WORKSPACE=apps/bills/src'
    ]
)
