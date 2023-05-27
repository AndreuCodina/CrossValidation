using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.Validations;
using CrossValidation.WebApplication.Resources;

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
            var model = new Model();
            Validate.Field(model.NullableString)
                .NotNull();
        });

        group.MapGet(ApiPath.Test.DefaultCultureMessage, () =>
        {
            var model = new Model();
            model.NullableString = "";
            Validate.Field(model.NullableString)
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
        
        group.MapGet(ApiPath.Test.UseDecimal, () =>
        {
            Validate.That("")
                .WithMessage(string.Format(ErrorResource1.UseDecimal, 3.3))
                .Must(_ => false);
        });

        return builder;
    }
}