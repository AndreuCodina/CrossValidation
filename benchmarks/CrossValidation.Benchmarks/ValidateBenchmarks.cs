using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotTrace;

namespace CrossValidation.Benchmarks;

[DotTraceDiagnoser]
[MemoryDiagnoser]
[RankColumn]
public class ValidateBenchmarks
{
    private const int Value = 1;
    private readonly Model _model = new(Value);

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
    
    [Benchmark]
    public void ModelValidator_with_exception_accumulation()
    {
        new ModelValidatorExceptionAccumulationSuccess().Validate(_model);
    }
    
    [Benchmark]
    public void ModelValidator_with_exception_accumulation_fails()
    {
        try
        {
            new ModelValidatorExceptionAccumulationFail().Validate(_model);
        }
        catch (Exception)
        {
        }
    }

    private record Model(int Value);

    private record ModelValidatorSuccess : ModelValidator<Model>
    {
        public override void CreateValidations()
        {
            Field(Model)
                .Must(_ => true);
        }
    }
    
    private record ModelValidatorFail : ModelValidator<Model>
    {
        public override void CreateValidations()
        {
            Field(Model)
                .Must(_ => false);
        }
    }
    
    private record ModelValidatorExceptionAccumulationSuccess : ModelValidator<Model>
    {
        public override void CreateValidations()
        {
            Field(Model)
                .Must(_ => true)
                .Transform(x => x.ToString())
                .Must(_ => true);
        }
    }
    
    private record ModelValidatorExceptionAccumulationFail : ModelValidator<Model>
    {
        public override void CreateValidations()
        {
            Field(Model)
                .Must(_ => false)
                .Transform(x => x.ToString())
                .Must(_ => true);
        }
    }
}