using CrossValidation.Resources;

namespace CrossValidation.Exceptions;

public static class CommonCrossException
{
    public class NotNull() : BusinessException(code: nameof(ErrorResource.NotNull));

    public class Null() : BusinessException(code: nameof(ErrorResource.Null));

    public class GreaterThan<TField>(TField comparisonValue) :
        BusinessException(code: nameof(ErrorResource.GreaterThan))
    {
        public TField ComparisonValue => comparisonValue;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(comparisonValue);
        }
    }

    public class Enum() : BusinessException(code: nameof(ErrorResource.Enum));

    public class EnumRange() : BusinessException(code: nameof(ErrorResource.Enum));

    public class LengthRange(int minimum, int maximum, int totalLength) :
        LengthException(code: nameof(ErrorResource.LengthRange))
    {
        public override int TotalLength => totalLength;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(minimum);
            AddPlaceholderValue(maximum);
            AddPlaceholderValue(totalLength);
        }
    }

    public class MinimumLength(int minimum, int totalLength)
        : LengthException(code: nameof(ErrorResource.MinimumLength))
    {
        public override int TotalLength => totalLength;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(minimum);
            AddPlaceholderValue(totalLength);
        }
    }

    public class Predicate() : BusinessException(code: nameof(ErrorResource.General));
    
    public class RegularExpression() : BusinessException(code: nameof(ErrorResource.RegularExpression));
    
    public class EmptyString() : BusinessException(code: nameof(ErrorResource.EmptyString));
    
    public class NotEmptyString() : BusinessException(code: nameof(ErrorResource.NotEmptyString));
    
    public class EmptyCollection() : BusinessException(code: nameof(ErrorResource.EmptyCollection));
    
    public class NotEmptyCollection() : BusinessException(code: nameof(ErrorResource.NotEmptyCollection));
}