﻿using System.Net;
using System.Text.Json;
using CrossValidation.DependencyInjection;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.WebApplication;
using CrossValidation.WebApplication.Resources;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;
using Environment = CrossValidation.Tests.TestUtils.Environment;

namespace CrossValidation.Tests;

public class DependencyInjectionTests :
    TestBase,
    IClassFixture<CommonFixture>,
    IDisposable
{
    private WebApplicationFactory<Program> _testWebApplicationFactory;
    private HttpClient _client;

    public DependencyInjectionTests()
    {
        _testWebApplicationFactory = new TestWebApplicationFactory();
        _client = _testWebApplicationFactory.CreateClient();
    }
    
    [Theory]
    [InlineData(EndpointPath.Test.Prefix + EndpointPath.Test.BusinessException, HttpStatusCode.UnprocessableEntity)]
    [InlineData(EndpointPath.Test.Prefix + EndpointPath.Test.BusinessListException, HttpStatusCode.UnprocessableEntity)]
    public async Task Get_http_status_code(string endpoint, HttpStatusCode expectedStatusCode)
    {
        var response = await _client.GetAsync(endpoint);
        
        response.StatusCode.ShouldBe(expectedStatusCode);
    }
    
    [Theory]
    [InlineData("en", "Hello")]
    [InlineData("es", "Hola")]
    public async Task Get_message_from_custom_resx(string languageCode, string expectedMessage)
    {
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.SetDefaultCulture("en");
                options.SetSupportedCultures("en", "es");
                options.AddResx<ErrorResource1>();
            });
        });
        _client = _testWebApplicationFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessExceptionWithCodeFromCustomResx);
        
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
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.SetDefaultCulture("en");
                options.SetSupportedCultures("en", "es");
                options.AddResx<ErrorResource1>();
            });
        });
        _client = _testWebApplicationFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ExceptionWithCodeWithoutResxKey);
        
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
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ExceptionWithCodeWithoutResxKey);
        
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
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.SetDefaultCulture("en");
                options.SetSupportedCultures("en", "es");
                options.AddResx<ErrorResource1>();
            });
        });
        _client = _testWebApplicationFactory.CreateClient();
        
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ReplaceBuiltInCodeWithCustomResx);
        
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
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
                options.AddResx<ErrorResource1>());
        });
        _client = _testWebApplicationFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", "fr");
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.DefaultCultureMessage);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!.Count().ShouldBe(1);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResource.Null));
        error.Message.ShouldBe(ErrorResource.Null);
    }
    
    [Fact]
    public async Task Get_exception_status_code()
    {
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.BusinessExceptionWithStatusCodeWithMapping);
        
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors.ShouldBeNull();
    }
    
    [Fact]
    public async Task Get_message_in_default_culture_when_a_built_in_language_is_requested_but_not_customized()
    {
        _client.DefaultRequestHeaders.Add("Accept-Language", "es");
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.DefaultCultureMessage);
        
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
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options => options
                .SetDefaultCulture("en")
                .SetSupportedCultures("en", "es")
                .AddResx<ErrorResource1>());
        });
        _client = _testWebApplicationFactory.CreateClient();
        
        _client.DefaultRequestHeaders.Add("Accept-Language", "es");
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.DefaultCultureMessage);
        
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
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.DefaultCultureMessage);
        
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
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
                options.AddResxAndAssociatedCultures<ErrorResource1>());
        });
        _client = _testWebApplicationFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessExceptionWithCodeFromCustomResx);
        
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
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.AddResxAndAssociatedCultures<ErrorResource1>();
                options.AddResxAndAssociatedCultures<ErrorResource2>();
            });
        });
        _client = _testWebApplicationFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessExceptionWithCodeFromCustomResx);
        
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
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.AddResxAndAssociatedCultures<ErrorResource1>();
            });
        });
        _client = _testWebApplicationFactory.CreateClient();
        _client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.UseDecimal);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public async Task Get_placeholders_from_FrontBusinessException()
    {
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.AddResxAndAssociatedCultures<ErrorResource1>();
            });
        });
        _client = _testWebApplicationFactory.CreateClient();
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.FrontBusinessExceptionWithPlaceholders);
        
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Message.ShouldBe(null);
        error.Placeholders!.Count.ShouldBe(4);
        error.Placeholders!.ElementAt(0).Key.ShouldBe("placeholder1");
        error.Placeholders!.ElementAt(1).Key.ShouldBe("placeholder2");
        error.Placeholders!.ElementAt(2).Key.ShouldBe("placeholder3");
        error.Placeholders!.ElementAt(3).Key.ShouldBe("placeholder4");
    }
    
    [Fact]
    public async Task Get_code_from_ResxBusinessException()
    {
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.AddResxAndAssociatedCultures<ErrorResource1>();
            });
        });
        _client = _testWebApplicationFactory.CreateClient();
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessException);
        
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResourceWithNoResx.Key));
        error.Message.ShouldBe(ErrorResourceWithNoResx.Key);
    }
    
    [Fact(Skip = "TODO")]
    public async Task Get_code_url_when_publication_url_is_specified()
    {
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.EnableErrorCodePage(publicationUrl: "https://www.backend.com");
            });
        });
        _client = _testWebApplicationFactory.CreateClient();
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessException);
        
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResourceWithNoResx.Key));
        error.CodeUrl.ShouldBe($"https://www.backend.com{CrossValidationOptions.ErrorCodePagePath}#Key");
    }
    
    [Fact(Skip = "TODO")]
    public async Task Get_code_url_when_publication_url_is_specified_and_environment_is_development()
    {
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options => options
                .EnableErrorCodePage());
        });
        _client = _testWebApplicationFactory.CreateClient();
        
        var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessException);
        
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Code.ShouldBe(nameof(ErrorResourceWithNoResx.Key));
        error.CodeUrl.ShouldBe($"http://localhost{CrossValidationOptions.ErrorCodePagePath}#Key");
    }
    
    [Theory(Skip = "TODO")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_error_code_page_when_it_is_enabled(bool isEnabled)
    {
        _testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                if (isEnabled)
                {
                    options.EnableErrorCodePage();
                }
            });
        });
        _client = _testWebApplicationFactory.CreateClient();
        
        var response = await _client.GetAsync(CrossValidationOptions.ErrorCodePagePath);

        var body = await response.Content.ReadAsStringAsync();

        AssertGet_error_code_page_when_it_is_enabled(isEnabled, body);
    }
    
    [Theory]
    [InlineData(
        EndpointPath.Test.ErrorStatusCodeWithEmptyBody,
        (int)HttpStatusCode.NotFound,
        "Not Found",
        "https://tools.ietf.org/html/rfc9110#section-15.5.5")]
    [InlineData(
        EndpointPath.Test.UnexpectedException,
        (int)HttpStatusCode.InternalServerError,
        "An error occurred while processing your request.",
        "https://tools.ietf.org/html/rfc9110#section-15.6.1")]
    [InlineData(
        EndpointPath.Test.BusinessException,
        (int)HttpStatusCode.UnprocessableEntity,
        "Unprocessable Entity",
        "https://tools.ietf.org/html/rfc4918#section-11.2")]
    [InlineData(
        EndpointPath.Test.BusinessExceptionWithStatusCodeWithMapping,
        (int)HttpStatusCode.Created,
        "Created",
        null)]
    [InlineData(
        EndpointPath.Test.BusinessExceptionWithStatusCodeWithNoMapping,
        450,
        null,
        null)]
    [InlineData(
        EndpointPath.Test.BusinessExceptionWithErrorStatusCode,
        (int)HttpStatusCode.GatewayTimeout,
        "Gateway Timeout",
        "https://tools.ietf.org/html/rfc9110#section-15.6.5")]
    [InlineData(
        EndpointPath.Test.BusinessListException,
        (int)HttpStatusCode.UnprocessableEntity,
        "Unprocessable Entity",
        "https://tools.ietf.org/html/rfc4918#section-11.2")]
    public async Task Get_problem_details_with_status_details_when_return_error_status_code(
        string endpointPath,
        int expectedStatusCode,
        string? expectedTitle,
        string? expectedType)
    {
        async Task Test(string environment)
        {
            _testWebApplicationFactory = new TestWebApplicationFactory(services =>
            {
                services.AddCrossValidation(options => options
                    .EnableErrorCodePage());
            }).WithWebHostBuilder(builder => builder.UseEnvironment(environment));
            _client = _testWebApplicationFactory.CreateClient();
        
            var response = await _client.GetAsync(EndpointPath.Test.Prefix + endpointPath);
        
            var problemDetails = await GetProblemDetailsFromResponse(response);
            problemDetails.Status
                .Should()
                .Be(expectedStatusCode);
            problemDetails.Title
                .Should()
                .Be(expectedTitle);
            problemDetails.Type
                .Should()
                .Be(expectedType);
        }

        await Test(Environment.Development);
        await Test(Environment.Production);
    }
    
    // TODO 1: In Development with unexpected exception, return detailed body
    // TODO 2: In Development with business exception, return a custom body, but with status details added
    
    // [Fact]
    // public async Task Get_problem_details_with_unexpected_exception()
    // {
    //     var response = await _client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ExceptionWithCodeWithoutResxKey);
    //     
    //     var problemDetails = await GetProblemDetailsFromResponse(response);
    //     problemDetails.Status
    //         .Should()
    //         .Be((int)HttpStatusCode.InternalServerError);
    //     problemDetails.Title
    //         .Should()
    //         .Be("System.Exception");
    //     problemDetails.Type
    //         .Should()
    //         .NotBeNullOrEmpty();
    // }
    
    // [Fact]
    // public async Task Handle_unknown_exception()
    // {
    //     var response = await _client.GetAsync(ApiPath.Test.Prefix + ApiPath.Test.Exception);
    //     
    //     response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    //     var problemDetails = await GetProblemDetailsFromResponse(response);
    //     problemDetails.ShouldNotBeNull();
    // }

    public void Dispose()
    {
        _client.Dispose();
        CrossValidationOptions.SetDefaultOptions();
    }

    private async Task<CrossProblemDetails> GetProblemDetailsFromResponse(HttpResponseMessage response)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var contentStream = await response.Content.ReadAsStreamAsync();
        var problemDetails = await JsonSerializer.DeserializeAsync<CrossProblemDetails>(contentStream, jsonSerializerOptions);
        return problemDetails!;
    }
    
    private void AssertGet_error_code_page_when_it_is_enabled(bool isEnabled, string body)
    {
        if (isEnabled)
        {
            body.ShouldNotBeEmpty();
        }
        else
        {
            body.ShouldBeEmpty();
        }
    }

    private static IEnumerable<object[]> Data()
    {
        return new[]
        {
            new object[] { 1, 2, 3 },
        };
    }
}