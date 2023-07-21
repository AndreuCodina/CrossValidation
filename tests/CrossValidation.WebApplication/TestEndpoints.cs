using System.Net;
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
            throw new BusinessException();
        });

        group.MapGet(ApiPath.Test.ValidationListException, () =>
        {
            throw new BusinessListException(new List<BusinessException>());
        });

        group.MapGet(ApiPath.Test.ExceptionWithCodeFromCustomResx, () =>
        {
            throw new ExceptionWithCodeFromCustomResx();
        });

        group.MapGet(ApiPath.Test.ExceptionWithCodeWithoutResxKey, () =>
        {
            throw new ExceptionWithCodeWithoutResxKey();
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

        group.MapGet(ApiPath.Test.ExceptionWithStatusCode, () =>
        {
            throw new BusinessException(statusCode: HttpStatusCode.Created);
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
        
        group.MapGet(ApiPath.Test.FrontBusinessExceptionWithPlaceholders, () =>
        {
            throw new FrontBusinessExceptionWithPlaceholders<string>(1, "value");
        });
        
        group.MapGet(ApiPath.Test.ResxBusinessException, () =>
        {
            throw new CustomResxBusinessException();
        });

        return builder;
    }
}