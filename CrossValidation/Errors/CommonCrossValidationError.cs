using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.Results;

namespace CrossValidation;

public abstract record CommonCrossValidationError(string Code) : ValidationErrorByCode(Code)
{
    public record NotNull() : CommonCrossValidationError(nameof(ErrorResource.NotNull));
    
    public record Null() : CommonCrossValidationError(nameof(ErrorResource.Null));

    public record GreaterThan<TField>(TField ComparisonValue) :
        CommonCrossValidationError(nameof(ErrorResource.GreaterThan))
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(ComparisonValue!);
            base.AddPlaceholderValues();
        }
    }
    
    public record Enum() : CommonCrossValidationError(nameof(ErrorResource.Enum));
    
    public record LengthRange(int Minimum, int Maximum, int TotalLength) :
        CommonCrossValidationError(nameof(ErrorResource.LengthRange)),
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
        CommonCrossValidationError(nameof(ErrorResource.MinimumLength)),
        ILengthError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Minimum);
            AddPlaceholderValue(TotalLength);
            base.AddPlaceholderValues();
        }
    }
    
    public record Predicate() : CommonCrossValidationError(nameof(ErrorResource.Predicate));
}