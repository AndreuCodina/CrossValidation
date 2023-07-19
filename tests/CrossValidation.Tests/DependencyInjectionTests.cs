using System.Net;
using System.Text.Json;
using CrossValidation.DependencyInjection;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.WebApplication;
using CrossValidation.WebApplication.Resources;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class DependencyInjectionTests :
    TestBase,
    IClassFixture<CommonFixture>,
    IDisposable
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
    public async Task Get_message_from_custom_resx(string languageCode, string expectedMessage)
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
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResource1.Hello));
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Theory]
    [InlineData("en")]
    [InlineData("es")]
    public async Task Not_get_message_from_custom_resx_or_built_in_resx_when_the_code_is_not_key_of_any_resx(
        string languageCode)
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
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.ErrorWithCodeWithoutResxKey);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe("RandomCode");
        error.Message.ShouldBeNull();
    }
    
    [Fact]
    public async Task Error_with_code_without_a_resx_key_have_null_message()
    {
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.ErrorWithCodeWithoutResxKey);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe("RandomCode");
        error.Message.ShouldBeNull();
    }
    
    [Theory]
    [InlineData("en", "Replaced NotNull")]
    [InlineData("es", "NotNull reemplazado")]
    public async Task Get_replaced_message_when_built_in_code_is_replaced_with_custom_resx(string languageCode, string expectedMessage)
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
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!.Count().ShouldBe(1);
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
                x.AddResx<ErrorResource1>());
        }).CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", "fr");
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.DefaultCultureMessage);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResource.Null));
        error.Message.ShouldBe(ErrorResource.Null);
    }
    
    [Fact]
    public async Task Get_error_status_code()
    {
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.ErrorWithStatusCode);
        
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors.ShouldBeNull();
    }
    
    [Fact]
    public async Task Handle_unknown_exception()
    {
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.Exception);
        
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task Get_message_in_default_culture_when_a_built_in_language_is_requested_but_not_customized()
    {
        _client.DefaultRequestHeaders.Add("Accept-Language", "es");
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.DefaultCultureMessage);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResource.Null));
        error.Message.ShouldBe(ErrorResource.Null);
    }
    
    [Fact]
    public async Task Get_message_in_requested_culture_when_a_built_in_language_is_requested_and_customized()
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
        
        _client.DefaultRequestHeaders.Add("Accept-Language", "es");
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.DefaultCultureMessage);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResource.Null));
        error.Message.ShouldBe("No debe tener un valor");
    }
    
    [Fact]
    public async Task Get_message_in_parent_culture_when_requested_culture_is_not_supported()
    {
        _client.DefaultRequestHeaders.Add("Accept-Language", "en-IE");
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.DefaultCultureMessage);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResource.Null));
        error.Message.ShouldBe(ErrorResource.Null);
    }
    
    [Theory]
    [InlineData("en", "Hello")]
    [InlineData("es", "Hola")]
    public async Task Get_message_from_resx_associated_culture(string languageCode, string expectedMessage)
    {
        _client = new TestApplicationFactory(services =>
        {
            services.AddCrossValidation(x =>
                x.AddResxAndAssociatedCultures<ErrorResource1>());
        }).CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.ErrorWithCodeFromCustomResx);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResource1.Hello));
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Theory]
    [InlineData("en", "Hello")]
    [InlineData("es", "Hola")]
    public async Task Get_message_from_resx_when_several_resx_and_associated_cultures_are_added(string languageCode, string expectedMessage)
    {
        _client = new TestApplicationFactory(services =>
        {
            services.AddCrossValidation(x =>
            {
                x.AddResxAndAssociatedCultures<ErrorResource1>();
                x.AddResxAndAssociatedCultures<ErrorResource2>();
            });
        }).CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.ErrorWithCodeFromCustomResx);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResource1.Hello));
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Theory]
    [InlineData("en", "The value is 3.3")]
    [InlineData("es", "El valor es 3,3")]
    public async Task Get_message_with_decimal(string languageCode, string expectedMessage)
    {
        _client = new TestApplicationFactory(services =>
        {
            services.AddCrossValidation(x =>
            {
                x.AddResxAndAssociatedCultures<ErrorResource1>();
            });
        }).CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.UseDecimal);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task Get_placeholders_from_FrontBusinessException()
    {
        var expectedPlaceholders = new Dictionary<string, object?>
        {
            {"placeholder1", 1},
            {"placeholder2", "value"}
        };
        _client = new TestApplicationFactory(services =>
        {
            services.AddCrossValidation(x =>
            {
                x.AddResxAndAssociatedCultures<ErrorResource1>();
            });
        }).CreateClient();
        
        var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.FrontBusinessExceptionWithPlaceholders);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Message.ShouldBe(null);
        error.Placeholders!.Count.ShouldBe(2);
        error.Placeholders!.ElementAt(0).Key.ShouldBe("placeholder1");
        error.Placeholders!.ElementAt(1).Key.ShouldBe("placeholder2");
    }

    public void Dispose()
    {
        _client.Dispose();
        CrossValidationOptions.SetDefaultOptions();
    }

    private async Task<CrossProblemDetails> GetProblemDetailsFromResponse(HttpResponseMessage response)
    {
        var contentStream = await response.Content.ReadAsStreamAsync();
        var problemDetails = await JsonSerializer.DeserializeAsync<CrossProblemDetails>(contentStream);
        return problemDetails!;
    }
}