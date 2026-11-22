using System.Reflection;

namespace Asti.Pipeline.Tests;

public class SmokeTests
{
    [Fact]
    public void ModuleAssemblyLoads()
    {
        var assembly = Assembly.Load("Asti.Pipeline");
        Assert.Equal("Asti.Pipeline", assembly.GetName().Name);
    }
}
