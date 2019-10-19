FROM node:11-alpine as codebuilder
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

COPY ./ /app
ENTRYPOINT ["nginx", "-c", "/app/nginx.conf"]
