using BenchmarkDotNet.Attributes;
using CrossValidation.Errors;
using CrossValidation.Rules;

namespace CrossValidation.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class WithErrorBenchmarks
{
    private IRule<int> _rule = default!;

    [GlobalSetup]
    public void Setup()
    {
        _rule = Validate.That(1);
    }
    
    [Benchmark(Baseline = true)]
    public void WithError()
    {
        _rule
            .WithError(new ValidationError(Message: "Error message"))
            .Must(_ => true);
    }
}