﻿using CrossValidation.Resources;

namespace CrossValidation.Errors;

public record CommonCrossError(string Code) : CodeCrossError(Code)
{
    public record NotNull() : CommonCrossError(nameof(ErrorResource.NotNull));
    
    public record Null() : CommonCrossError(nameof(ErrorResource.Null));

    public record GreaterThan<TField>(TField ComparisonValue) :
        CommonCrossError(nameof(ErrorResource.GreaterThan))
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(ComparisonValue!);
            base.AddPlaceholderValues();
        }
    }
    
    public record Enum() : CommonCrossError(nameof(ErrorResource.Enum));
    
    public record EnumRange() : CommonCrossError(nameof(ErrorResource.Enum));
    
    public record LengthRange(int Minimum, int Maximum, int TotalLength) :
        CommonCrossError(nameof(ErrorResource.LengthRange)),
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
        CommonCrossError(nameof(ErrorResource.MinimumLength)),
        ILengthError
    {
        public override void AddPlaceholderValues()
        {
            AddPlaceholderValue(Minimum);
            AddPlaceholderValue(TotalLength);
            base.AddPlaceholderValues();
        }
    }
    
    public record Predicate() : CommonCrossError(nameof(ErrorResource.Predicate));
    
    public record RegularExpression() : CommonCrossError(nameof(ErrorResource.RegularExpression));
}