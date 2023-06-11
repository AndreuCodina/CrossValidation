namespace CrossValidation.Extensions;

public static class NullExtensions
{
    public static TResult? Map<T, TResult>(this T? value, Func<T, TResult> f)
        where T : struct
        where TResult : class
    {
        if (value is null)
        {
            return null;
        }
        
        return f(value.Value);
    }
    
    public static TResult? Map<T, TResult>(this T? value, Func<T, TResult> f)
        where T : class
        where TResult : class
    {
        if (value is null)
        {
            return null;
        }
        
        return f(value);
    }
}