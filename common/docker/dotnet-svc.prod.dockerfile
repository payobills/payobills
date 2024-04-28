# https://devblogs.microsoft.com/dotnet/improving-multiplatform-container-support/

ARG DOTNET_VERSION=6.0
ARG DOTNET_RUNTIME_VERSION=6.0.14-bullseye-slim-arm64v8

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build-env

WORKDIR /app

# copy everything else and build
COPY src ./

ARG TARGETARCH
# RUN dotnet restore
RUN dotnet publish -c Release --self-contained -a $TARGETARCH -o out

# build runtime image

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime:${DOTNET_RUNTIME_VERSION}

WORKDIR /app

COPY --from=build-env /app/out ./

EXPOSE 80

ARG SVC
ENV SVC=${SVC}
ENTRYPOINT ./${SVC}
