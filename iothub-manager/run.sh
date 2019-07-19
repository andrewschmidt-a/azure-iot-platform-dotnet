#!/usr/bin/env bash

cd /app/

#Can add environment variables here

cd webservice && dotnet Microsoft.Azure.IoTSolutions.IotHubManager.WebService.dll && \
    fg
