ARG DOTNET_VERSION

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}

WORKDIR /app

CMD dotnet watch run --no-launch-profile
