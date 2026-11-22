using System.Reflection;

namespace Asti.Tests;

public class SmokeTests
{
    [Fact]
    public void ModuleAssemblyLoads()
    {
        var assembly = Assembly.Load("Asti");
        Assert.Equal("Asti", assembly.GetName().Name);
    }
}
