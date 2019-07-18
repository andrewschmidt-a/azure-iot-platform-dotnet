#!/usr/bin/env bash

cd /app/

cd webservice

#add environment variables here
#export VARNAME="my value"  

export PCS_APPLICATION_CONFIGURATION=""

dotnet Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.dll
