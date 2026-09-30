using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DistributedIntegrationPlatform.BuildingBlocks.Identity;

public sealed class TenantContext(IHttpContextAccessor httpContextAccessor)
{
    public Guid? TenantId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue("tenant_id");
            return Guid.TryParse(value, out var tenantId) ? tenantId : null;
        }
    }

    public string? Subject => httpContextAccessor.HttpContext?.User.FindFirstValue("sub");
}
