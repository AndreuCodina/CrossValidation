namespace CrossValidation.WebApplication;

public static class ApiPath
{
    public static class Test
    {
        public const string Prefix = "/tests";
        public const string CrossException = "/crossException";
        public const string ValidationListException = "/validationListException";
        public const string ErrorWithCodeFromCustomResx = "/errorWithCodeFromCustomResx";
        public const string ErrorWithCodeWithoutResxKey = "/errorWithCodeWithoutResxKey";
        public const string ReplaceBuiltInCodeWithCustomResx = "/replaceBuiltInCodeWithCustomResx";
        public const string DefaultCultureMessage = "/defaultCultureMessage";
        public const string ErrorWithStatusCode = "/errorWithStatusCode";
        public const string Exception = "/exception";
        public const string UseDecimal = "/useDecimal";
        public const string FrontBusinessExceptionWithPlaceholders = "/frontBusinessExceptionWithPlaceholders";
    }
}