FROM valian/docker-nginx-auto-ssl

# MAINTAINER Devis Lucato (https://github.com/dluc)

# LABEL Tags="Azure,IoT,Solutions,Proxy,nginx"

# ARG user=app

# RUN groupadd $user && useradd --no-log-init -r -g $user $user

# COPY ./content/ /app
RUN  mkdir -p /app/config/  \
 && mkdir -p /var/lib/nginx /var/cache/nginx /var/tmp/nginx \
#  && chown -R $user:$user /var/lib/nginx /var/cache/nginx /var/tmp/nginx

RUN mkdir /app/logs && touch /app/logs/error.log && touch /app/logs/nginx.pid 
# WORKDIR /app

# TODO: remove port 10080
EXPOSE 10080 
EXPOSE 10443
EXPOSE 443
# VOLUME ["/app/certs", "/app/config"]
COPY ./content/nginx.conf /usr/local/openresty/nginx/conf/

# ENTRYPOINT ["/bin/bash", "/app/run.sh"]

# USER $user