FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["auth/Services/Services.csproj", "auth/Services/"]
COPY ["config/AuthUtils/AuthUtils.csproj", "config/AuthUtils/"]
COPY ["config/Services/Services.csproj", "config/Services/"]
COPY ["config/WebService/WebService.csproj", "config/WebService/"]

RUN dotnet restore "config/WebService/WebService.csproj"
COPY . .
WORKDIR "/src/config/WebService"
RUN dotnet build "WebService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Microsoft.Azure.IoTSolutions.UIConfig.WebService.dll"]