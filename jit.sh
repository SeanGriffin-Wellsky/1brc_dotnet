#!/bin/bash

hyperfine --warmup 1 --runs 5 "ConsoleApp/bin/Release/net8.0/ConsoleApp $1 "
