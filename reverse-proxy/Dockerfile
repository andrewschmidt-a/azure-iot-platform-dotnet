FROM nginx
RUN mkdir /app && mkdir /app/logs && touch /app/logs/error.log
COPY ./content/nginx.conf /etc/nginx/nginx.conf
EXPOSE 10080