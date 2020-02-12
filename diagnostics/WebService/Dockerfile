FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS restore
WORKDIR /src
COPY Directory.Build.props .
COPY StyleCop.json .
COPY Mmm.Iot.ruleset .
COPY common/Services/Services.csproj common/Services/
COPY diagnostics/Services/Services.csproj diagnostics/Services/
COPY diagnostics/WebService/WebService.csproj diagnostics/WebService/
RUN dotnet restore diagnostics/WebService/WebService.csproj
COPY common common
COPY diagnostics diagnostics

FROM restore AS publish
ARG AppConfigurationConnectionString=
WORKDIR /src/diagnostics/WebService
RUN dotnet publish WebService.csproj \
--output /app \
--configuration Release \
--no-restore \
--no-dependencies

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Mmm.Iot.Diagnostics.WebService.dll"]