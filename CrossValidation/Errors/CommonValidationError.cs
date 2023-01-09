using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.Results;

namespace CrossValidation;

public record CommonValidationError(string Code) : ValidationErrorByCode(Code)
{
    public record NotNull() : CommonValidationError(nameof(ErrorResource.NotNull));
    
    public record Null() : CommonValidationError(nameof(ErrorResource.Null));

    public record GreaterThan<TField>(TField ComparisonValue) :
        CommonValidationError(nameof(ErrorResource.GreaterThan))
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(ComparisonValue!);
            base.AddPlaceholderValues();
        }
    }
    
    public record Enum() : CommonValidationError(nameof(ErrorResource.Enum));
    
    public record LengthRange(int Minimum, int Maximum, int TotalLength) :
        CommonValidationError(nameof(ErrorResource.LengthRange)),
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
        CommonValidationError(nameof(ErrorResource.MinimumLength)),
        ILengthError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Minimum);
            AddPlaceholderValue(TotalLength);
            base.AddPlaceholderValues();
        }
    }
    
    public record Predicate() : CommonValidationError(nameof(ErrorResource.Predicate));
}