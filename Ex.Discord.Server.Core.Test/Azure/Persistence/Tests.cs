using Ex.Discord.Server.Core.Test.Base;
using Xunit;

namespace Ex.Discord.Server.Core.Test.Azure.Persistence; 

[TestType(TestType.IntegrationTest, AZURITE_DOCKER_CONTAINER_DEPENDENCY)]
public class SinglePartitionAzureTableStorageDaoUnitTest : AbstractIntegrationTest {

    /// <summary>
    ///      Used by pipeline to boot up docker container required for running this test.
    /// <p/>
    ///     For running locally, run the following:
    ///     <code>
    ///         docker run 
    ///     </code>
    ///     TODO: Make DockerCompose
    /// </summary>
    public const string AZURITE_DOCKER_CONTAINER_DEPENDENCY = "AzuriteDockerContainer";

    [TestType(TestType.SmokeTest)]
    [Fact]
    public void Test1() {
        Assert.True(true);
    }

}

public abstract class AbstractUnitTest {
    
}

public abstract class AbstractComponentTest : AbstractUnitTest {
    
}

/// <summary>
/// TODO
/// </summary>
/// <remarks>
///     Should the test be unable to run in parallel with other tests, mark them both with the same <see cref="CollectionAttribute"/>
///     <code>
///         // Example:
///         [TestType(TestType.IntegrationTest, AZURITE_DOCKER_CONTAINER_DEPENDENCY)]
///         [Collection(AZURITE_DOCKER_CONTAINER_DEPENDENCY)]
///         public class SinglePartitionAzureTableStorageDaoUnitTest : AbstractIntegrationTest {
///         // ...
///         // Will not rin in parallel with another test class marked with [Collection(AZURITE_DOCKER_CONTAINER_DEPENDENCY)]
///     </code>
/// </remarks>
public abstract class AbstractIntegrationTest : AbstractComponentTest {
    
}