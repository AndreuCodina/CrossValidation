using CrossValidation.Exceptions;
using CrossValidation.Resources;

namespace CrossValidation.Errors;

#pragma warning disable CS9113 // Parameter is unread.
public static class CommonCrossError
{
    public class NotNull() : BusinessException(nameof(ErrorResource.NotNull))
    {
        public override bool IsCommon => true;
    }

    public class Null() : BusinessException(nameof(ErrorResource.Null))
    {
        public override bool IsCommon => true;
    }

    public class GreaterThan<TField>(TField comparisonValue) :
        BusinessException(nameof(ErrorResource.GreaterThan))
    {
        public override bool IsCommon => true;
        public TField ComparisonValue => comparisonValue;
    }

    public class Enum() : BusinessException(nameof(ErrorResource.Enum))
    {
        public override bool IsCommon => true;
    }

    public class EnumRange() : BusinessException(nameof(ErrorResource.Enum))
    {
        public override bool IsCommon => true;
    }

    public class LengthRange(int minimum, int maximum, int totalLength) :
        LengthException(nameof(ErrorResource.LengthRange))
    {
        public override bool IsCommon => true;
        public override int TotalLength => totalLength;
    }

    public class MinimumLength(int minimum, int totalLength)
        : LengthException(nameof(ErrorResource.MinimumLength))
    {
        public override bool IsCommon => true;
        public override int TotalLength => totalLength;
    }

    public class Predicate() : BusinessException(nameof(ErrorResource.General))
    {
        public override bool IsCommon => true;
    }
    
    public class RegularExpression() : BusinessException(nameof(ErrorResource.RegularExpression))
    {
        public override bool IsCommon => true;
    }
    
    public class EmptyString() : BusinessException(nameof(ErrorResource.EmptyString))
    {
        public override bool IsCommon => true;
    }
    
    public class NotEmptyString() : BusinessException(nameof(ErrorResource.NotEmptyString))
    {
        public override bool IsCommon => true;
    }
    
    public class EmptyCollection() : BusinessException(nameof(ErrorResource.EmptyCollection))
    {
        public override bool IsCommon => true;
    }
    
    public class NotEmptyCollection() : BusinessException(nameof(ErrorResource.NotEmptyCollection))
    {
        public override bool IsCommon => true;
    }
}
#pragma warning restore CS9113 // Parameter is unread.