using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CrossValidation.AspNetCore.IntegrationTests.TestUtils;

public class TestWebApplicationFactory(Action<IServiceCollection>? services = null)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(serviceCollection =>
            services?.Invoke(serviceCollection));
    }
}