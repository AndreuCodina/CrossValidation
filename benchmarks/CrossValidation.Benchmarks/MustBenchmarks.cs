using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotTrace;
using CrossValidation.Exceptions;

namespace CrossValidation.Benchmarks;

[DotTraceDiagnoser]
[MemoryDiagnoser]
[RankColumn]
public class MustBenchmarks
{
    private const int Value = 1;

    [Benchmark]
    public void Must_returns_exception()
    {
        try
        {
            Validate.That(Value)
                .Must(_ => CheckReturnsException());
        }
        catch (Exception)
        {
        }
    }
    
    [Benchmark]
    public void Must_returns_boolean()
    {
        try
        {
            Validate.That(Value)
                .WithException(new BusinessException())
                .Must(_ => CheckReturnsBoolean());
        }
        catch (Exception)
        {
        }
    }

    private BusinessException CheckReturnsException()
    {
        return new BusinessException();
    }
    
    private bool CheckReturnsBoolean()
    {
        return false;
    }
}