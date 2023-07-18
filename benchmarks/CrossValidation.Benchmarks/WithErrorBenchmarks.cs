using BenchmarkDotNet.Attributes;
using CrossValidation.Exceptions;
using CrossValidation.Validations;

namespace CrossValidation.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class WithErrorBenchmarks
{
    private IValidation<int> _validation = default!;

    [GlobalSetup]
    public void Setup()
    {
        _validation = Validate.That(1);
    }
    
    [Benchmark(Baseline = true)]
    public void WithError()
    {
        _validation
            .WithException(new BusinessException(message: "Error message"))
            .Must(_ => true);
    }
}