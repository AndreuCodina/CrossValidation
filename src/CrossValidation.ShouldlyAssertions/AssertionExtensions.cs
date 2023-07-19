// using CrossValidation.Exceptions;
// using Shouldly;
//
// namespace CrossValidation.ShouldlyAssertions;
//
// [ShouldlyMethods]
// public static class AssertionExtensions
// {
//     public static BusinessException ShouldThrowCrossError(this Action actual, string? customMessage = null)
//     {
//         return actual.ShouldThrow<BusinessException>(customMessage);
//     }
//     
//     public static async Task<BusinessException> ShouldThrowCrossErrorAsync(this Task<Action> actual, string? customMessage = null)
//     {
//         return await actual.ShouldThrowAsync<BusinessException>(customMessage);
//     }
//     
//     public static BusinessException ShouldThrowCrossError(this Func<object?> actual, string? customMessage = null)
//     {
//         return actual.ShouldThrow<BusinessException>(customMessage);
//     }
//     
//     public static async Task<BusinessException> ShouldThrowCrossErrorAsync(this Func<Task> actual, string? customMessage = null)
//     {
//         return await actual.ShouldThrowAsync<BusinessException>(customMessage);
//     }
//     
//     public static TCrossError ShouldThrowCrossError<TCrossError>(this Action actual, string? customMessage = null)
//         where TCrossError : BusinessException
//     {
//         var error = actual.ShouldThrow<BusinessException>();
//         return error.ShouldBeOfType<TCrossError>(customMessage);
//     }
//     
//     public static TCrossError ShouldThrowCrossError<TCrossError>(this Func<object?> actual, string? customMessage = null)
//         where TCrossError : BusinessException
//     {
//         var error = actual.ShouldThrow<BusinessException>();
//         return error.ShouldBeOfType<TCrossError>(customMessage);
//     }
//     
//     public static async Task<TCrossError> ShouldThrowCrossErrorAsync<TCrossError>(this Task<Action> actual, string? customMessage = null)
//         where TCrossError : BusinessException
//     {
//         var error = await actual.ShouldThrowAsync<BusinessException>();
//         return error.ShouldBeOfType<TCrossError>(customMessage);
//     }
//     
//     public static TCrossError ShouldThrowCrossError<TCrossError>(this Func<Task> actual, string? customMessage = null)
//         where TCrossError : BusinessException
//     {
//         var error = actual.ShouldThrow<BusinessException>();
//         return error.ShouldBeOfType<TCrossError>(customMessage);
//     }
//     
//     public static async Task<TCrossError> ShouldThrowCrossErrorAsync<TCrossError>(this Func<Task> actual, string? customMessage = null)
//         where TCrossError : BusinessException
//     {
//         var error = await actual.ShouldThrowAsync<BusinessException>();
//         return error.ShouldBeOfType<TCrossError>(customMessage);
//     }
//     
//     public static List<BusinessException> ShouldThrowCrossErrors(this Action actual, string? customMessage = null)
//     {
//         var errors = actual.ShouldThrow<ValidationListException>(customMessage).Exceptions;
//         return errors;
//     }
//     
//     public static async Task<List<BusinessException>> ShouldThrowCrossErrorsAsync(this Task<Action> actual, string? customMessage = null)
//     {
//         var errors = (await actual.ShouldThrowAsync<ValidationListException>(customMessage)).Exceptions;
//         return errors;
//     }
//     
//     public static List<BusinessException> ShouldThrowCrossErrors(this Func<object?> actual, string? customMessage = null)
//     {
//         var errors = actual.ShouldThrow<ValidationListException>(customMessage).Exceptions;
//         return errors;
//     }
//     
//     public static async Task<List<BusinessException>> ShouldThrowCrossErrorsAsync(this Func<Task> actual, string? customMessage = null)
//     {
//         var errors = (await actual.ShouldThrowAsync<ValidationListException>(customMessage)).Exceptions;
//         return errors;
//     }
// }