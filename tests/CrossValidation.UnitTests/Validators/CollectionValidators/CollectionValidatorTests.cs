using CrossValidation.UnitTests.TestUtils.Builders;
using CrossValidation.UnitTests.TestUtils.Models;
using CrossValidation.Validations;
using FluentAssertions;
using Xunit;

namespace CrossValidation.UnitTests.Validators.CollectionValidators;

public class CollectionValidatorTests
{
    private ParentModel _model;

    public CollectionValidatorTests()
    {
        _model = new ParentModelBuilder().Build();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Calculate_total_items(
        bool isCollection,
        bool isEnumerable)
    {
        IEnumerable<int>? fieldValue = null;

        if (isCollection)
        {
            fieldValue = _model.IntList;
        }
        else if (isEnumerable)
        {
            fieldValue = _model.IntList
                .Select(x => x);
        }
        
        var action = () => Validate.Field(fieldValue!)
            .MinimumItems(_model.IntList.Count);

        action.Should()
            .NotThrow();
    }
}