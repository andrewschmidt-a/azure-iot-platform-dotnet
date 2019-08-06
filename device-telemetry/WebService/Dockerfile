FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["ActionsAgent/ActionsAgent.csproj", "ActionsAgent/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["WebService/WebService.csproj", "WebService/"]

RUN dotnet restore "WebService/WebService.csproj"
COPY . .
WORKDIR "/src/WebService"
RUN dotnet build "WebService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.dll"]