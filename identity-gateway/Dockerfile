FROM microsoft/dotnet:2.2-sdk as build
COPY . /src
WORKDIR /src/TokenGenerator
RUN dotnet restore
RUN dotnet build --configuration Release
RUN dotnet publish . --configuration Release --output /app/tokengenerator

#FROM microsoft/dotnet:2.2.0-aspnetcore-runtime
FROM microsoft/dotnet:2.2-sdk

MAINTAINER Devis Lucato (https://github.com/dluc)

LABEL Tags="Azure,IoT,Solutions,IoT Hub,.NET"

#ARG user=pcsuser

#RUN useradd -m -s /bin/bash -U $user

ENTRYPOINT ["dotnet", "/app/tokengenerator/TokenGenerator.dll"]
RUN dotnet dev-certs https
COPY --from=build /app /app
COPY run.sh /app

RUN chown -R $user.$user /app
WORKDIR /app

USER $user

EXPOSE 5001
EXPOSE 5000