
## System Specs

2.4 GHz 8-Core Intel Core i9\
L2 Cache (per Core): 256 KB\
L3 Cache:	16 MB\
32 GB 2667 MHz DDR4\
Apple SSD AP0512N 512 GB
Hyperthreading: *Off*

## Baseline C# Runtime

260,985ms (260.985s, 4.35m)
7.47 GB in LOH
GC time is ~17.5% of total time

## Pre-allocate Temperature List

253,037ms (253.037s, 4.22m) - 3.05% improvement over baseline

## Read into blocks of char arrays

285,698ms (285.698s, 4.76m) - 9.5% slower than baseline
GC time is ~17.3% of total time

## Read into blocks of byte arrays

406,350ms (406.350s, 6.77m) - 55.7% slower than baseline
GC time is ~23% of total time

![Memory Snapshot](./assets/MemorySnapshot1.png)

## Read into blocks of byte spans

210,162ms (210.162, 3.5m) - 19.5% improvement over baseline
GC time is ~13% of total time

![Memory Snapshot](./assets/MemorySnapshot2.png)

## Read into blocks of char spans

187,307ms (187.307s, 3.12m) - 28% improvement over baseline
GC time is ~7% of total time (11,683ms)

![Memory Snapshot](./assets/MemorySnapshot3.png)

## Calculate temperature statistics on the fly

244,068ms (244.068s, 4.07m) - 6.4% slower than baseline
Heap bounces between 380 MB and 770 MB in LOH
GC time is ~6.7% of total time (15,532ms)

![Memory Snapshot](./assets/MemorySnapshot4.png)
