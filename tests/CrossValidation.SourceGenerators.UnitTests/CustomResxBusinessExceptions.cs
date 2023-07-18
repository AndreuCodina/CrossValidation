using CrossValidation.Exceptions;
using CrossValidation.SourceGenerators.UnitTests.Resources;

public partial class WithNoParametersGlobalNamespaceException()
    : ResxBusinessException(TestErrorResource.WithNoParameters);

public partial class WithParametersGlobalNamespaceException(int firstParameter, string secondParameter)
    : ResxBusinessException(TestErrorResource.WithParameters);

namespace ParentNamespace
{
    public partial class WithNoParametersWithDeclaredNamespaceException()
        : ResxBusinessException(TestErrorResource.WithNoParameters);

    public partial class WithParametersWithDeclaredNamespaceException(int firstParameter, string secondParameter)
        : ResxBusinessException(TestErrorResource.WithParameters);
    
    namespace ChildNamespace.GrandchildNamespace
    {
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
    }
}