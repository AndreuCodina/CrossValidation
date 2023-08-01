using System.Text.RegularExpressions;
using CrossValidation.Exceptions;

namespace CrossValidation.Validators.RegularExpressionValidators;

public class RegularExpressionValidator(string fieldValue, string pattern) : Validator
{
    public override bool IsValid()
    {
        return Regex.IsMatch(fieldValue, pattern, RegexOptions.None, TimeSpan.FromSeconds(2.0));
    }

    public override BusinessException CreateException()
    {
        return new CommonException.RegularExpressionException();
    }
}