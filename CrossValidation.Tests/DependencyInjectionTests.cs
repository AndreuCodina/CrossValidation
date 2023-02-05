using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class DependencyInjectionTests
{
    private readonly HttpClient _client;

    public DependencyInjectionTests()
    {
        var factory = new WebApplicationFactory<Program>();
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Get_http_status_code()
    {
        var response = await _client.GetAsync("/validationException");
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Get_trace_id_sent()
    {
        var expectedTraceId = "Trace1";
        _client.DefaultRequestHeaders.Add("X-Trace-Id", expectedTraceId);
        
        var response = await _client.GetAsync("/validationException");

        response.Headers.TryGetValues("X-Trace-Id", out var headers);
        headers!.First().ShouldBe(expectedTraceId);
    }
}