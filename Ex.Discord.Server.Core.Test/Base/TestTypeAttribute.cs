using Xunit.Sdk;

namespace Ex.Discord.Server.Core.Test.Base;

[TraitDiscoverer("Ex.Discord.Server.Core.Test.Base.CustomTraitDiscoverer", "Ex.Discord.Server.Core.Test")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class TestTypeAttribute : Attribute, ITraitAttribute {
    public TestTypeAttribute(TestType type, params string[] dependencies) {
    }
}