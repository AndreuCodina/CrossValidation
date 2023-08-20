using CrossValidation.DependencyInjection;
using CrossValidation.Exceptions;
using CrossValidation.WebApplication;
using CrossValidation.WebApplication.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCrossValidation();

var app = builder.Build();
app.UseCrossValidation();
app.UseTestEndpoints();

// Por qué no obtengo el "exception" en el Problem Details de Postman??
app.MapGet("/foo", () =>
{
    throw new Exception("foo");
});

app.Run();

public partial class Program;

public class ResxBusinessExceptionWithCodeFromCustomResx() : ResxBusinessException(ErrorResource1.Hello);

public class ExceptionWithCodeWithoutResxKey() : BusinessException(code: "RandomCode");

public partial class FrontBusinessExceptionWithPlaceholders<T>(
    int placeholder1,
    T placeholder2,
    string? placeholder3,
    string placeholder4)
    : FrontBusinessException;

public static class ErrorResourceWithNoResx
{
    public const string Key = "Translation";
}

public partial class CustomResxBusinessException() : ResxBusinessException(ErrorResourceWithNoResx.Key);