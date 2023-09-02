using Microsoft.Extensions.Logging;

namespace EK.Discord.Common.Base.Component.Common;

/// <summary>
///     Base class for all component types providing some common utility
/// </summary>
public abstract class AbstractComponentPartBase : IComponentPart {

    /// <summary>
    ///     An <see cref="ILogger"/> instance for logging in the <see cref="ILogger"/> context.
    /// </summary>
    protected ILogger? Logger { get; }
    
    /// <summary>
    ///     The <see cref="ServiceProvider"/> instance for fetching Services configured through the Dependency Injection framework (Service Locator Pattern).
    /// </summary>
    /// <remarks> When possible, it is preferred to use the Dependency Injection Pattern, instead of the Service Locator Pattern </remarks>
    protected IServiceProvider ServiceProvider { get; }
    
    /// <summary>
    ///     Constructor
    /// </summary>
    protected AbstractComponentPartBase(IServiceProvider serviceProvider) {
        var type = GetType();
        var loggertype = typeof(ILogger<>).MakeGenericType(GetType());
        Logger = serviceProvider.GetService(typeof(ILogger<>).MakeGenericType(GetType())) as ILogger;
        ServiceProvider = serviceProvider;
    }
}