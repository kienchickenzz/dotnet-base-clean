/**
 * RequestActivityCounter provides lock-free atomic counters for HTTP request tracking.
 *
 * <p>Middleware increments on request entry and decrements on exit.
 * PerfMonitorService reads snapshots on its own timer for metrics reporting.</p>
 */
namespace BaseCleanArchitecture.Api.Monitoring;

/// <summary>
/// Lock-free atomic counters for tracking in-flight HTTP requests.
/// </summary>
public static class RequestActivityCounter
{
    private static long _activeQueryRequests;
    private static long _activeIngestionRequests;

    /// <summary>
    /// Gets the current count of active query requests.
    /// </summary>
    public static long ActiveQueryRequests => Interlocked.Read(ref _activeQueryRequests);

    /// <summary>
    /// Gets the current count of active ingestion requests.
    /// </summary>
    public static long ActiveIngestionRequests => Interlocked.Read(ref _activeIngestionRequests);

    /// <summary>
    /// Increments the query request counter atomically.
    /// </summary>
    public static void IncrementQuery() => Interlocked.Increment(ref _activeQueryRequests);

    /// <summary>
    /// Decrements the query request counter atomically.
    /// </summary>
    public static void DecrementQuery() => Interlocked.Decrement(ref _activeQueryRequests);

    /// <summary>
    /// Increments the ingestion request counter atomically.
    /// </summary>
    public static void IncrementIngestion() => Interlocked.Increment(ref _activeIngestionRequests);

    /// <summary>
    /// Decrements the ingestion request counter atomically.
    /// </summary>
    public static void DecrementIngestion() => Interlocked.Decrement(ref _activeIngestionRequests);
}
