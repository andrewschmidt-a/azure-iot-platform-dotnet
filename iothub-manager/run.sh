#!/usr/bin/env bash

cd /app/

#Can add environment variables here

cd webservice && dotnet Mmm.Platform.IoT.IoTHubManager.WebService.dll && \
    fg
