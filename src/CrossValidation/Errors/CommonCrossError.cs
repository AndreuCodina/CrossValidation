using CrossValidation.Resources;

namespace CrossValidation.Errors;

public record CommonCrossError(string Code) : CodeCrossError(Code)
{
    public override bool IsCommon => true;
    
    public record NotNull() : CommonCrossError(nameof(ErrorResource.NotNull));
    
    public record Null() : CommonCrossError(nameof(ErrorResource.Null));

    public record GreaterThan<TField>(TField ComparisonValue) :
        CommonCrossError(nameof(ErrorResource.GreaterThan));

    public record Enum() : CommonCrossError(nameof(ErrorResource.Enum));
    
    public record EnumRange() : CommonCrossError(nameof(ErrorResource.Enum));

    public record LengthRange(int Minimum, int Maximum, int TotalLength) :
        CommonCrossError(nameof(ErrorResource.LengthRange)),
        ILengthError;

    public record MinimumLength(int Minimum, int TotalLength) :
        CommonCrossError(nameof(ErrorResource.MinimumLength)),
        ILengthError;
    
    public record Predicate() : CommonCrossError(nameof(ErrorResource.General));
    
    public record RegularExpression() : CommonCrossError(nameof(ErrorResource.RegularExpression));
    
    public record EmptyString() : CommonCrossError(nameof(ErrorResource.EmptyString));
    
    public record NotEmptyString() : CommonCrossError(nameof(ErrorResource.NotEmptyString));
    
    public record EmptyCollection() : CommonCrossError(nameof(ErrorResource.EmptyCollection));
    
    public record NotEmptyCollection() : CommonCrossError(nameof(ErrorResource.NotEmptyCollection));
    
}