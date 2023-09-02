
using EK.Discord.Common.Base.Component.Common;

namespace EK.Discord.Common.Base.Component.Persistence; 

/// <summary>
///     Base class for all component repositories providing some common utility
/// </summary>
public class AbstractRepositoryBase : AbstractComponentPartBase, IRepository {

    /// <inheritdoc cref="AbstractComponentPartBase"/>
    public AbstractRepositoryBase(IServiceProvider serviceProvider) : base(serviceProvider) { }

}