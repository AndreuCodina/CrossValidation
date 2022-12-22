using CrossValidation.Resources;
using CrossValidation.Results;

namespace CrossValidation;

public abstract record CommonCrossValidationError(string Code) : ResXValidationError(Code)
{
    public record NotNull() : CommonCrossValidationError(nameof(ErrorResource.NotNull));
    
    public record Null() : CommonCrossValidationError(nameof(ErrorResource.Null));

    public record GreaterThan<T>(T ComparisonValue) :
        CommonCrossValidationError(nameof(ErrorResource.GreaterThan))
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(ComparisonValue!);
            base.AddPlaceholderValues();
        }
    }
    
    public record Enum() : CommonCrossValidationError(nameof(ErrorResource.Enum));
    
    public record LengthRange(int Minimum, int Maximum) :
        CommonCrossValidationError(nameof(ErrorResource.LengthRange))
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Minimum);
            AddPlaceholderValue(Maximum);
            base.AddPlaceholderValues();
        }
    }
    
    public record Predicate() : CommonCrossValidationError(nameof(ErrorResource.Predicate));
}