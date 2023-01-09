using BenchmarkDotNet.Attributes;

namespace CrossValidation.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class ValidateBenchmarks
{
    private const int Value = 1;
    private readonly Model _model = new Model(Value);

    [Benchmark]
    public void ValidateThat()
    {
        Validate.That(Value)
            .Must(_ => true);
    }

    [Benchmark]
    public void ValidateThat_fails()
    {
        try
        {
            Validate.That(1)
                .Must(_ => false);
        }
        catch (Exception)
        {
        }
    }

    [Benchmark]
    public void ModelValidator()
    {
        new ModelValidatorSuccess().Validate(_model);
    }
    
    [Benchmark]
    public void ModelValidator_fails()
    {
        try
        {
            new ModelValidatorFail().Validate(_model);
        }
        catch (Exception)
        {
        }
    }

    private record Model(int Value);

    private record ModelValidatorSuccess : ModelValidator<Model>
    {
        public override void CreateRules(Model model)
        {
            RuleFor(x => x.Value)
                .Must(_ => true);
        }
    }
    
    private record ModelValidatorFail : ModelValidator<Model>
    {
        public override void CreateRules(Model model)
        {
            RuleFor(x => x.Value)
                .Must(_ => false);
        }
    }
}