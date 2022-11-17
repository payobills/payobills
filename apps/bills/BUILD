subinclude("@pleasings//docker")

docker_image(
    name="docker",
    image=package_name(),
    dockerfile="//common/docker:dotnet-svc.prod.dockerfile",
    srcs=glob(["src"], ["src/bin", "src/obj"]),
    build_args='--build-arg DOTNET_VERSION=6.0 --build-arg SVC=payobills.bills'
)
