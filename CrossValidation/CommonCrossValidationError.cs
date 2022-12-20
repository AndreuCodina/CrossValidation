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
        public override void AddPlaceHolderValues()
        {
            AddPlaceholderValue(ComparisonValue!);
            base.AddPlaceHolderValues();
        }
    }
    
    public record Enum() : CommonCrossValidationError(nameof(ErrorResource.Enum));
    
    public record LengthRange(int Minimum, int Maximum) :
        CommonCrossValidationError(nameof(ErrorResource.Length))
    {
        public override void AddPlaceHolderValues()
        {
            AddPlaceholderValue(Minimum);
            AddPlaceholderValue(Maximum);
            base.AddPlaceHolderValues();
        }
    }
}