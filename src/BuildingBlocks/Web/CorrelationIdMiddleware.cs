namespace DistributedIntegrationPlatform.BuildingBlocks.Web;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        const string headerName = "X-Correlation-Id";
        var correlationId = context.Request.Headers[headerName].FirstOrDefault();
        if (!Guid.TryParse(correlationId, out _))
        {
            correlationId = Guid.NewGuid().ToString("D");
        }

        context.TraceIdentifier = correlationId;
        context.Response.Headers[headerName] = correlationId;
        await next(context);
    }
}
