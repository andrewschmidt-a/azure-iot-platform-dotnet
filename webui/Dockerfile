FROM node:11-alpine as codebuilder
WORKDIR /app
RUN echo "Install Bash for scripts"     && apk add --no-cache --virtual bash
RUN echo "Installing the python2 (reqd for node pckgs)" && apk --update add python3
RUN echo "Install GCC for make command" && apk add --no-cache --virtual build-base
RUN echo "Install Curl for scripts"     && apk add --no-cache --virtual curl
RUN echo "Install JSON parser for app config" && apk add --no-cache --virtual jq

RUN \
  echo "Installing Azure cli" \
  apk update && \
  apk add bash py-pip && \
  apk add --virtual=build gcc libffi-dev musl-dev openssl-dev python-dev make && \
  pip --no-cache-dir install -U pip && \
  pip --no-cache-dir install azure-cli && \
  apk del --purge build

RUN echo "Install App Configuration"     && az extension add -n appconfig


MAINTAINER Andrew Schmidt (https://github.com/andrewschmidt-a)

LABEL Tags="Azure,IoT,Solutions,React,SPA"

RUN echo "Installing web server..." \
 && apk add --no-cache nginx \
 && mkdir /app/logs \
 && mkdir -p /var/lib/nginx /var/cache/nginx /var/tmp/nginx /var/log/nginx \
 && echo "Removing unused files..." \
 && apk del --force --purge alpine-keys apk-tools \
 && rm -rf /var/cache/apk /etc/apk /lib/apk

EXPOSE 10080
VOLUME /app/logs

COPY package.json .
COPY azure-iot-ux-fluent-controls/package.json azure-iot-ux-fluent-controls/package.json
COPY azure-iot-ux-fluent-css/package.json azure-iot-ux-fluent-css/package.json
RUN echo "Installing node packages ..." && npm --add-python-to-path='true' install
COPY ./ /app
RUN ["chmod", "+x", "/app/run.sh"]
RUN ["chmod", "+x", "/app/build.sh"]
RUN ["chmod", "+x", "/app/set_env.sh"]
RUN "/app/build.sh"

ENTRYPOINT ["/bin/bash", "-c", "/app/run.sh"]
