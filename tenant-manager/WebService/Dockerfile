FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src

COPY ["./Directory.Build.props", "/src/"]
COPY ["WebService/tenant-manager.csproj", "WebService/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["AppConfiguration/AppConfiguration.csproj", "AppConfiguration/"]
COPY ["AuthUtils/AuthUtils.csproj", "AuthUtils/"]

WORKDIR /src/WebService
RUN dotnet restore

COPY [".", "/src/"]
RUN dotnet build --configuration Release
RUN dotnet publish . --configuration Release --output /app/webservice

FROM mcr.microsoft.com/dotnet/core/runtime:2.2
COPY --from=build /app /app
ENTRYPOINT ["dotnet", "/app/webservice/tenant-manager.dll"]
EXPOSE 80
