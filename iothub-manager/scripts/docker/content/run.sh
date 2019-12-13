#!/usr/bin/env bash

cd /app/

cd webservice && dotnet Mmm.Platform.IoT.IoTHubManager.WebService.dll && \
    fg
