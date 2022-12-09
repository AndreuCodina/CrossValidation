using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CrossValidation.Utils;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests.Utils;

public class PathExpressionVisitorTests
{
    [Fact]
    public void Field_name_and_full_path_are_equal_with_one_level()
    {
        var expectedFieldName = nameof(FirstLevelModel.Property);
        var expectedFieldFullPath = nameof(FirstLevelModel.Property);

        var expression = CreateExpressionFromDelegate<FirstLevelModel, int>(model =>
            model.Property);

        var visitor = PathExpressionVisitor.Create(expression);
        
        visitor.FieldName.ShouldBe(expectedFieldName);
        visitor.FieldFullPath.ShouldBe(expectedFieldFullPath);
    }
    
    [Fact]
    public void Field_name_is_part_of_full_path_with_multilevel()
    {
        var expectedFieldName = nameof(FirstLevelModel.SecondLevel.ThirdLevel.Property);
        var expectedFieldFullPath = string.Join('.', new List<string>
        {
            nameof(FirstLevelModel.SecondLevel),
            nameof(FirstLevelModel.SecondLevel.ThirdLevel),
            nameof(FirstLevelModel.SecondLevel.ThirdLevel.Property),
        });

        var expression = CreateExpressionFromDelegate<FirstLevelModel, int>(model =>
            model.SecondLevel.ThirdLevel.Property);

        var visitor = PathExpressionVisitor.Create(expression);
        
        visitor.FieldName.ShouldBe(expectedFieldName);
        visitor.FieldFullPath.ShouldBe(expectedFieldFullPath);
        visitor.FieldFullPath.ShouldContain(visitor.FieldName);
    }
    
    [Fact]
    public void Field_name_and_full_path_are_empty_without_field_selected()
    {
        var expectedFieldName = "";
        var expectedFieldFullPath = "";

        var expression = CreateExpressionFromDelegate<FirstLevelModel, FirstLevelModel>(model => model);

        var visitor = PathExpressionVisitor.Create(expression);
        
        visitor.FieldName.ShouldBe(expectedFieldName);
        visitor.FieldFullPath.ShouldBe(expectedFieldFullPath);
    }

    private Expression<Func<TModel, TField>> CreateExpressionFromDelegate<TModel, TField>(
        Expression<Func<TModel, TField>> expression)
    {
        return expression;
    }
}