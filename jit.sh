#!/bin/bash

hyperfine --warmup 1 --runs 10 "ConsoleApp/bin/Release/net8.0/ConsoleApp $1 "
