FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS restore
WORKDIR /src
COPY Directory.Build.props .
COPY common/Services/Services.csproj common/Services/
COPY asa-manager/Services/Services.csproj asa-manager/Services/
COPY asa-manager/WebService/WebService.csproj asa-manager/WebService/
RUN dotnet restore asa-manager/WebService/WebService.csproj
COPY common common
COPY asa-manager asa-manager

FROM restore AS publish
ARG AppConfigurationConnectionString=
WORKDIR /src/asa-manager/WebService
RUN dotnet publish WebService.csproj \
--output /app \
--configuration Release \
--no-restore \
--no-dependencies

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Mmm.Platform.IoT.AsaManager.WebService.dll"]