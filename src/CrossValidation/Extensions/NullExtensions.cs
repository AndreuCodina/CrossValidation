namespace CrossValidation.Extensions;

public static class NullExtensions
{
    public static TResult? Map<T, TResult>(this T? value, Func<T, TResult> f)
        where T : struct
    {
        return value is not null ? f(value.Value) : default;
    }
    
    public static TResult? Map<T, TResult>(this T? value, Func<T, TResult> f)
        where T : class
    {
        return value is not null ? f(value) : default;
    }
}