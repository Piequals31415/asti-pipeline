using System.Reflection;

namespace Asti.Conventions.Tests;

public class SmokeTests
{
    [Fact]
    public void ModuleAssemblyLoads()
    {
        var assembly = Assembly.Load("Asti.Conventions");
        Assert.Equal("Asti.Conventions", assembly.GetName().Name);
    }
}
