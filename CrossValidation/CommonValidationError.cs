using CrossValidation.Resources;
using CrossValidation.Results;

namespace CrossValidation;

public abstract record CommonValidationError(string Code) : ResXValidationError(Code)
{
    public record NotNull() : CommonValidationError(nameof(ErrorResource.NotNull));
    
    public record Null() : CommonValidationError(nameof(ErrorResource.Null));

    public record GreaterThan<T>(T ComparisonValue) :
        CommonValidationError(nameof(ErrorResource.GreaterThan))
    {
        public override void AddPlaceHolderValues()
        {
            AddPlaceholderValue(ComparisonValue!);
            base.AddPlaceHolderValues();
        }
    }
    
    public record Enum() : CommonValidationError(nameof(ErrorResource.Enum));
    
    public record LengthRange(int Minimum, int Maximum) :
        CommonValidationError(nameof(ErrorResource.Length))
    {
        public override void AddPlaceHolderValues()
        {
            AddPlaceholderValue(Minimum);
            AddPlaceholderValue(Maximum);
            base.AddPlaceHolderValues();
        }
    }
}