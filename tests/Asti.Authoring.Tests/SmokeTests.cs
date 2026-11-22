using System.Reflection;

namespace Asti.Authoring.Tests;

public class SmokeTests
{
    [Fact]
    public void ModuleAssemblyLoads()
    {
        var assembly = Assembly.Load("Asti.Authoring");
        Assert.Equal("Asti.Authoring", assembly.GetName().Name);
    }
}
