#!/bin/bash

echo "Publishing .NET app..."
dotnet publish src/API/MFFVP.Api/MFFVP.Api.csproj \
  -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -o publish

echo "Copying Dockerfile to publish directory..."
cp src/API/MFFVP.Api/Dockerfile publish/

echo "Building Docker image..."
docker build -t mffvp-test publish/

echo "Removing existing container (if running)..."
docker rm -f mffvp-api 2>/dev/null

echo "Running new container..."
docker run -d -e PORT=8080 -p 8080:8080 --name mffvp-api mffvp-test

echo "Done. Visit http://localhost:8080/"
