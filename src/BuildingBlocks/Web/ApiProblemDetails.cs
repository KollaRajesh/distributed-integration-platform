namespace DistributedIntegrationPlatform.BuildingBlocks.Web;

public static class ApiProblemDetails
{
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        });

        return services;
    }
}
