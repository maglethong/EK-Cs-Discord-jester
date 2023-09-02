using Ex.Discord.Server.Core.Test.Azure.Persistence;

namespace Ex.Discord.Server.Core.Test.Base;

/// <summary>
///     Defines the different types of tests and their corresponding recommended base classes to be used. 
/// </summary>
public enum TestType {
    /// <summary>
    ///     Tests a single class in isolation of remaining code of the project.
    /// <p/>
    ///     This is the default type. If no <see cref="TestType"/> is defined, this one is assumed.
    /// </summary>
    /// <remarks>
    ///     Inherit <see cref="AbstractUnitTest"/> for some utility in unit testing.
    /// </remarks>
    UnitTest = 0,
    
    /// <summary>
    ///     Marks a test as being a smoke test.
    /// </summary>
    /// <remarks>
    ///     Tests marked as SmokeTest will not run on the pipeline.
    /// </remarks>
    SmokeTest = 1,
    
    /// <summary>
    ///     Marks a test as being a developer test.
    /// <p/>
    ///     Developer tests usually require special setup before being able to run. This should be properly documented.
    /// </summary>
    /// <remarks>
    ///     Tests marked as SmokeTest will not run on the pipeline.
    /// </remarks>
    DeveloperTest = 2,
    
    /// <summary>
    ///     Tests business logic of a component, using all it's internal classes, yet still isolated from other components.
    /// </summary>
    /// <remarks>
    ///     Inherit <see cref="AbstractComponentTest"/> for some utility in component testing.
    /// </remarks>
    ComponentTest = 10,
    
    /// <summary>
    ///     Tests communication between a specific component and one or more of its dependencies.
    ///     Dependencies might be other components or external applications
    /// </summary>
    /// <remarks>
    ///     Inherit <see cref="AbstractComponentTest"/> for some utility in component testing.
    /// </remarks>
    IntegrationTest = 20,
}