FROM microsoft/dotnet:2.2-sdk AS build
COPY . /src
WORKDIR /src/WebService
RUN dotnet restore
RUN dotnet build --configuration Release
RUN dotnet publish . --configuration Release --output /app/webservice

FROM microsoft/dotnet:2.2-sdk
RUN dotnet dev-certs https
COPY --from=build /app /app
ENTRYPOINT ["dotnet", "/app/webservice/tenant-manager.dll"]
EXPOSE 80
