#!/bin/bash

dotnet build -c Release ConsoleApp/ConsoleApp.csproj
dotnet publish -f net8.0 -c Release ConsoleApp/ConsoleApp.csproj
