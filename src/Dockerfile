#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.



FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
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
ARG BUILD_VERSION=1.2.3
ARG GITHUB_RUN_NUMBER=4
COPY ["AzureIoTHub.Portal/Server/AzureIoTHub.Portal.Server.csproj", "AzureIoTHub.Portal/Server/"]
COPY ["AzureIoTHub.Portal/Shared/AzureIoTHub.Portal.Shared.csproj", "AzureIoTHub.Portal/Shared/"]
COPY ["AzureIoTHub.Portal/Client/AzureIoTHub.Portal.Client.csproj", "AzureIoTHub.Portal/Client/"]
RUN dotnet restore "AzureIoTHub.Portal/Server/AzureIoTHub.Portal.Server.csproj"
COPY . .
WORKDIR "/src/AzureIoTHub.Portal/Server"
RUN dotnet build "AzureIoTHub.Portal.Server.csproj" -c Release -o /app/build -p:Version="${BUILD_VERSION}.${GITHUB_RUN_NUMBER}"

FROM builder AS publish
RUN dotnet publish "AzureIoTHub.Portal.Server.csproj" -c Release -o /app/publish  -p:Version="${BUILD_VERSION}.${GITHUB_RUN_NUMBER}"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AzureIoTHub.Portal.Server.dll"]
