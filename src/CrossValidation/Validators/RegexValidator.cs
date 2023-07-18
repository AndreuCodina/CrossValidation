using System.Text.RegularExpressions;
using CrossValidation.Errors;
using CrossValidation.Exceptions;

namespace CrossValidation.Validators;

public class RegularExpressionValidator(string fieldValue, string pattern) : Validator
{
    public override bool IsValid()
    {
        return Regex.IsMatch(fieldValue, pattern, RegexOptions.None, TimeSpan.FromSeconds(2.0));
    }

    public override BusinessException CreateError()
    {
        return new CommonCrossError.RegularExpression();
    }
}