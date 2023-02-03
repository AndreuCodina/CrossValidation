using CrossValidation.Resources;

namespace CrossValidation.Errors;

public record CommonCodeValidationError(string Code) : CodeValidationError(Code)
{
    public record NotNull() : CommonCodeValidationError(nameof(ErrorResource.NotNull));
    
    public record Null() : CommonCodeValidationError(nameof(ErrorResource.Null));

    public record GreaterThan<TField>(TField ComparisonValue) :
        CommonCodeValidationError(nameof(ErrorResource.GreaterThan))
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(ComparisonValue!);
            base.AddPlaceholderValues();
        }
    }
    
    public record Enum() : CommonCodeValidationError(nameof(ErrorResource.Enum));
    
    public record LengthRange(int Minimum, int Maximum, int TotalLength) :
        CommonCodeValidationError(nameof(ErrorResource.LengthRange)),
        ILengthError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Minimum);
            AddPlaceholderValue(Maximum);
            AddPlaceholderValue(TotalLength);
            base.AddPlaceholderValues();
        }
    }
    
    public record MinimumLength(int Minimum, int TotalLength) :
        CommonCodeValidationError(nameof(ErrorResource.MinimumLength)),
        ILengthError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Minimum);
            AddPlaceholderValue(TotalLength);
            base.AddPlaceholderValues();
        }
    }
    
    public record Predicate() : CommonCodeValidationError(nameof(ErrorResource.Predicate));
}