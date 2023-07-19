using CrossValidation.DependencyInjection;
using CrossValidation.Exceptions;
using CrossValidation.WebApplication;
using CrossValidation.WebApplication.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCrossValidation();

var app = builder.Build();
app.UseCrossValidation();

app.UseTestEndpoints();

app.Run();

public partial class Program;

public class ErrorWithCodeFromCustomResx() : ResxBusinessException(ErrorResource1.Hello);

public class ErrorWithCodeWithoutResxKey() : BusinessException(code: "RandomCode");

public partial class FrontBusinessExceptionWithPlaceholders<T>(int placeholder1, T placeholder2)
    : FrontBusinessException;