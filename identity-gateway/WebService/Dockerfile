FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS restore
WORKDIR /src
COPY Directory.Build.props .
COPY StyleCop.json .
COPY Mmm.Iot.ruleset .
COPY common/Services/Services.csproj common/Services/
COPY identity-gateway/Services/Services.csproj identity-gateway/Services/
COPY identity-gateway/WebService/WebService.csproj identity-gateway/WebService/
RUN dotnet restore identity-gateway/WebService/WebService.csproj
COPY common common
COPY identity-gateway identity-gateway

FROM restore AS publish
ARG AppConfigurationConnectionString=
WORKDIR /src/identity-gateway/WebService
RUN dotnet publish WebService.csproj \
--output /app \
--configuration Release \
--no-restore \
--no-dependencies

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Mmm.Platform.IoT.IdentityGateway.WebService.dll"]