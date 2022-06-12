ARG DOTNET_VERSION

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}

CMD tail -f /dev/null

