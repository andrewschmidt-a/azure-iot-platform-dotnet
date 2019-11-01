FROM microsoft/dotnet:2.2-sdk AS build
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

FROM microsoft/dotnet:2.2-sdk
RUN dotnet dev-certs https
COPY --from=build /app /app
ENTRYPOINT ["dotnet", "/app/webservice/tenant-manager.dll"]
EXPOSE 80
