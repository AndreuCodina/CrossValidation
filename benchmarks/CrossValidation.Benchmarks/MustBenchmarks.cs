using BenchmarkDotNet.Attributes;
using CrossValidation.Errors;

namespace CrossValidation.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class MustBenchmarks
{
    private const int Value = 1;

    [Benchmark]
    public void Must_returns_error()
    {
        try
        {
            Validate.That(Value)
                .Must(_ => CheckReturnsError());
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
                .WithError(new CrossError())
                .Must(_ => CheckReturnsBoolean());
        }
        catch (Exception)
        {
        }
    }

    private ICrossError CheckReturnsError()
    {
        return new CrossError();
    }
    
    private bool CheckReturnsBoolean()
    {
        return false;
    }
}