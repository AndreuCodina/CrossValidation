namespace CrossValidation.WebApplication;

public static class ApiPath
{
    public static class Test
    {
        public const string Prefix = "/tests";
        public const string CrossException = "/crossException";
        public const string BusinessListException = "/validationListException";
        public const string ExceptionWithCodeFromCustomResx = "/exceptionWithCodeFromCustomResx";
        public const string ExceptionWithCodeWithoutResxKey = "/exceptionWithCodeWithoutResxKey";
        public const string ReplaceBuiltInCodeWithCustomResx = "/replaceBuiltInCodeWithCustomResx";
        public const string DefaultCultureMessage = "/defaultCultureMessage";
        public const string ExceptionWithStatusCode = "/exceptionWithStatusCode";
        public const string Exception = "/exception";
        public const string UseDecimal = "/useDecimal";
        public const string FrontBusinessExceptionWithPlaceholders = "/frontBusinessExceptionWithPlaceholders";
        public const string ResxBusinessException = "/resxBusinessException";
    }
}