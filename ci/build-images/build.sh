#!/bin/

# project
cd /app/PicoStreamingAssistantFTUDP

# get the dependencies
git submodule update --init --recursive

# Restore dependencies
dotnet restore

# Build the project
dotnet build PicoStreamingAssistantFTUDP/Pico4SAFTExtTrackingModule.csproj --graph --configuration Release --no-restore
# Publish the build output
dotnet publish PicoStreamingAssistantFTUDP/Pico4SAFTExtTrackingModule.csproj --graph --configuration Release --no-build --no-restore --output /app/artifacts