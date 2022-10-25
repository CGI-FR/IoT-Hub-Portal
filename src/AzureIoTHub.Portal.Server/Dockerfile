#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG BUILD_VERSION=1.0.0-dev

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
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
	  apt-get install -y dotnet-sdk-6.0

FROM build AS builder
WORKDIR /src
COPY ["AzureIoTHub.Portal/Server/AzureIoTHub.Portal.Server.csproj", "AzureIoTHub.Portal/Server/"]
COPY ["AzureIoTHub.Portal/Shared/AzureIoTHub.Portal.Shared.csproj", "AzureIoTHub.Portal/Shared/"]
COPY ["AzureIoTHub.Portal/Client/AzureIoTHub.Portal.Client.csproj", "AzureIoTHub.Portal/Client/"]
RUN dotnet restore "AzureIoTHub.Portal/Server/AzureIoTHub.Portal.Server.csproj"
COPY . .
WORKDIR "/src/AzureIoTHub.Portal/Server"
RUN dotnet build "AzureIoTHub.Portal.Server.csproj" -c Release -o /app/build -p:Version=${BUILD_VERSION}

FROM builder AS publish
RUN dotnet publish "AzureIoTHub.Portal.Server.csproj" -c Release -o /app/publish

FROM base AS final

HEALTHCHECK CMD curl --fail http://localhost/healthz || exit

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AzureIoTHub.Portal.Server.dll"]
