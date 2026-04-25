namespace BaseCleanArchitecture.Application.Common.Resilience;

using Polly;
using Polly.Retry;


public static class RetryPolicy
{
    private static readonly ILogger logger = LoggerFactory.Get(typeof(RetryPolicy));

    public static readonly AsyncRetryPolicy<HttpResponseMessage> HttpRetryPolicy = Policy
        .HandleResult<HttpResponseMessage>(response =>
            !response.IsSuccessStatusCode && IsRetryableStatusCode(response.StatusCode)
        )
        .Or<HttpRequestException>()
        .Or<TaskCanceledException>()
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (result, timeSpan, retryCount, context) =>
            {
                if (result.Exception != null)
                {
                    logger.Warn($"Retry {retryCount} due to exception: {result.Exception.Message}");
                }
                else
                {
                    logger.Warn(
                        $"Retry {retryCount} due to retryable HTTP status: {result.Result.StatusCode}"
                    );
                }
            }
        );

    public static readonly AsyncRetryPolicy GeneralRetryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, timeSpan, retryCount, context) =>
            {
                logger.Warn(
                    $"Retry {retryCount} due to exception: {exception.Message}. Retrying in {timeSpan.TotalSeconds} seconds..."
                );
            }
        );

    private static Boolean IsRetryableStatusCode(System.Net.HttpStatusCode statusCode)
    {
        return statusCode == System.Net.HttpStatusCode.InternalServerError
            || statusCode == System.Net.HttpStatusCode.BadGateway
            || statusCode == System.Net.HttpStatusCode.ServiceUnavailable
            || statusCode == System.Net.HttpStatusCode.GatewayTimeout
            || statusCode == System.Net.HttpStatusCode.RequestTimeout;
    }
}