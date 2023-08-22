namespace CrossValidation.WebApplication;

public static class EndpointPath
{
    public static class Test
    {
        public const string Prefix = "/tests";
        public const string BusinessException = "/businessException";
        public const string BusinessListException = "/businessListException";
        public const string ResxBusinessExceptionWithCodeFromCustomResx = "/resxBusinessexceptionWithCodeFromCustomResx";
        public const string ExceptionWithCodeWithoutResxKey = "/exceptionWithCodeWithoutResxKey";
        public const string ReplaceBuiltInCodeWithCustomResx = "/replaceBuiltInCodeWithCustomResx";
        public const string DefaultCultureMessage = "/defaultCultureMessage";
        public const string BusinessExceptionWithStatusCodeWithMapping = "/businessExceptionWithStatusCodeWithMapping";
        public const string BusinessExceptionWithStatusCodeWithNoMapping = "/businessExceptionWithStatusCodeWithNoMapping";
        public const string BusinessExceptionWithErrorStatusCode = "/businessExceptionWithErrorStatusCode";
        public const string UnexpectedException = "/unexpectedException";
        public const string UseDecimal = "/useDecimal";
        public const string FrontBusinessExceptionWithPlaceholders = "/frontBusinessExceptionWithPlaceholders";
        public const string ResxBusinessException = "/resxBusinessException";
        public const string ErrorStatusCodeWithEmptyBody = "/errorStatusCodeWithEmpyBody";
    }
}