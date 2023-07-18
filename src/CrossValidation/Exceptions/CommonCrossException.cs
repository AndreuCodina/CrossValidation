using CrossValidation.Resources;

namespace CrossValidation.Exceptions;

#pragma warning disable CS9113 // Parameter is unread.
public static class CommonCrossException
{
    public class NotNull() : BusinessException(code: nameof(ErrorResource.NotNull))
    {
        public override bool IsCommon => true;
    }

    public class Null() : BusinessException(code: nameof(ErrorResource.Null))
    {
        public override bool IsCommon => true;
    }

    public class GreaterThan<TField>(TField comparisonValue) :
        BusinessException(code: nameof(ErrorResource.GreaterThan))
    {
        public override bool IsCommon => true;
        public TField ComparisonValue => comparisonValue;
    }

    public class Enum() : BusinessException(code: nameof(ErrorResource.Enum))
    {
        public override bool IsCommon => true;
    }

    public class EnumRange() : BusinessException(code: nameof(ErrorResource.Enum))
    {
        public override bool IsCommon => true;
    }

    public class LengthRange(int minimum, int maximum, int totalLength) :
        LengthException(code: nameof(ErrorResource.LengthRange))
    {
        public override bool IsCommon => true;
        public override int TotalLength => totalLength;
    }

    public class MinimumLength(int minimum, int totalLength)
        : LengthException(code: nameof(ErrorResource.MinimumLength))
    {
        public override bool IsCommon => true;
        public override int TotalLength => totalLength;
    }

    public class Predicate() : BusinessException(code: nameof(ErrorResource.General))
    {
        public override bool IsCommon => true;
    }
    
    public class RegularExpression() : BusinessException(code: nameof(ErrorResource.RegularExpression))
    {
        public override bool IsCommon => true;
    }
    
    public class EmptyString() : BusinessException(code: nameof(ErrorResource.EmptyString))
    {
        public override bool IsCommon => true;
    }
    
    public class NotEmptyString() : BusinessException(code: nameof(ErrorResource.NotEmptyString))
    {
        public override bool IsCommon => true;
    }
    
    public class EmptyCollection() : BusinessException(code: nameof(ErrorResource.EmptyCollection))
    {
        public override bool IsCommon => true;
    }
    
    public class NotEmptyCollection() : BusinessException(code: nameof(ErrorResource.NotEmptyCollection))
    {
        public override bool IsCommon => true;
    }
}
#pragma warning restore CS9113 // Parameter is unread.