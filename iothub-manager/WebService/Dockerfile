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
COPY iothub-manager/Services/Services.csproj iothub-manager/Services/
COPY iothub-manager/WebService/WebService.csproj iothub-manager/WebService/
RUN dotnet restore iothub-manager/WebService/WebService.csproj
COPY common common
COPY iothub-manager iothub-manager

FROM restore AS publish
WORKDIR /src/iothub-manager/WebService
RUN dotnet publish WebService.csproj \
--output /app \
--configuration Release \
--no-restore \
--no-dependencies

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Microsoft.Azure.IoTSolutions.IotHubManager.WebService.dll"]