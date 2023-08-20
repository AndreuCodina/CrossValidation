using System.Net;
using CrossValidation.Exceptions;
using CrossValidation.Validations;
using CrossValidation.WebApplication.Resources;

namespace CrossValidation.WebApplication;

public static class TestEndpoints
{
    public static IEndpointRouteBuilder UseTestEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup(EndpointPath.Test.Prefix);
        
        group.MapGet(EndpointPath.Test.BusinessException, () =>
        {
            throw new BusinessException();
        });

        group.MapGet(EndpointPath.Test.BusinessListException, () =>
        {
            throw new BusinessListException(new List<BusinessException>());
        });

        group.MapGet(EndpointPath.Test.ResxBusinessExceptionWithCodeFromCustomResx, () =>
        {
            throw new ResxBusinessExceptionWithCodeFromCustomResx();
        });

        group.MapGet(EndpointPath.Test.ExceptionWithCodeWithoutResxKey, () =>
        {
            throw new ExceptionWithCodeWithoutResxKey();
        });

        group.MapGet(EndpointPath.Test.ReplaceBuiltInCodeWithCustomResx, () =>
        {
            var model = new Model();
            Validate.Field(model.NullableString)
                .NotNull();
        });

        group.MapGet(EndpointPath.Test.DefaultCultureMessage, () =>
        {
            var model = new Model();
            model.NullableString = "";
            Validate.Field(model.NullableString)
                .Null();
        });

        group.MapGet(EndpointPath.Test.BusinessExceptionWithStatusCodeWithMapping, () =>
        {
            throw new BusinessException(statusCode: HttpStatusCode.Created);
        });
        
        group.MapGet(EndpointPath.Test.BusinessExceptionWithStatusCodeWithNoMapping, () =>
        {
            throw new BusinessException(statusCodeInt: 450);
        });
        
        group.MapGet(EndpointPath.Test.BusinessExceptionWithErrorStatusCode, () =>
        {
            throw new BusinessException(statusCode: HttpStatusCode.GatewayTimeout);
        });

        group.MapGet(EndpointPath.Test.UnexpectedException, () =>
        {
            throw new Exception();
        });
        
        group.MapGet(EndpointPath.Test.UseDecimal, () =>
        {
            Validate.That("")
                .WithMessage(string.Format(ErrorResource1.UseDecimal, 3.3))
                .Must(_ => false);
        });
        
        group.MapGet(EndpointPath.Test.FrontBusinessExceptionWithPlaceholders, () =>
        {
            throw new FrontBusinessExceptionWithPlaceholders<string>(1, "genericValue", "stringValue1", "stringValue2");
        });
        
        group.MapGet(EndpointPath.Test.ResxBusinessException, () =>
        {
            throw new CustomResxBusinessException();
        });
        
        group.MapGet(EndpointPath.Test.ErrorStatusCodeWithEmptyBody, () =>
        {
            return TypedResults.NotFound();
        });

        return builder;
    }
}