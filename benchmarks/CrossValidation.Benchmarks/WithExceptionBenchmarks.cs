using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotTrace;
using CrossValidation.Exceptions;
using CrossValidation.Validations;

namespace CrossValidation.Benchmarks;

[DotTraceDiagnoser]
[MemoryDiagnoser]
[RankColumn]
public class WithExceptionBenchmarks
{
    private IValidation<int> _validation = default!;

    [GlobalSetup]
    public void Setup()
    {
        _validation = Validate.That(1);
    }
    
    [Benchmark(Baseline = true)]
    public void WithException()
    {
        _validation
            .WithException(new BusinessException(message: "Error message"))
            .Must(_ => true);
    }
}