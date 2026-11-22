using System.Reflection;

namespace Asti.Aseprite.Tests;

public class SmokeTests
{
    [Fact]
    public void ModuleAssemblyLoads()
    {
        var assembly = Assembly.Load("Asti.Aseprite");
        Assert.Equal("Asti.Aseprite", assembly.GetName().Name);
    }
}
