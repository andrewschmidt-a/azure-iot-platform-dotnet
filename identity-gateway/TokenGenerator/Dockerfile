FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["TokenGenerator/TokenGenerator.csproj", "TokenGenerator/"]

RUN dotnet restore "TokenGenerator/TokenGenerator.csproj"
COPY . .
WORKDIR "/src/TokenGenerator"
RUN dotnet build "TokenGenerator.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "TokenGenerator.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TokenGenerator.dll"]