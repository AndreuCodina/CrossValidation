using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CrossValidation.DependencyInjection;
using CrossValidation.Resources;
using CrossValidation.WebApplication.Resources;
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
    
    [Theory]
    [InlineData("/crossException", HttpStatusCode.UnprocessableEntity)]
    [InlineData("/validationListException", HttpStatusCode.UnprocessableEntity)]
    public async Task Get_http_status_code(string endpoint, HttpStatusCode expectedStatusCode)
    {
        var response = await _client.GetAsync(endpoint);
        
        response.StatusCode.ShouldBe(expectedStatusCode);
    }

    [Fact]
    public async Task Get_trace_id_sent()
    {
        var expectedTraceId = "Trace1";
        _client.DefaultRequestHeaders.Add("X-Trace-Id", expectedTraceId);
        
        var response = await _client.GetAsync("/crossException");

        response.Headers.TryGetValues("X-Trace-Id", out var headers);
        headers!.First().ShouldBe(expectedTraceId);
    }

    [Theory]
    [InlineData("en", "Hello")]
    [InlineData("es", "Hola")]
    public async Task Use_custom_resx(string languageCode, string expectedMessage)
    {
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync("/errorWithCodeFromCustomResx");
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await response.Content.ReadFromJsonAsync<CrossProblemDetails>();
        problemDetails!.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResource1.Hello));
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task Error_with_code_without_a_resx_key_have_null_message()
    {
        var response = await _client.GetAsync("/errorWithCodeWithoutResxKey");
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await response.Content.ReadFromJsonAsync<CrossProblemDetails>();
        problemDetails!.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe("RandomKey");
        error.Message.ShouldBeNull();
    }
    
    [Theory]
    [InlineData("en", "Replaced NotNull")]
    [InlineData("es", "NotNull reemplazado")]
    public async Task Replace_built_in_code_with_custom_resx(string languageCode, string expectedMessage)
    {
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync("/replaceBuiltInCodeWithCustomResx");
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await response.Content.ReadFromJsonAsync<CrossProblemDetails>();
        problemDetails!.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResource1.NotNull));
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task Get_default_culture_message_when_the_culture_provided_is_not_present()
    {
        _client.DefaultRequestHeaders.Add("Accept-Language", "fr");
        
        var response = await _client.GetAsync("/defaultCultureMessage");
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await response.Content.ReadFromJsonAsync<CrossProblemDetails>();
        problemDetails!.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResource.Null));
        error.Message.ShouldBe(ErrorResource.Null);
    }
    
    [Fact]
    public async Task Get_error_status_code()
    {
        var response = await _client.GetAsync("/errorWithStatusCode");
        
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var problemDetails = await response.Content.ReadFromJsonAsync<CrossProblemDetails>();
        problemDetails!.Errors.ShouldBeNull();
    }
}