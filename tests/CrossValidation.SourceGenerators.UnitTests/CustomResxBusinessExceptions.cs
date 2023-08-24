using System.Net;
using CrossValidation.Exceptions;
using CrossValidation.SourceGenerators.UnitTests.Resources;

namespace CrossValidation.SourceGenerators.UnitTests;

public partial class WithNoParametersGlobalNamespaceException()
    : ResxBusinessException(TestErrorResource.WithNoParameters);

public partial class WithParametersGlobalNamespaceException(int firstParameter, string secondParameter)
    : ResxBusinessException(TestErrorResource.WithParameters);

public partial class WithNoParametersWithDeclaredNamespaceException()
    : ResxBusinessException(TestErrorResource.WithNoParameters);

public partial class WithParametersWithDeclaredNamespaceException(int firstParameter, string secondParameter)
    : ResxBusinessException(TestErrorResource.WithParameters);
    
public class ClassBase;

public partial class GenericClass<T>()
    : ResxBusinessException(TestErrorResource.WithNoParameters)
    where T : ClassBase;
        
#pragma warning disable CS9113 // Parameter is unread.
internal partial class ParentClass<T1>(T1 generic) where T1 : class
#pragma warning restore CS9113 // Parameter is unread.
{
    public partial class ChildClass
    {
        internal partial class ExceptionWithParameters<T2>(int firstParameter, T2 secondParameter)
            : ResxBusinessException(key: TestErrorResource.WithParameters)
            where T2 : struct;
    }
}

public partial class WithNoParametersNoMessageException()
    : ResxBusinessException(statusCode: HttpStatusCode.Accepted);

public partial class DeclaredSeveralTimesException : ResxBusinessException;
public partial class DeclaredSeveralTimesException;
    
public partial class DeclaredSeveralTimesWithInheritanceException : ResxBusinessException;
public partial class DeclaredSeveralTimesWithInheritanceException : ResxBusinessException;