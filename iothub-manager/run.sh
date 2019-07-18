#!/usr/bin/env bash

cd /app/

#add environment variables here
#export VARNAME="my value"  
export PCS_AAD_APPSECRET=""
export PCS_AAD_APPID=""
export PCS_KEYVAULT_NAME=""
export PCS_APPLICATION_CONFIGURATION=""
export PCS_STORAGE_URL=""
export PCS_STORAGE_URL=""

cd webservice && dotnet Microsoft.Azure.IoTSolutions.IotHubManager.WebService.dll && \
    fg
