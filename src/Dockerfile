#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_VERSION=1.2.3
ARG GITHUB_RUN_NUMBER=4
WORKDIR /src
RUN ls
COPY ["IoTHub.Portal.Server/IoTHub.Portal.Server.csproj", "IoTHub.Portal.Server/"]
COPY ["IoTHub.Portal.Shared/IoTHub.Portal.Shared.csproj", "IoTHub.Portal.Shared/"]
COPY ["IoTHub.Portal.Client/IoTHub.Portal.Client.csproj", "IoTHub.Portal.Client/"]
RUN dotnet restore "IoTHub.Portal.Server/IoTHub.Portal.Server.csproj"
COPY . .
WORKDIR "/src/IoTHub.Portal.Server"
RUN dotnet build "IoTHub.Portal.Server.csproj" -c Release -o /app/build -p:Version="${BUILD_VERSION}.${GITHUB_RUN_NUMBER}"

FROM build AS publish
ARG BUILD_VERSION=1.2.3
ARG GITHUB_RUN_NUMBER=4
RUN dotnet publish "IoTHub.Portal.Server.csproj" -c Release -o /app/publish  -p:Version="${BUILD_VERSION}.${GITHUB_RUN_NUMBER}"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IoTHub.Portal.Server.dll"]
