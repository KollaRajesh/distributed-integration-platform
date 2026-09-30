namespace DistributedIntegrationPlatform.BuildingBlocks.Configuration;

public sealed class ServiceOptions
{
    public const string SectionName = "Service";

    public required string Name { get; init; }
}
