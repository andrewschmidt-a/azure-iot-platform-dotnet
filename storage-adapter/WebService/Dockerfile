FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS restore
WORKDIR /src
COPY Directory.Build.props .
COPY common/AuthUtils/AuthUtils.csproj common/AuthUtils/
COPY common/AppConfiguration/AppConfiguration.csproj common/AppConfiguration/
COPY common/Services/Services.csproj common/Services/
COPY common/WebService/WebService.csproj common/WebService/
COPY storage-adapter/Services/Services.csproj storage-adapter/Services/
COPY storage-adapter/WebService/WebService.csproj storage-adapter/WebService/
RUN dotnet restore storage-adapter/WebService/WebService.csproj
COPY common common
COPY storage-adapter storage-adapter

FROM restore AS publish
WORKDIR /src/storage-adapter/WebService
RUN dotnet publish WebService.csproj \
--output /app \
--configuration Release \
--no-restore \
--no-dependencies

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.dll"]