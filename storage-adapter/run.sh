#!/usr/bin/env bash

cd /app/

cd webservice

#Can add environment variables here 
echo $PCS_KEYVAULT_NAME
dotnet Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.dll
