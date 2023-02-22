namespace CrossValidation.Extensions;

public static class NullExtensions
{
    public static TResult? Map<T, TResult>(this T? value, Func<T, TResult> f)
    {
        return value is not null ? f(value) : default;
    }
}