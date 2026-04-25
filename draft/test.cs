public async Task InvokeAsync(HttpContext httpContext)
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        await _next(httpContext);
    }
    finally
    {
        var ms = stopwatch.ElapsedMilliseconds;
        var scopes = new List<KeyValuePair<String, String>>();
        scopes.Add(new KeyValuePair<String, String>(ScopedLoggingFields.Duration, $"{ms}"));
        scopes.Add(new KeyValuePair<String, String>(ScopedLoggingFields.RequestMethod, LogSecurityHelper.SanitizeLog($"{httpContext.Request.Method}")));
        scopes.Add(new KeyValuePair<String, String>(ScopedLoggingFields.RequestPath, LogSecurityHelper.SanitizeLog($"{httpContext.Request.Path}")));
        using (logger.CreateScope(scopes))
        {
            if (!httpContext.Request.Path.ToString().Contains("healthz"))
            {
                var method = LogSecurityHelper.SanitizeLog(httpContext.Request.Method);
                var path = LogSecurityHelper.SanitizeLog(httpContext.Request.Path.ToString());
                var queryString = LogSecurityHelper.SanitizeLog(httpContext.Request.QueryString.ToString());
                logger.Info($"{method} {path}{queryString} {httpContext.Response.StatusCode} in {ms}ms");
            }
        }
    }
}
