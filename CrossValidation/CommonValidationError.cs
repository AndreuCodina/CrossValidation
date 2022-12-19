using CrossValidation.Resources;
using CrossValidation.Results;

namespace CrossValidation;

public abstract record CommonValidationError(string Code) : ResXValidationError(Code)
{
    public record NotNull() : CommonValidationError("NotNull");
    public record Null() : CommonValidationError("Null");

    public record GreaterThan<T>(T ComparisonValue) :
        CommonValidationError(nameof(ErrorResource.GreaterThan)),
        IPlaceholderValue
    {
        public override void AddPlaceHolderValues()
        {
            AddPlaceholderValue(ComparisonValue);
            base.AddPlaceHolderValues();
        }
    }
    public record Enum() : CommonValidationError("Enum");
    public record Length(int Minimum, int Maximum) :
        CommonValidationError(nameof(ErrorResource.Length)),
        IPlaceholderValue
    {
        public override void AddPlaceHolderValues()
        {
            AddPlaceholderValue(Minimum);
            AddPlaceholderValue(Maximum);
            base.AddPlaceHolderValues();
        }
    }
}

public interface IPlaceholderValue
{
    // public 
}