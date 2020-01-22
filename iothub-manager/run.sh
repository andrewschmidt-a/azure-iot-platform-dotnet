#!/usr/bin/env bash

cd /app/

#Can add environment variables here

cd webservice && dotnet Mmm.Iot.IoTHubManager.WebService.dll && \
    fg
