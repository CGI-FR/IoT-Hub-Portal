#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["AzureIoTHub.Portal/Server/AzureIoTHub.Portal.Server.csproj", "AzureIoTHub.Portal/Server/"]
COPY ["AzureIoTHub.Portal/Shared/AzureIoTHub.Portal.Shared.csproj", "AzureIoTHub.Portal/Shared/"]
COPY ["AzureIoTHub.Portal/Client/AzureIoTHub.Portal.Client.csproj", "AzureIoTHub.Portal/Client/"]
RUN dotnet restore "AzureIoTHub.Portal/Server/AzureIoTHub.Portal.Server.csproj"
COPY . .
WORKDIR "/src/AzureIoTHub.Portal/Server"
RUN dotnet build "AzureIoTHub.Portal.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AzureIoTHub.Portal.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AzureIoTHub.Portal.Server.dll"]