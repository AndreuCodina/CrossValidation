using System.Globalization;

namespace CrossValidation.DependencyInjection;

public class UseCrossValidationOptionsBuilder
{
    internal string DefaultCulture { get; set; } = CultureInfo.CurrentCulture.Name;
    internal List<CultureInfo> SupportedCultures { get; set; } = new() {CultureInfo.CurrentCulture};
    
    public UseCrossValidationOptionsBuilder SetDefaultCulture(string culture)
    {
        DefaultCulture = culture;
        return this;
    }
    
    public UseCrossValidationOptionsBuilder SetSupportedCultures(params string[] cultures)
    {
        SupportedCultures = cultures.Select(x => new CultureInfo(x))
            .ToList();
        return this;
    }
}