# https://devblogs.microsoft.com/dotnet/improving-multiplatform-container-support/

ARG DOTNET_VERSION
ARG DOTNET_RUNTIME_VERSION

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build-env

WORKDIR /app

# copy everything else and build
COPY src ./

ARG TARGETARCH
# RUN dotnet restore
RUN dotnet publish -c Release --self-contained -a $TARGETARCH -o out API/API.csproj

# build runtime image

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime:${DOTNET_RUNTIME_VERSION}

WORKDIR /app

COPY --from=build-env /app/out ./

EXPOSE 80

ARG SVC
ENV SVC=${SVC}

ENV DOTNET_URLS='http://0.0.0.0:80'

ENTRYPOINT ./${SVC}
