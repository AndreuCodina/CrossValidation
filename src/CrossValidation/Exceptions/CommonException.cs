using CrossValidation.Resources;

namespace CrossValidation.Exceptions;

public static class CommonException
{
    public class NotNullException() : BusinessException(code: nameof(ErrorResource.NotNull));

    public class NullException() : BusinessException(code: nameof(ErrorResource.Null));

    public class GreaterThanException<TField>(TField comparisonValue) :
        BusinessException(code: nameof(ErrorResource.GreaterThan))
    {
        public TField ComparisonValue => comparisonValue;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(comparisonValue);
        }
    }

    public class EnumException() : BusinessException(code: nameof(ErrorResource.Enum));

    public class EnumRangeException() : BusinessException(code: nameof(ErrorResource.Enum));

    public class LengthRangeException(int minimumLength, int maximumLength, int totalLength) :
        LengthException(code: nameof(ErrorResource.LengthRange))
    {
        public override int TotalLength => totalLength;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(minimumLength);
            AddPlaceholderValue(maximumLength);
            AddPlaceholderValue(totalLength);
        }
    }

    public class MinimumLengthException(int minimumLength, int totalLength)
        : LengthException(code: nameof(ErrorResource.MinimumLength))
    {
        public override int TotalLength => totalLength;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(minimumLength);
            AddPlaceholderValue(totalLength);
        }
    }
    
    public class MaximumLengthException(int maximumLength, int totalLength)
        : LengthException(code: nameof(ErrorResource.MaximumLength))
    {
        public override int TotalLength => totalLength;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(maximumLength);
            AddPlaceholderValue(totalLength);
        }
    }

    public class PredicateException() : BusinessException(code: nameof(ErrorResource.Generic));
    
    public class RegularExpressionException() : BusinessException(code: nameof(ErrorResource.RegularExpression));
    
    public class EmptyStringException() : BusinessException(code: nameof(ErrorResource.EmptyString));
    
    public class NotEmptyStringException() : BusinessException(code: nameof(ErrorResource.NotEmptyString));
    
    public class EmptyCollectionException() : BusinessException(code: nameof(ErrorResource.EmptyCollection));
    
    public class NotEmptyCollectionException() : BusinessException(code: nameof(ErrorResource.NotEmptyCollection));
    
    public class TrueBooleanException() : BusinessException(code: nameof(ErrorResource.Generic));
    
    public class FalseBooleanException() : BusinessException(code: nameof(ErrorResource.Generic));
}