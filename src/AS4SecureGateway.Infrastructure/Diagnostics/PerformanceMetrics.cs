namespace AS4SecureGateway.Infrastructure.Diagnostics;

public sealed class PerformanceMetrics
{
    public long ElapsedMilliseconds { get; init; }
    public long MemoryUsageBytes { get; init; }
    public TimeSpan CpuTime { get; init; }
    public int Gen0Collections { get; init; }
    public int Gen1Collections { get; init; }
    public int Gen2Collections { get; init; }
    public int ThreadCountChange { get; init; }

    public override string ToString()
    {
        return
            $"Elapsed: {ElapsedMilliseconds} ms, " +
            $"Memory: {MemoryUsageBytes} bytes, " +
            $"CPU: {CpuTime.TotalMilliseconds} ms, " +
            $"GC Gen0/1/2: {Gen0Collections}/{Gen1Collections}/{Gen2Collections}, " +
            $"Thread change: {ThreadCountChange}";
    }
}