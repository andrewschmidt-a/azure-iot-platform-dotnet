FROM mcr.microsoft.com/dotnet/core/runtime:2.2 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["./Directory.Build.props", "/src/"]
COPY ["AppConfiguration/AppConfiguration.csproj", "AppConfiguration/"]
COPY ["AuthUtils/AuthUtils.csproj", "AuthUtils/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["WebService/WebService.csproj", "WebService/"]

RUN dotnet restore "WebService/WebService.csproj"
COPY . .
WORKDIR /src/WebService
RUN dotnet build "WebService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebService.dll"]