using CrossValidation.DependencyInjection;
using CrossValidation.Errors;
using CrossValidation.WebApplication;
using CrossValidation.WebApplication.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCrossValidation();

var app = builder.Build();
app.UseCrossValidation();

app.UseTestEndpoints();

app.Run();

public partial class Program
{
}

public record ErrorWithCodeFromCustomResx() : CodeCrossError(ErrorResource1.Hello);

public record ErrorWithCodeWithoutResxKey() : CompleteCrossError(Code: "RandomCode");