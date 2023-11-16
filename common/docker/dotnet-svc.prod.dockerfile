ARG DOTNET_VERSION

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build-env
WORKDIR /app

# copy everything else and build
COPY . ./

RUN dotnet publish -c Release --self-contained -r linux-x64 -o out /p:PublishSingleFile=True

# build runtime image
FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION}
WORKDIR /app

COPY --from=build-env /app/out ./

EXPOSE 80

ARG SVC
ENV SVC=${SVC}
ENTRYPOINT ./${SVC}