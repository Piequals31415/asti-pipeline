using System.Reflection;

namespace Asti.Tiled.Tests;

public class SmokeTests
{
    [Fact]
    public void ModuleAssemblyLoads()
    {
        var assembly = Assembly.Load("Asti.Tiled");
        Assert.Equal("Asti.Tiled", assembly.GetName().Name);
    }
}
