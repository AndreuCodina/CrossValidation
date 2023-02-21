using System.Net;
using CrossValidation;
using CrossValidation.DependencyInjection;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Validations;
using CrossValidation.WebApplication.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCrossValidation(options =>
{
    options.AddResx<ErrorResource1>();
});

var app = builder.Build();
app.UseCrossValidation(options =>
{
    options.SetDefaultCulture("en");
    options.SetSupportedCultures("en", "es");
});

app.MapGet("/crossException", () =>
{
    throw new CrossException(new CrossError());
});

app.MapGet("/validationListException", () =>
{
    throw new ValidationListException(new List<ICrossError>());
});

app.MapGet("/errorWithCodeFromCustomResx", () =>
{
    throw new ErrorWithCodeFromCustomResx().ToException();
});

app.MapGet("/errorWithCodeWithoutResxKey", () =>
{
    throw new ErrorWithCodeWithoutResxKey().ToException();
});

app.MapGet("/replaceBuiltInCodeWithCustomResx", () =>
{
    string? value = null;
    Validate.That(value)
        .NotNull();
});

app.MapGet("/defaultCultureMessage", () =>
{
    string? value = "";
    Validate.That(value)
        .Null();
});

app.MapGet("/errorWithStatusCode", () =>
{
    throw new CrossError(HttpStatusCode: HttpStatusCode.Created).ToException();
});

app.Run();

public record ErrorWithCodeFromCustomResx() : CodeCrossError(nameof(ErrorResource1.Hello));

public record ErrorWithCodeWithoutResxKey() : CodeCrossError("RandomKey");