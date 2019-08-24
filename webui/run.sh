#!/usr/bin/env bash
export NODE_PATH=src/  # fixes the "cannot find module app.config" issue

echo "Installing node packages ..." && npm --add-python-to-path='true' install
echo "Building app..."              && npm run build
#echo "Removing temp files..."       && rm -rf node_modules src public package.json Dockerfile .dockerignore

set -e
pwd && ls

# copying webui config
cp /app/public/webui-config.js /app/webui-config.js

# call in current shell.
echo "Creating/Updating web config"
#. /app/set_env.sh AUTH authRequired TENANT aadTenantId INSTANCE_URL "-"  # Uses Keyvault to set environment variables
cp /app/webui-config.js /app/build/webui-config.js

echo "Starting server"
# serve the app via nginx
mkdir -p /app/logs
#nginx -c /app/nginx.conf
