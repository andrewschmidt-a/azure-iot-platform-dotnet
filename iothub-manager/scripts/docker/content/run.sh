#!/usr/bin/env bash

cd /app/

cd webservice && dotnet Mmm.Iot.IoTHubManager.WebService.dll && \
    fg
