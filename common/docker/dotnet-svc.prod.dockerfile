# https://devblogs.microsoft.com/dotnet/improving-multiplatform-container-support/

ARG IMAGE__BUILD='mcr.microsoft.com/dotnet/sdk:8.0.406-noble'
ARG IMAGE__RUNTIME='mcr.microsoft.com/dotnet/runtime:8.0.13-bookworm-slim'
ARG TARGETARCH

FROM --platform=$BUILDPLATFORM ${IMAGE__BUILD} AS build-env

WORKDIR /app

# copy everything else and build
COPY src ./

ARG TARGETARCH
# RUN dotnet restore
RUN dotnet publish -c Release --self-contained -a $TARGETARCH -o out API/API.csproj

# build runtime image

ARG TARGETARCH
ARG IMAGE__RUNTIME

FROM --platform=$BUILDPLATFORM ${IMAGE__RUNTIME}

WORKDIR /app

COPY --from=build-env /app/out ./

EXPOSE 80

ARG SVC
ENV SVC=${SVC}
ENV TARGETARCH=${TARGETARCH}

ENV DOTNET_URLS='http://0.0.0.0:80'

ENTRYPOINT ${SVC}
