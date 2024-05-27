#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG BUILD_VERSION=1.0.0-dev

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80;https://+:443

EXPOSE 80
EXPOSE 443

FROM node:14-bullseye AS build
RUN wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN  apt-get update; \
	  apt-get install -y apt-transport-https && \
	  apt-get update && \
	  apt-get install -y dotnet-sdk-8.0

FROM build AS builder
WORKDIR /src
COPY ["IoTHub.Portal/Server/IoTHub.Portal.Server.csproj", "IoTHub.Portal/Server/"]
COPY ["IoTHub.Portal/Shared/IoTHub.Portal.Shared.csproj", "IoTHub.Portal/Shared/"]
COPY ["IoTHub.Portal/Client/IoTHub.Portal.Client.csproj", "IoTHub.Portal/Client/"]
RUN dotnet restore "IoTHub.Portal/Server/IoTHub.Portal.Server.csproj"
COPY . .
WORKDIR "/src/IoTHub.Portal/Server"
RUN dotnet build "IoTHub.Portal.Server.csproj" -c Release -o /app/build -p:Version=${BUILD_VERSION}

FROM builder AS publish
RUN dotnet publish "IoTHub.Portal.Server.csproj" -c Release -o /app/publish

FROM base AS final

HEALTHCHECK CMD curl --fail http://localhost/healthz || exit

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IoTHub.Portal.Server.dll"]
