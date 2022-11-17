subinclude("@pleasings//docker")

# bare_package_name=

docker_image(
    name="docker",
    image=package_name(),
    dockerfile="//common/docker:dotnet-svc.prod.dockerfile",
    srcs=glob(["src"], ["src/bin", "src/obj"]),
    build_args='--build-arg DOTNET_VERSION=6.0 --build-arg SVC=payobills.bills'
)

helm_upgrade_cmd_str='ls apps/bills/; helm upgrade --install --create-namespace -n payobills-dev --values ${PROJ}/apps/bills/k8s/values.local.yaml bills apps/bills/k8s'.format(APP=package_name())

sh_cmd(
    name="local",
    cmd=helm_upgrade_cmd_str,
    srcs=["k8s"],
    # data, out, shell, labels, deps, visibility, expand_env_vars, test_only)
)

