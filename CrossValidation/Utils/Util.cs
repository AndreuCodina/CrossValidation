using System.Linq.Expressions;
using System.Reflection;

namespace CrossValidation.Utils;

public static class Util
{
    public static FieldInformation<TField> GetFieldInformation<TModel, TField>(
        Expression<Func<TModel, TField?>> fieldSelector,
        TModel model)
    {
        var fieldFullPath = PathExpressionVisitor.Create(fieldSelector.Body).FieldFullPath;
        TField? fieldValue;
        bool isFieldSelectedDifferentThanModel;

        if (fieldSelector.Body is MemberExpression memberExpression)
        {
            isFieldSelectedDifferentThanModel = true;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            var currentModel = GetModelRelatedToField(fieldFullPath, model);
            fieldValue = (TField)propertyInfo.GetValue(currentModel)!;
        }
        else if (fieldSelector.Body is MethodCallExpression)
        {
            throw new InvalidOperationException("Cannot use a call in a field selector");
        }
        else
        {
            isFieldSelectedDifferentThanModel = false;
            fieldValue = (TField)(object)model!;
        }
        
        return new FieldInformation<TField>(
            SelectionFullPath: fieldFullPath,
            Value: fieldValue,
            IsFieldSelectedDifferentThanModel: isFieldSelectedDifferentThanModel);
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