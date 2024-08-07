#!/bin/bash

hyperfine --warmup 1 --runs 10 "./ConsoleApp/bin/Release/net8.0/osx-x64/publish/ConsoleApp $1 "
