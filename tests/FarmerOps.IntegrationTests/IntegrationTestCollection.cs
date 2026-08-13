namespace FarmerOps.IntegrationTests;

/// <summary>Shares one SQL Server container across every integration test class instead of one per class.</summary>
[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>;
