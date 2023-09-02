using EK.Discord.Common.Base.Component.Api;
using EK.Discord.Common.Base.Component.Common;

namespace EK.Discord.Common.Base.Component.Business; 

public abstract class AbstractServiceBase : AbstractComponentPartBase, IService {

    /// <inheritdoc cref="AbstractComponentPartBase"/>
    protected AbstractServiceBase(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

}