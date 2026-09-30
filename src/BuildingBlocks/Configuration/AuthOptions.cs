namespace DistributedIntegrationPlatform.BuildingBlocks.Configuration;

public sealed class AuthOptions
{
    public const string SectionName = "Authentication";

    public required string Authority { get; init; }

    public required string Audience { get; init; }

    public string TenantClaim { get; init; } = "tenant_id";
}
