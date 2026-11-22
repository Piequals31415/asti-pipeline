using System.Reflection;

namespace Asti.Cli.Tests;

public class SmokeTests
{
    [Fact]
    public void ModuleAssemblyLoads()
    {
        var assembly = Assembly.Load("Asti.Cli");
        Assert.Equal("Asti.Cli", assembly.GetName().Name);
    }
}
