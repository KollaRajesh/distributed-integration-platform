using DistributedIntegrationPlatform.BuildingBlocks.Identity;
using DistributedIntegrationPlatform.BuildingBlocks.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiFoundation(builder.Configuration);

var app = builder.Build();
app.UseApiFoundation();

app.MapGet("/v1/customers/foundation", () => Results.Ok(new { service = "customer-api" }))
    .RequireAuthorization(PolicyNames.TenantRead);

app.Run();

public partial class Program;
