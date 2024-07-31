
## System Specs

2.4 GHz 8-Core Intel Core i9\
L2 Cache (per Core): 256 KB\
L3 Cache:	16 MB\
32 GB 2667 MHz DDR4\
Apple SSD AP0512N 512 GB

## Baseline C# Runtime

234,833ms (234.833s, 3m54s)
7.47 GB in LOH
GC time is ~17.5% of total time (41,096ms)

## Pre-allocate Temperature List

233,307ms (233.307s, 3m53s) - 0.65% improvement over baseline

## Read into blocks of char arrays

253,976ms (253.976s, 4m14s) - 8.2% slower than baseline
GC time is ~17.3% of total time (43,938ms)

## Read into blocks of byte arrays

372,823ms (372.823s, 6m13s) - 58.8% slower than baseline
GC time is ~23% of total time (85,749ms)

![Memory Snapshot](./assets/MemorySnapshot1.png)

## Read into blocks of byte spans

199,262ms (199.262s, 3m19s) - 15.1% improvement over baseline
GC time is ~13% of total time (25,904ms)

![Memory Snapshot](./assets/MemorySnapshot2.png)

## Read into blocks of char spans

166,898ms (166.898s, 2m46s) - 29.1% improvement over baseline
GC time is ~7% of total time (11,683ms)

![Memory Snapshot](./assets/MemorySnapshot3.png)
