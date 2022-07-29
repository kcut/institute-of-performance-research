# 성능연구소

## Welcome to Institute of Performance Research Kim Chaek University.

Is for performance testing of libraries for Discord platform. Contain tests for REST internet and WebSocket internet
client, various body size.

Some library designed for ease with good abstraction, can test direct. Others not, need reflection. Good example
library **DSharpPlus** design by Emzi, cannot to test REST internet rate limit code because deadlocks. Good desing.

## Maintainer

Institute pleased to work with library for inclusion in performance measurement. If existing library maintainer problem
or issue with methodology please contact and can work on fix.
Goal is objective measurement and test end to end for improval of .NET library ecosystem.

## Result

| Term               | Define                                                   |
|--------------------|----------------------------------------------------------|
| RestApiCall        | REST internet API call                                   |
| WebSocketLifecycle | HELLO, READY, GUILD_CREATE, disconnect                   |
| LargeBody          | Contain list of guild channel, large document            |
| SmallBody          | Empty list                                               |
| LowLevel           | Basic abstraction - request to get model, no rich entity |
| HighLevel          | Build on LowLevel - rich entity, no direct JSON model    |
| LargeGuildCreate   | Very large guild payload                                 |
| SmallGuildCreate   | Very small guild payload                                 |

### Discord.Net

|                             Method |        Job | UnrollFactor |           Mean |        Error |        StdDev |      Gen 0 |      Gen 1 |     Gen 2 |  Allocated |
|----------------------------------- |----------- |------------- |---------------:|-------------:|--------------:|-----------:|-----------:|----------:|-----------:|
|       RestApiCallLargeBodyLowLevel | DefaultJob |           16 |     3,058.0 μs |     56.61 μs |      52.95 μs |    93.7500 |    46.8750 |   46.8750 |     646 KB |
|       RestApiCallEmptyBodyLowLevel | DefaultJob |           16 |       232.9 μs |      3.32 μs |       2.94 μs |     1.4648 |     0.7324 |         - |      14 KB |
| WebSocketLifecycleLargeGuildCreate | Job-IHORUM |            1 | 2,658,264.5 μs | 94,780.85 μs | 276,480.19 μs | 73000.0000 | 36000.0000 | 6000.0000 | 609,785 KB |
| WebSocketLifecycleSmallGuildCreate | Job-IHORUM |            1 |     8,586.0 μs |    100.72 μs |      89.29 μs |          - |          - |         - |   2,096 KB |

### Disqord

|                             Method |        Job | UnrollFactor |            Mean |         Error |        StdDev |      Gen 0 |      Gen 1 |     Gen 2 |  Allocated |
|----------------------------------- |----------- |------------- |----------------:|--------------:|--------------:|-----------:|-----------:|----------:|-----------:|
|      RestApiCallLargeBodyHighLevel | DefaultJob |           16 |     3,927.43 μs |     75.361 μs |     95.307 μs |    93.7500 |    46.8750 |   46.8750 |     857 KB |
|       RestApiCallLargeBodyLowLevel | DefaultJob |           16 |     3,917.64 μs |     68.169 μs |     63.765 μs |    93.7500 |    46.8750 |   46.8750 |     850 KB |
|      RestApiCallEmptyBodyHighLevel | DefaultJob |           16 |        20.54 μs |      0.402 μs |      1.059 μs |     1.2207 |          - |         - |      10 KB |
|       RestApiCallEmptyBodyLowLevel | DefaultJob |           16 |        22.27 μs |      0.646 μs |      1.904 μs |     1.1902 |          - |         - |      10 KB |
| WebSocketLifecycleLargeGuildCreate | Job-IHORUM |            1 | 1,493,037.40 μs | 19,421.511 μs | 16,217.842 μs | 41000.0000 | 20000.0000 | 3000.0000 | 322,348 KB |
| WebSocketLifecycleSmallGuildCreate | Job-IHORUM |            1 |     9,022.59 μs |    137.582 μs |    121.963 μs |          - |          - |         - |   1,990 KB |

### Remora.Discord

|                             Method |        Job | UnrollFactor |             Mean |          Error |         StdDev |       Gen 0 |      Gen 1 |    Allocated |
|----------------------------------- |----------- |------------- |-----------------:|---------------:|---------------:|------------:|-----------:|-------------:|
|       RestApiCallLargeBodyLowLevel | DefaultJob |           16 |    40,147.955 μs |    794.9488 μs |    743.5956 μs |   1384.6154 |   615.3846 |    12,023 KB |
|       RestApiCallEmptyBodyLowLevel | DefaultJob |           16 |         4.292 μs |      0.0739 μs |      0.0617 μs |      0.5035 |          - |         4 KB |
| WebSocketLifecycleLargeGuildCreate | Job-IHORUM |            1 | 6,003,956.660 μs | 91,207.7798 μs | 85,315.8138 μs | 217000.0000 | 79000.0000 | 1,973,344 KB |
| WebSocketLifecycleSmallGuildCreate | Job-IHORUM |            1 |    59,789.081 μs |  3,375.3438 μs |  9,952.2773 μs |   1000.0000 |  1000.0000 |    11,917 KB |

Remore design not easy abstract - use patching, replace `ClientWebSocket` with no-op stub. Should provide best-case performance no network call, instant return only copy bytes.
Attempt contact library development for necessary abstract, can follow progress [Remora.Discord#224](https://github.com/Nihlus/Remora.Discord/issues/224) issue here.

### DSharpPlus

|                   Method | Mean | Error |
|------------------------- |-----:|------:|
|          RestApiCallCost |   NA |    NA |
| RestApiCallCostEmptyBody |   NA |    NA |

Benchmarks with issues:

* DSharpPlusBenchmarks.RestApiCallCost: Deadlock
* DSharpPlusBenchmarks.RestApiCallCostEmptyBody: Deadlock
