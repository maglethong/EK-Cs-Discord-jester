using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Ex.Discord.Server.Core.Test.Base;

/// <summary>
/// The implementation of <see cref="ITraitDiscoverer"/> which returns the trait values for <see cref="TraitAttribute"/>.
/// </summary>
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Discovered by reflection")]
public class CustomTraitDiscoverer : ITraitDiscoverer
{
    /// <inheritdoc/>
    public virtual IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        TestType ctorArg = traitAttribute.GetConstructorArguments()
                                         .Take(1)
                                         .Cast<TestType>()
                                         .First();
        yield return new KeyValuePair<string, string>("Category", ctorArg.ToString());
    }
}