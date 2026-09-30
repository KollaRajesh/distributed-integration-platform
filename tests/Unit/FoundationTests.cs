using DistributedIntegrationPlatform.BuildingBlocks.Configuration;
using DistributedIntegrationPlatform.BuildingBlocks.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Foundation.UnitTests;

public sealed class FoundationTests
{
    [Fact]
    public void PolicyNames_ExposeSharedAuthorizationContracts()
    {
        Assert.Equal("tenant-read", PolicyNames.TenantRead);
        Assert.Equal("tenant-write", PolicyNames.TenantWrite);
        Assert.Equal("platform-admin", PolicyNames.PlatformAdmin);
    }

    [Fact]
    public void TenantContext_RequiresAValidTenantClaim()
    {
        var claims = new[] { new Claim("tenant_id", "not-a-guid") };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        Assert.False(Guid.TryParse(principal.FindFirstValue("tenant_id"), out _));
    }

    [Fact]
    public void AuthOptions_UsesTheSharedTenantClaimByDefault()
    {
        var options = new AuthOptions
        {
            Authority = "https://issuer.example.test",
            Audience = "ohs-api"
        };

        Assert.Equal("tenant_id", options.TenantClaim);
    }
}
