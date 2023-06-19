
using EK.Discord.Common.Base.Component.Access;
using Microsoft.AspNetCore.Mvc;

namespace EK.Discord.Server.Base.Component.Access; 

public abstract class AbstractControllerBase : ControllerBase, IController {

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
    protected AbstractControllerBase(IServiceProvider serviceProvider) {
        Logger = serviceProvider.GetService(typeof(ILogger<>).MakeGenericType(GetType())) as ILogger;
        ServiceProvider = serviceProvider;
    }
}