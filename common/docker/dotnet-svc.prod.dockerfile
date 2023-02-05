ARG DOTNET_VERSION=6.0
ARG DOTNET_RUNTIME_VERSION=6.0-alpine

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build-env

WORKDIR /app

# copy everything else and build
COPY src ./

ARG ARCH=linux-x64
RUN dotnet publish -c Release --self-contained -r $ARCH -o out

# build runtime image

FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_RUNTIME_VERSION}

WORKDIR /app

COPY --from=build-env /app/out ./

EXPOSE 80

ARG SVC
ENV SVC=${SVC}
ENTRYPOINT ./${SVC}
