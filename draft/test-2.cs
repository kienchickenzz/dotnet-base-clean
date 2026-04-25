using Common;
using FluentAssertions;
using System.Net;
using Xunit;

namespace DAL.UnitTests.Infrastructure;

public class CommonRetryPolicyTests
{
    // ---------------------------------------------------------------------------
    // HttpRetryPolicy
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task HttpRetryPolicy_ShouldSucceed_OnFirstAttempt()
    {
        var callCount = 0;

        var response = await CommonRetryPolicy.HttpRetryPolicy.ExecuteAsync(() =&gt;
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        callCount.Should().Be(1);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    [InlineData(HttpStatusCode.RequestTimeout)]
    public async Task HttpRetryPolicy_ShouldRetryAndSucceed_OnRetryableStatusCode(HttpStatusCode retryableStatus)
    {
        var callCount = 0;

        var response = await CommonRetryPolicy.HttpRetryPolicy.ExecuteAsync(() =&gt;
        {
            callCount++;
            return callCount &lt; 3
                ? Task.FromResult(new HttpResponseMessage(retryableStatus))
                : Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        callCount.Should().Be(3);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    [InlineData(HttpStatusCode.RequestTimeout)]
    public async Task HttpRetryPolicy_ShouldExhaustRetries_WhenAlwaysRetryableStatusCode(HttpStatusCode retryableStatus)
    {
        var callCount = 0;

        var response = await CommonRetryPolicy.HttpRetryPolicy.ExecuteAsync(() =&gt;
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(retryableStatus));
        });

        // Polly returns the last response after exhausting retries (does not throw)
        response.StatusCode.Should().Be(retryableStatus);
        callCount.Should().Be(4); // 1 original + 3 retries
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task HttpRetryPolicy_ShouldNotRetry_OnNonRetryableStatusCode(HttpStatusCode nonRetryableStatus)
    {
        var callCount = 0;

        var response = await CommonRetryPolicy.HttpRetryPolicy.ExecuteAsync(() =&gt;
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(nonRetryableStatus));
        });

        response.StatusCode.Should().Be(nonRetryableStatus);
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task HttpRetryPolicy_ShouldRetryAndSucceed_OnHttpRequestException()
    {
        var callCount = 0;

        var response = await CommonRetryPolicy.HttpRetryPolicy.ExecuteAsync(() =&gt;
        {
            callCount++;
            if (callCount &lt; 3)
                throw new HttpRequestException("Simulated network error");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task HttpRetryPolicy_ShouldThrow_WhenHttpRequestExceptionPersists()
    {
        var callCount = 0;

        var act = async () =&gt;
        {
            await CommonRetryPolicy.HttpRetryPolicy.ExecuteAsync(() =&gt;
            {
                callCount++;
                throw new HttpRequestException("Persistent network error");
#pragma warning disable CS0162
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
#pragma warning restore CS0162
            });
        };

        await act.Should().ThrowAsync&lt;HttpRequestException&gt;().WithMessage("Persistent network error");
        callCount.Should().Be(4); // 1 original + 3 retries
    }

    [Fact]
    public async Task HttpRetryPolicy_ShouldRetryAndSucceed_OnTaskCanceledException()
    {
        var callCount = 0;

        var response = await CommonRetryPolicy.HttpRetryPolicy.ExecuteAsync(() =&gt;
        {
            callCount++;
            if (callCount &lt; 2)
                throw new TaskCanceledException("Simulated timeout");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        callCount.Should().Be(2);
    }

    // ---------------------------------------------------------------------------
    // GeneralRetryPolicy
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GeneralRetryPolicy_ShouldSucceed_OnFirstAttempt()
    {
        var callCount = 0;

        await CommonRetryPolicy.GeneralRetryPolicy.ExecuteAsync(() =&gt;
        {
            callCount++;
            return Task.CompletedTask;
        });

        callCount.Should().Be(1);
    }

    [Fact]
    public async Task GeneralRetryPolicy_ShouldRetryAndSucceed_WhenExceptionThenSuccess()
    {
        var callCount = 0;

        await CommonRetryPolicy.GeneralRetryPolicy.ExecuteAsync(() =&gt;
        {
            callCount++;
            if (callCount &lt; 3)
                throw new InvalidOperationException("Transient error");
            return Task.CompletedTask;
        });

        callCount.Should().Be(3);
    }

    [Fact]
    public async Task GeneralRetryPolicy_ShouldThrow_WhenExceptionPersists()
    {
        var callCount = 0;

        var act = async () =&gt;
        {
            await CommonRetryPolicy.GeneralRetryPolicy.ExecuteAsync(() =&gt;
            {
                callCount++;
                throw new InvalidOperationException("Persistent error");
#pragma warning disable CS0162
                return Task.CompletedTask;
#pragma warning restore CS0162
            });
        };

        await act.Should().ThrowAsync&lt;InvalidOperationException&gt;().WithMessage("Persistent error");
        callCount.Should().Be(4); // 1 original + 3 retries
    }
}