FROM microsoft/dotnet:2.0.3-sdk-jessie as build
COPY . /src
WORKDIR /src
RUN dotnet restore
RUN dotnet build --configuration Release
RUN dotnet publish WebService --configuration Release --output /app/webservice



FROM microsoft/dotnet:2.0.3-runtime-jessie

MAINTAINER Devis Lucato (https://github.com/dluc)

LABEL Tags="Azure,IoT,Solutions,IoT Hub,Storage,.NET"

ARG user=pcsuser

RUN useradd -m -s /bin/bash -U $user
ENTRYPOINT ["/bin/bash", "/app/run.sh"]

COPY --from=build /app /app
COPY run.sh /app

RUN chown -R $user.$user /app
WORKDIR /app

USER $user
