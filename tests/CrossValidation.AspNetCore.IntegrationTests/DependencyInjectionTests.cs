using System.Net;
using System.Text.Json;
using Common.Tests;
using CrossValidation.AspNetCore.IntegrationTests.TestUtils;
using CrossValidation.Resources;
using CrossValidation.WebApplication;
using CrossValidation.WebApplication.Resources;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;

namespace CrossValidation.AspNetCore.IntegrationTests;

public class DependencyInjectionTests : TestBase
{
    [Theory]
    [InlineData(EndpointPath.Test.Prefix + EndpointPath.Test.BusinessException, HttpStatusCode.UnprocessableEntity)]
    [InlineData(EndpointPath.Test.Prefix + EndpointPath.Test.BusinessListException, HttpStatusCode.UnprocessableEntity)]
    public async Task Get_http_status_code(string endpoint, HttpStatusCode expectedStatusCode)
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services => services
            .AddCrossValidation());
        var client = testWebApplicationFactory.CreateClient();
        var response = await client.GetAsync(endpoint);
        
        response.StatusCode
            .Should()
            .Be(expectedStatusCode);
    }
    
    [Theory]
    [InlineData("en", "Hello")]
    [InlineData("es", "Hola")]
    public async Task Get_message_from_custom_resx(string languageCode, string expectedMessage)
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.SetDefaultCulture("en");
                options.SetSupportedCultures("en", "es");
                options.AddResxWithoutAddingAssociatedCultures<ErrorResource1>();
            });
        });
        var client = testWebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessExceptionWithCodeFromCustomResx);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!
            .Should()
            .HaveCount(1);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be(nameof(ErrorResource1.Hello));
        error.Message
            .Should()
            .Be(expectedMessage);
    }
    
    [Theory]
    [InlineData("en")]
    [InlineData("es")]
    public async Task Not_get_message_from_custom_resx_or_built_in_resx_when_the_code_is_not_key_of_any_resx(
        string languageCode)
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.SetDefaultCulture("en");
                options.SetSupportedCultures("en", "es");
                options.AddResxWithoutAddingAssociatedCultures<ErrorResource1>();
            });
        });
        var client = testWebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ExceptionWithCodeWithoutResxKey);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!
            .Should()
            .HaveCount(1);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be("RandomCode");
        error.Message
            .Should()
            .BeNull();
    }
    
    [Fact]
    public async Task Error_with_code_without_a_resx_key_have_null_message()
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services => services
            .AddCrossValidation());
        var client = testWebApplicationFactory.CreateClient();
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ExceptionWithCodeWithoutResxKey);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!
            .Should()
            .HaveCount(1);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be("RandomCode");
        error.Message
            .Should()
            .BeNull();
    }
    
    [Theory]
    [InlineData("en", "Replaced NotNull")]
    [InlineData("es", "NotNull reemplazado")]
    public async Task Get_replaced_message_when_built_in_code_is_replaced_with_custom_resx(string languageCode, string expectedMessage)
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.SetDefaultCulture("en");
                options.SetSupportedCultures("en", "es");
                options.AddResxWithoutAddingAssociatedCultures<ErrorResource1>();
            });
        });
        var client = testWebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ReplaceBuiltInCodeWithCustomResx);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!
            .Should()
            .HaveCount(1);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be(nameof(ErrorResource1.NotNull));
        error.Message
            .Should()
            .Be(expectedMessage);
    }
    
    [Fact]
    public async Task Get_default_culture_message_when_the_culture_provided_is_not_present()
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
                options.AddResxWithoutAddingAssociatedCultures<ErrorResource1>());
        });
        var client = testWebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", "fr");
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.DefaultCultureMessage);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!
            .Should()
            .HaveCount(1);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be(nameof(ErrorResource.Null));
        error.Message
            .Should()
            .Be(ErrorResource.Null);
    }
    
    [Fact]
    public async Task Get_exception_status_code()
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services => services
            .AddCrossValidation());
        var client = testWebApplicationFactory.CreateClient();
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.BusinessExceptionWithStatusCodeWithMapping);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors
            .Should()
            .BeNull();
    }
    
    [Fact]
    public async Task Get_message_in_default_culture_when_a_built_in_language_is_requested_but_not_customized()
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services => services
            .AddCrossValidation());
        var client = testWebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", "es");
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.DefaultCultureMessage);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!
            .Should()
            .HaveCount(1);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be(nameof(ErrorResource.Null));
        error.Message
            .Should()
            .Be(ErrorResource.Null);
    }
    
    [Fact]
    public async Task Get_message_in_requested_culture_when_a_built_in_language_is_requested_and_customized()
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options => options
                .SetDefaultCulture("en")
                .SetSupportedCultures("en", "es")
                .AddResxWithoutAddingAssociatedCultures<ErrorResource1>());
        });
        var client = testWebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", "es");
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.DefaultCultureMessage);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!
            .Should()
            .HaveCount(1);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be(nameof(ErrorResource.Null));
        error.Message
            .Should()
            .Be("No debe tener un valor");
    }
    
    [Fact]
    public async Task Get_message_in_parent_culture_when_requested_culture_is_not_supported()
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services => services
            .AddCrossValidation());
        var client = testWebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", "en-IE");
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.DefaultCultureMessage);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!
            .Should()
            .HaveCount(1);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be(nameof(ErrorResource.Null));
        error.Message
            .Should()
            .Be(ErrorResource.Null);
    }
    
    [Theory]
    [InlineData("en", "Hello")]
    [InlineData("es", "Hola")]
    public async Task Get_message_from_resx_associated_culture(string languageCode, string expectedMessage)
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
                options.AddResx<ErrorResource1>());
        });
        var client = testWebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessExceptionWithCodeFromCustomResx);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!
            .Should()
            .HaveCount(1);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be(nameof(ErrorResource1.Hello));
        error.Message
            .Should()
            .Be(expectedMessage);
    }
    
    [Theory]
    [InlineData("en", "Hello")]
    [InlineData("es", "Hola")]
    public async Task Get_message_from_resx_when_several_resx_and_associated_cultures_are_added(string languageCode, string expectedMessage)
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.AddResx<ErrorResource1>();
                options.AddResx<ErrorResource2>();
            });
        });
        var client = testWebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessExceptionWithCodeFromCustomResx);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        problemDetails.Errors!
            .Should()
            .HaveCount(1);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be(nameof(ErrorResource1.Hello));
        error.Message
            .Should()
            .Be(expectedMessage);
    }
    
    [Theory]
    [InlineData("en", "The value is 3.3")]
    [InlineData("es", "El valor es 3,3")]
    public async Task Get_message_with_decimal(string languageCode, string expectedMessage)
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.AddResx<ErrorResource1>();
            });
        });
        var client = testWebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Language", languageCode);
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.UseDecimal);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Message
            .Should()
            .Be(expectedMessage);
    }
    
    [Fact]
    public async Task Get_placeholders_from_FrontBusinessException()
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.AddResx<ErrorResource1>();
            });
        });
        var client = testWebApplicationFactory.CreateClient();
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.FrontBusinessExceptionWithPlaceholders);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.UnprocessableEntity);
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Message
            .Should()
            .BeNull();
        error.Placeholders!
            .Should()
            .HaveCount(4);
        error.Placeholders!
            .ElementAt(0)
            .Key
            .Should()
            .Be("placeholder1");
        error.Placeholders!
            .ElementAt(1)
            .Key
            .Should()
            .Be("placeholder2");
        error.Placeholders!
            .ElementAt(2)
            .Key
            .Should()
            .Be("placeholder3");
        error.Placeholders!
            .ElementAt(3)
            .Key
            .Should()
            .Be("placeholder4");
    }
    
    [Fact]
    public async Task Get_code_from_ResxBusinessException()
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.AddResx<ErrorResource1>();
            });
        });
        var client = testWebApplicationFactory.CreateClient();
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessException);
        
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be(nameof(ErrorResourceWithNoResx.Key));
        error.Message
            .Should()
            .Be(ErrorResourceWithNoResx.Key);
    }
    
    [Fact]
    public async Task Get_code_url_when_publication_url_is_specified()
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                options.EnableErrorCodePage(publicationUrl: "https://www.backend.com");
            });
        });
        var client = testWebApplicationFactory.CreateClient();
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessException);
        
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be(nameof(ErrorResourceWithNoResx.Key));
        error.CodeUrl
            .Should()
            .Be($"https://www.backend.com/{CrossValidationOptions.ErrorCodePagePath}#Key");
    }
    
    [Theory]
    [InlineData(TestEnvironment.Development, $"http://localhost/{CrossValidationOptions.ErrorCodePagePath}#Key")]
    [InlineData(TestEnvironment.Production, null)]
    public async Task Get_code_url_when_publication_url_is_not_specified(string environment, string? expectedCodeUrl)
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options => options
                .EnableErrorCodePage());
        }).WithWebHostBuilder(builder => builder.UseEnvironment(environment));
        var client = testWebApplicationFactory.CreateClient();
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.ResxBusinessException);
        
        var problemDetails = await GetProblemDetailsFromResponse(response);
        var error = problemDetails.Errors!.First();
        error.Code
            .Should()
            .Be(nameof(ErrorResourceWithNoResx.Key));
        error.CodeUrl
            .Should()
            .Be(expectedCodeUrl);
    }
    
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_error_code_page_when_it_is_enabled(bool isEnabled)
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options =>
            {
                if (isEnabled)
                {
                    options.EnableErrorCodePage();
                }
            });
        });
        var client = testWebApplicationFactory.CreateClient();
        
        var response = await client.GetAsync(CrossValidationOptions.ErrorCodePagePath);

        AssertGet_error_code_page_when_it_is_enabled(isEnabled, response);
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
            var testWebApplicationFactory = new TestWebApplicationFactory(services =>
            {
                services.AddCrossValidation(options => options
                    .EnableErrorCodePage());
            }).WithWebHostBuilder(builder => builder.UseEnvironment(environment));
            var client = testWebApplicationFactory.CreateClient();
        
            var response = await client.GetAsync(EndpointPath.Test.Prefix + endpointPath);
        
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

        await Test(TestEnvironment.Development);
        await Test(TestEnvironment.Production);
    }
    
    [Theory]
    [InlineData(EndpointPath.Test.BusinessException)]
    [InlineData(EndpointPath.Test.UnexpectedException)]
    public async Task Get_problem_details_with_exception_extension(string endpointPath)
    {
        async Task Test(string environment)
        {
            var testWebApplicationFactory = new TestWebApplicationFactory(services =>
            {
                services.AddCrossValidation(options => options
                    .EnableErrorCodePage());
            }).WithWebHostBuilder(builder => builder.UseEnvironment(environment));
            var client = testWebApplicationFactory.CreateClient();
        
            var response = await client.GetAsync(EndpointPath.Test.Prefix + endpointPath);
        
            var problemDetails = await GetProblemDetailsFromResponse(response);

            void AssertExceptionExtensionInDevelopment()
            {
                if (environment != TestEnvironment.Development)
                {
                    return;
                }
                
                problemDetails.Exception
                    .Should()
                    .NotBeNull();
                problemDetails.Exception!
                    .Details
                    .Should()
                    .NotBeNull();
                problemDetails.Exception!
                    .Headers
                    .Should()
                    .NotBeNullOrEmpty();
                problemDetails.Exception!
                    .Path
                    .Should()
                    .NotBeNull();
            }

            void AssertExceptionExtensionInProduction()
            {
                if (environment != TestEnvironment.Production)
                {
                    return;
                }
                
                problemDetails.Exception
                    .Should()
                    .BeNull();
            }
            
            AssertExceptionExtensionInDevelopment();
            AssertExceptionExtensionInProduction();
        }

        await Test(TestEnvironment.Development);
        await Test(TestEnvironment.Production);
    }

    [Fact]
    public async Task Return_default_http_response_when_http_response_customizers_are_disabled()
    {
        var testWebApplicationFactory = new TestWebApplicationFactory(services =>
        {
            services.AddCrossValidation(options => options
                .NotCustomizeHttpResponse());
        });
        var client = testWebApplicationFactory.CreateClient();
        
        var response = await client.GetAsync(EndpointPath.Test.Prefix + EndpointPath.Test.BusinessException);
        
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.InternalServerError);
        response.Content
            .Headers
            .ToDictionary()["Content-Type"]
            .First()
            .Should()
            .Be("text/plain; charset=utf-8");
    }

    private async Task<CrossValidationProblemDetails> GetProblemDetailsFromResponse(HttpResponseMessage response)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var contentStream = await response.Content.ReadAsStreamAsync();
        var problemDetails = await JsonSerializer.DeserializeAsync<CrossValidationProblemDetails>(contentStream, jsonSerializerOptions);
        return problemDetails!;
    }
    
    private void AssertGet_error_code_page_when_it_is_enabled(bool isEnabled, HttpResponseMessage response)
    {
        if (isEnabled)
        {
            response.StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
        }
        else
        {
            response.StatusCode
                .Should()
                .Be(HttpStatusCode.NotFound);
        }
    }
}