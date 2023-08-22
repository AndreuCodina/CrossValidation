using CrossValidation.Resources;

namespace CrossValidation.Exceptions;

public static partial class CommonException
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
    
    public class GreaterThanOrEqualException<TField>(TField comparisonValue)
        : BusinessException(code: nameof(ErrorResource.GreaterThanOrEqual))
    {
        public TField ComparisonValue => comparisonValue;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(comparisonValue);
        }
    }
    
    public class LessThanException<TField>(TField comparisonValue)
        : BusinessException(code: nameof(ErrorResource.LessThan))
    {
        public TField ComparisonValue => comparisonValue;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(comparisonValue);
        }
    }
    
    public class LessThanOrEqualException<TField>(TField comparisonValue)
        : BusinessException(code: nameof(ErrorResource.LessThanOrEqual))
    {
        public TField ComparisonValue => comparisonValue;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(comparisonValue);
        }
    }
    
    public class EqualException<TField>(TField comparisonValue)
        : BusinessException(code: nameof(ErrorResource.Equal))
    {
        public TField ComparisonValue => comparisonValue;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(comparisonValue);
        }
    }
    
    public class NotEqualException<TField>(TField comparisonValue)
        : BusinessException(code: nameof(ErrorResource.NotEqual))
    {
        public TField ComparisonValue => comparisonValue;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(comparisonValue);
        }
    }

    public class EnumException() : BusinessException(code: nameof(ErrorResource.Enum));

    public class EnumRangeException() : BusinessException(code: nameof(ErrorResource.Enum));

    public class InclusiveLengthRangeException(int minimumLength, int maximumLength, int totalLength)
        : BusinessException(code: nameof(ErrorResource.InclusiveLengthRange))
    {
        public int MinimumLength => minimumLength;
        public int MaximumLength => maximumLength;
        public int TotalLength => totalLength;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(minimumLength);
            AddPlaceholderValue(maximumLength);
            AddPlaceholderValue(totalLength);
        }
    }
    
    public class ExclusiveLengthRangeException(int minimumLength, int maximumLength, int totalLength)
        : BusinessException(code: nameof(ErrorResource.ExclusiveLengthRange))
    {
        public int MinimumLength => minimumLength;
        public int MaximumLength => maximumLength;
        public int TotalLength => totalLength;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(minimumLength);
            AddPlaceholderValue(maximumLength);
            AddPlaceholderValue(totalLength);
        }
    }

    public class MinimumLengthException(int minimumLength, int totalLength)
        : BusinessException(code: nameof(ErrorResource.MinimumLength))
    {
        public int MinimumLength => minimumLength;
        public int TotalLength => totalLength;
        
        public override void AddParametersAsPlaceholderValues()
        {
            AddPlaceholderValue(minimumLength);
            AddPlaceholderValue(totalLength);
        }
    }
    
    public class MaximumLengthException(int maximumLength, int totalLength)
        : BusinessException(code: nameof(ErrorResource.MaximumLength))
    {
        public int MaximumLength => maximumLength;
        public int TotalLength => totalLength;
        
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