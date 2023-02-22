using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CrossValidation.DependencyInjection;
using CrossValidation.Resources;
using CrossValidation.WebApplication;
using CrossValidation.WebApplication.Resources;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

[CollectionDefinition("DependencyInjectionTests", DisableParallelization = true)]
public class DependencyInjectionTests : IDisposable
{
    private HttpClient _client;

    public DependencyInjectionTests()
    {
        var factory = new TestApplicationFactory();
        _client = factory.CreateClient();
    }
    
    [Theory]
    [InlineData(ApiPath.Test.Prefix + ApiPath.Test.CrossException, HttpStatusCode.UnprocessableEntity)]
    [InlineData(ApiPath.Test.Prefix + ApiPath.Test.ValidationListException, HttpStatusCode.UnprocessableEntity)]
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
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.CrossException);
    
        response.Headers.TryGetValues("X-Trace-Id", out var headers);
        headers!.First().ShouldBe(expectedTraceId);
    }
    
    [Theory]
    [InlineData("en", "Hello")]
    [InlineData("es", "Hola")]
    public async Task Use_custom_resx(string languageCode, string expectedMessage)
    {
        _client = new TestApplicationFactory(services =>
        {
            services.AddCrossValidation(x =>
            {
                x.SetDefaultCulture("en");
                x.SetSupportedCultures("en", "es");
                x.AddResx<ErrorResource1>();
            });
        }).CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.ErrorWithCodeFromCustomResx);
        
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
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.ErrorWithCodeWithoutResxKey);
        
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
        _client = new TestApplicationFactory(services =>
        {
            services.AddCrossValidation(x =>
            {
                x.SetDefaultCulture("en");
                x.SetSupportedCultures("en", "es");
                x.AddResx<ErrorResource1>();
            });
        }).CreateClient();
        
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.ReplaceBuiltInCodeWithCustomResx);
        
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
        _client = new TestApplicationFactory(services =>
        {
            services.AddCrossValidation(x =>
            {
                x.AddResx<ErrorResource1>();
            });
        }).CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", "fr");
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.DefaultCultureMessage);
        
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
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.ErrorWithStatusCode);
        
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var problemDetails = await response.Content.ReadFromJsonAsync<CrossProblemDetails>();
        problemDetails!.Errors.ShouldBeNull();
    }
    
    [Fact]
    public async Task Handle_unknown_exception()
    {
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.Exception);
        
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        var problemDetails = await response.Content.ReadFromJsonAsync<CrossProblemDetails>();
        problemDetails.ShouldNotBeNull();
    }

    public void Dispose()
    {
        _client.Dispose();
        CrossValidationOptions.SetDefaultOptions();
    }
}