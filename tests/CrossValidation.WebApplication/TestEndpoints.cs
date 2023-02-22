using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Validations;

namespace CrossValidation.WebApplication;

public static class TestEndpoints
{
    public static IEndpointRouteBuilder UseTestEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup(ApiPath.Test.Prefix);
        
        group.MapGet(ApiPath.Test.CrossException, () =>
        {
            throw new CrossException(new CrossError());
        });

        group.MapGet(ApiPath.Test.ValidationListException, () =>
        {
            throw new ValidationListException(new List<ICrossError>());
        });

        group.MapGet(ApiPath.Test.ErrorWithCodeFromCustomResx, () =>
        {
            throw new ErrorWithCodeFromCustomResx().ToException();
        });

        group.MapGet(ApiPath.Test.ErrorWithCodeWithoutResxKey, () =>
        {
            throw new ErrorWithCodeWithoutResxKey().ToException();
        });

        group.MapGet(ApiPath.Test.ReplaceBuiltInCodeWithCustomResx, () =>
        {
            string? value = null;
            Validate.That(value)
                .NotNull();
        });

        group.MapGet(ApiPath.Test.DefaultCultureMessage, () =>
        {
            string? value = "";
            Validate.That(value)
                .Null();
        });

        group.MapGet(ApiPath.Test.ErrorWithStatusCode, () =>
        {
            throw new CrossError(HttpStatusCode: HttpStatusCode.Created).ToException();
        });

        group.MapGet(ApiPath.Test.Exception, () =>
        {
            throw new Exception();
        });

        return builder;
    }
}