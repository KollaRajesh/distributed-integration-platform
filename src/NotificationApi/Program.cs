using DistributedIntegrationPlatform.BuildingBlocks.Identity;
using DistributedIntegrationPlatform.BuildingBlocks.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiFoundation(builder.Configuration);

var app = builder.Build();
app.UseApiFoundation();

app.MapGet("/v1/notifications/foundation", () => Results.Ok(new { service = "notification-api" }))
    .RequireAuthorization(PolicyNames.TenantRead);

app.Run();

public partial class Program;
