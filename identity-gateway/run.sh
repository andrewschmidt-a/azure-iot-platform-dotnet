#!/usr/bin/env bash

cd /app/

cd tokengenerator && dotnet TokenGenerator.dll && \
    fg
