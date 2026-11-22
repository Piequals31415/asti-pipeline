using System.Reflection;

namespace Asti.World.Tests;

public class SmokeTests
{
    [Fact]
    public void ModuleAssemblyLoads()
    {
        var assembly = Assembly.Load("Asti.World");
        Assert.Equal("Asti.World", assembly.GetName().Name);
    }
}
