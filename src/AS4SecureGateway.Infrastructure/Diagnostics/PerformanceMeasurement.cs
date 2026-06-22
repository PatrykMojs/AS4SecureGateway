using System.Diagnostics;

namespace AS4SecureGateway.Infrastructure.Diagnostics;

public static class PerformanceMeasurement
{
    public static async Task<PerformanceMetrics> MeasureAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var process = Process.GetCurrentProcess();

        var stopwatch = Stopwatch.StartNew();
        var initialMemory = GC.GetTotalMemory(forceFullCollection: false);
        var initialCpuTime = process.TotalProcessorTime;
        var initialGen0 = GC.CollectionCount(0);
        var initialGen1 = GC.CollectionCount(1);
        var initialGen2 = GC.CollectionCount(2);
        var initialThreadCount = process.Threads.Count;

        await action();

        stopwatch.Stop();

        var finalMemory = GC.GetTotalMemory(forceFullCollection: false);
        var finalCpuTime = process.TotalProcessorTime;
        var finalThreadCount = process.Threads.Count;

        return new PerformanceMetrics
        {
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            MemoryUsageBytes = finalMemory - initialMemory,
            CpuTime = finalCpuTime - initialCpuTime,
            Gen0Collections = GC.CollectionCount(0) - initialGen0,
            Gen1Collections = GC.CollectionCount(1) - initialGen1,
            Gen2Collections = GC.CollectionCount(2) - initialGen2,
            ThreadCountChange = finalThreadCount - initialThreadCount
        };
    }

    public static PerformanceMetrics Measure(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return MeasureAsync(() =>
        {
            action();
            return Task.CompletedTask;
        }).GetAwaiter().GetResult();
    }
}