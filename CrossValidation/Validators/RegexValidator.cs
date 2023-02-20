using System.Text.RegularExpressions;
using CrossValidation.Errors;

namespace CrossValidation.Validators;

public record RegularExpressionValidator(string FieldValue, string Pattern) : Validator
{
    public override bool IsValid()
    {
        return Regex.IsMatch(FieldValue, Pattern, RegexOptions.None, TimeSpan.FromSeconds(2.0));
    }

    public override CrossError CreateError()
    {
        return new CommonCrossError.RegularExpression();
    }
}