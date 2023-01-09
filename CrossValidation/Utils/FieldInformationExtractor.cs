using System.Linq.Expressions;
using System.Reflection;
using CrossValidation.Results;
using static CrossValidation.Utils.FieldInformationExtractorError;

namespace CrossValidation.Utils;

public record FieldInformationExtractorError : Error
{
    public record CodeCallInFieldSelectorError : FieldInformationExtractorError;
}

public class FieldInformationExtractor<TField>
{
    public string SelectionFullPath { get; }
    public TField Value { get; }

    private FieldInformationExtractor(string selectionFullPath, TField value)
    {
        SelectionFullPath = selectionFullPath;
        Value = value;
    }

    public static FieldInformationExtractor<TField> Extract<TModel>(
        TModel model,
        Expression<Func<TModel, TField>> fieldSelector)
    {
        var fieldFullPath = PathExpressionVisitor.Create(fieldSelector.Body).FieldFullPath;
        TField fieldValue;

        if (fieldSelector.Body is MemberExpression memberExpression)
        {
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            var currentModel = GetModelRelatedToField(fieldFullPath, model);
            fieldValue = (TField)propertyInfo.GetValue(currentModel)!;
        }
        else if (fieldSelector.Body is MethodCallExpression)
        {
            throw new CodeCallInFieldSelectorError().ToException();
        }
        else
        {
            fieldValue = (TField)(object)model!;
        }

        return new FieldInformationExtractor<TField>(fieldFullPath, fieldValue);
    }

    private static object GetModelRelatedToField<TModel>(string fieldFullPath, TModel model)
    {
        var nodeNames = fieldFullPath.Split('.')
            .SkipLast(1) // Remove the field selected
            .ToList();
        var hasSomeNode = !(nodeNames.Count == 1 && nodeNames[0] == "");

        if (!hasSomeNode)
        {
            return model!;
        }

        object currentNodeModel = model!;
        var propertiesInCurrentNodeModel = model!.GetType().GetProperties();

        foreach (var node in nodeNames)
        {
            var propertyInfo = propertiesInCurrentNodeModel.First(x => x.Name == node);
            var childNodeModel = ConvertToChildField(propertyInfo, currentNodeModel!);
            currentNodeModel = childNodeModel;
        }

        return currentNodeModel!;
    }

    private static object ConvertToChildField(PropertyInfo propertyInfo, object parent)
    {
        var source = propertyInfo.GetValue(parent, null)!;
        var destination = Activator.CreateInstance(propertyInfo.PropertyType)!;

        foreach (var prop in destination.GetType().GetProperties().ToList())
        {
            var value = source.GetType().GetProperty(prop.Name)!.GetValue(source, null);
            prop.SetValue(destination, value, null);
        }

        return destination;
    }
}