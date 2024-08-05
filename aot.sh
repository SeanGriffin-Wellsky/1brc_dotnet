#!/bin/bash

hyperfine --show-output --warmup 1 --runs 5 "./ConsoleApp/bin/Release/net8.0/osx-x64/publish/ConsoleApp $1 "
