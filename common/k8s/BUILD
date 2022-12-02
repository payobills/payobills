files(
  name="k8s",
  sources=["templates/**"]
)

archive(
  format='tar',
  name='tar',
  files=[':k8s']
)

relocated_files(
  name="k8s_relocated",
  files_targets=[":k8s"],
  src="common/k8s/templates",
  dest="templates"
)
