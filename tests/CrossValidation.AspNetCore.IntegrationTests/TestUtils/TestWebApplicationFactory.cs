using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CrossValidation.AspNetCore.IntegrationTests.TestUtils;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Action<IServiceCollection>? _services;

    public TestWebApplicationFactory(Action<IServiceCollection>? services = null)
    {
        _services = services;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            _services?.Invoke(services);
        });
    }
}