using CrossValidation.DependencyInjection;
using CrossValidation.Errors;
using CrossValidation.Exceptions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCrossValidation();

var app = builder.Build();
app.UseCrossValidation();

app.MapGet("/validationException", () =>
{
    throw new ValidationListException(new List<ICrossError>());
});

app.MapGet("/exception", () =>
{
    throw new ValidationListException(new List<ICrossError>());
});

app.Run();