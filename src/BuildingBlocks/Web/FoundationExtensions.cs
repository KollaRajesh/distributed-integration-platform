using DistributedIntegrationPlatform.BuildingBlocks.Configuration;
using DistributedIntegrationPlatform.BuildingBlocks.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Trace;

namespace DistributedIntegrationPlatform.BuildingBlocks.Web;

public static class FoundationExtensions
{
    public static IServiceCollection AddApiFoundation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .ValidateOnStart();
        services.AddOptions<ServiceOptions>()
            .Bind(configuration.GetSection(ServiceOptions.SectionName))
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddSingleton<TenantContext>();
        services.AddApiProblemDetails();
        services.AddHealthChecks();
        services.AddHttpClient("default").AddStandardResilienceHandler();
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var auth = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>();
                if (auth is null || string.IsNullOrWhiteSpace(auth.Authority) || string.IsNullOrWhiteSpace(auth.Audience))
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = false,
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                    return;
                }

                options.Authority = auth.Authority;
                options.Audience = auth.Audience;
                options.RequireHttpsMetadata = !configuration.GetValue<bool>("Authentication:AllowInsecureMetadata");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles"
                };
            });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.TenantRead, policy =>
                policy.RequireAuthenticatedUser().RequireClaim("tenant_id"));
            options.AddPolicy(PolicyNames.TenantWrite, policy =>
                policy.RequireAuthenticatedUser().RequireClaim("tenant_id")
                    .RequireRole("tenant-admin", "operator", "platform-admin"));
            options.AddPolicy(PolicyNames.PlatformAdmin, policy =>
                policy.RequireAuthenticatedUser().RequireRole("platform-admin"));
        });

        return services;
    }

    public static WebApplication UseApiFoundation(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHealthChecks("/health/live").AllowAnonymous();
        app.MapHealthChecks("/health/ready").AllowAnonymous();
        return app;
    }
}
