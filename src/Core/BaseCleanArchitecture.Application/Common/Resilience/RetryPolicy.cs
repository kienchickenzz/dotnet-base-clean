/**
 * RetryPolicyFactory provides resilience policies using Polly.
 *
 * <p>Creates retry policies with exponential backoff for HTTP calls
 * and general exception handling scenarios.</p>
 */
namespace BaseCleanArchitecture.Application.Common.Resilience;

using System.Net;
using System.Net.Http;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Retry;

/// <summary>
/// Factory for creating retry policies with logging support.
/// </summary>
public static class RetryPolicyFactory
{
    private const int MaxRetryAttempts = 3;

    /// <summary>
    /// Creates an HTTP retry policy with exponential backoff.
    /// Retries on transient HTTP errors (5xx, 408) and network exceptions.
    /// </summary>
    public static AsyncRetryPolicy<HttpResponseMessage> CreateHttpRetryPolicy(ILogger logger)
    {
        return Policy
            .HandleResult<HttpResponseMessage>(response =>
                !response.IsSuccessStatusCode && _IsRetryableStatusCode(response.StatusCode))
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                MaxRetryAttempts,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (result, timeSpan, retryCount, context) =>
                {
                    if (result.Exception != null)
                    {
                        logger.LogWarning(
                            "Retry {RetryCount} due to exception: {Message}",
                            retryCount,
                            result.Exception.Message);
                    }
                    else
                    {
                        logger.LogWarning(
                            "Retry {RetryCount} due to HTTP status: {StatusCode}",
                            retryCount,
                            result.Result.StatusCode);
                    }
                });
    }

    /// <summary>
    /// Creates a general retry policy with exponential backoff.
    /// Retries on any exception.
    /// </summary>
    public static AsyncRetryPolicy CreateGeneralRetryPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                MaxRetryAttempts,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        "Retry {RetryCount} due to exception: {Message}. Retrying in {Seconds}s",
                        retryCount,
                        exception.Message,
                        timeSpan.TotalSeconds);
                });
    }

    /// <summary>
    /// Determines if an HTTP status code is retryable.
    /// </summary>
    private static bool _IsRetryableStatusCode(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.InternalServerError
            || statusCode == HttpStatusCode.BadGateway
            || statusCode == HttpStatusCode.ServiceUnavailable
            || statusCode == HttpStatusCode.GatewayTimeout
            || statusCode == HttpStatusCode.RequestTimeout;
    }
}