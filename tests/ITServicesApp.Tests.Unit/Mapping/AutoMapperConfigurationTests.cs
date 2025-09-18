// tests/ITServicesApp.Tests.Unit/Mapping/AutoMapperConfigurationTests.cs
using Xunit;
using ITServicesApp.Tests.Unit.TestHelpers;

public class AutoMapperConfigurationTests
{
    [Fact]
    public void AutoMapperConfiguration_IsValid()
    {
        _ = AutoMapperTestConfig.Create(validate: true);
    }
}
