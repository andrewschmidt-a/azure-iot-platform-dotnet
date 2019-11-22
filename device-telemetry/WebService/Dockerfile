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
COPY device-telemetry/Services/Services.csproj device-telemetry/Services/
COPY device-telemetry/WebService/WebService.csproj device-telemetry/WebService/
RUN dotnet restore device-telemetry/WebService/WebService.csproj
COPY common common
COPY device-telemetry device-telemetry

FROM restore AS publish
WORKDIR /src/device-telemetry/WebService
RUN dotnet publish WebService.csproj \
--output /app \
--configuration Release \
--no-restore \
--no-dependencies

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Mmm.Platform.IoT.DeviceTelemetry.WebService.dll"]