#!/bin/bash

hyperfine --warmup 1 --runs 5 "./bin/Release/net8.0/ConsoleApp"
