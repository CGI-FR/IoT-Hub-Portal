#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG BUILD_VERSION=1.0.0-dev

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80;https://+:443

EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["IoTHub.Portal.Server/IoTHub.Portal.Server.csproj", "IoTHub.Portal.Server/"]

RUN dotnet restore "IoTHub.Portal.Server/IoTHub.Portal.Server.csproj"

COPY . .

WORKDIR "/src/IoTHub.Portal.Server"
RUN dotnet build "IoTHub.Portal.Server.csproj" -c Release -o /app/build
RUN dotnet dev-certs https --trust

FROM build AS publish
RUN dotnet publish "IoTHub.Portal.Server.csproj" -c Release -o /app/publish

FROM base AS final

HEALTHCHECK CMD curl --fail http://localhost/healthz || exit

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "IoTHub.Portal.Server.dll"]
