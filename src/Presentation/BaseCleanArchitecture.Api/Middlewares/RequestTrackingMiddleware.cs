/**
 * RequestTrackingMiddleware tracks HTTP request lifecycle for performance monitoring.
 *
 * <p>Increments/decrements RequestActivityCounter based on request path.
 * Should be placed early in the pipeline to capture full request lifetime.</p>
 */
namespace BaseCleanArchitecture.Api.Middlewares;

using BaseCleanArchitecture.Api.Monitoring;

/// <summary>
/// Middleware that tracks active HTTP requests by category.
/// </summary>
public class RequestTrackingMiddleware
{
    /// <summary>
    /// URL segments that indicate ingestion operations.
    /// </summary>
    private static readonly HashSet<string> IngestionSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "import",
        "upload",
        "bulk",
        "batch"
    };

    private readonly RequestDelegate _next;

    public RequestTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Processes the request, tracking its lifecycle in the activity counter.
    /// </summary>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var category = Classify(httpContext.Request);

        switch (category)
        {
            case RequestCategory.Ingestion:
                RequestActivityCounter.IncrementIngestion();
                break;
            case RequestCategory.Query:
                RequestActivityCounter.IncrementQuery();
                break;
        }

        try
        {
            await _next(httpContext);
        }
        finally
        {
            // Decrement in finally block ensures counter is always decremented
            switch (category)
            {
                case RequestCategory.Ingestion:
                    RequestActivityCounter.DecrementIngestion();
                    break;
                case RequestCategory.Query:
                    RequestActivityCounter.DecrementQuery();
                    break;
            }
        }
    }

    /// <summary>
    /// Classifies the request into Query, Ingestion, or None category.
    /// </summary>
    private static RequestCategory Classify(HttpRequest request)
    {
        var path = request.Path;
        if (!path.HasValue)
        {
            return RequestCategory.None;
        }

        var pathValue = path.Value!;

        // Health check and swagger endpoints are not tracked
        if (pathValue.Contains("health", StringComparison.OrdinalIgnoreCase) ||
            pathValue.Contains("swagger", StringComparison.OrdinalIgnoreCase))
        {
            return RequestCategory.None;
        }

        // Ingestion: POST requests to specific segments
        if (string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var segment in IngestionSegments)
            {
                if (pathValue.Contains(segment, StringComparison.OrdinalIgnoreCase))
                {
                    return RequestCategory.Ingestion;
                }
            }
        }

        // All other API requests count as query
        if (pathValue.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            return RequestCategory.Query;
        }

        return RequestCategory.None;
    }

    private enum RequestCategory
    {
        None,
        Query,
        Ingestion
    }
}
