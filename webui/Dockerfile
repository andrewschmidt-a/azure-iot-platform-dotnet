FROM node:11-alpine as codebuilder
COPY ./ /app/
WORKDIR /app
RUN echo "Install Bash for scripts"     && apk add --no-cache --virtual bash
RUN echo "Installing the python2 (reqd for node pckgs)" && apk --update add python3
RUN echo "Install GCC for make command" && apk add --no-cache --virtual build-base
RUN echo "Install Curl for scripts"     && apk add --no-cache --virtual curl

MAINTAINER Devis Lucato (https://github.com/dluc)

LABEL Tags="Azure,IoT,Solutions,React,SPA"

COPY ./ /app
WORKDIR /app

RUN echo "Installing web server..." \
 && apk add --no-cache nginx \
 && mkdir /app/logs \
 && mkdir -p /var/lib/nginx /var/cache/nginx /var/tmp/nginx /var/log/nginx \
 && echo "Removing unused files..." \
 && apk del --force --purge alpine-keys apk-tools \
 && rm -rf /var/cache/apk /etc/apk /lib/apk

EXPOSE 10080
VOLUME /app/logs

RUN ["chmod", "+x", "/app/run.sh"]
RUN "/app/run.sh"
ENTRYPOINT [ "nginx", "-c", "/app/nginx.conf" ]
#ENTRYPOINT ["/bin/bash", "-c", "/app/run.sh"]
