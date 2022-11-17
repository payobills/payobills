subinclude("@pleasings//docker")

docker_image(
    name="docker",
    image=package_name(),
    dockerfile="//common/docker:svelte-svc.prod.dockerfile",
    srcs=glob(["src"], ["src/node_modules/**", "src/.svelte-kit/**", "src/tests/**"]),
    build_args="--no-cache --build-arg NODE_VERSION=18.11.0"
)