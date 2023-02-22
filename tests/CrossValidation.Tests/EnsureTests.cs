using System.Collections.Generic;
using CrossValidation.Exceptions;
using CrossValidation.Resources;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class EnsureTests : TestBase
{
    [Fact]
    public void Use_field_without_model()
    {
        int? field = null;
        
        var action = () => Ensure.Field(field)
            .NotNull();
        
        action.ShouldThrow<CrossArgumentException>();
    }
    
    [Fact]
    public void Use_index_in_field_name()
    {
        var list = new List<int?> {1, null, 2};

        var action = () => Ensure.Field(list)
            .ForEach(x => x
                .NotNull());

        var exception = action.ShouldThrow<CrossArgumentException>();
        exception.Message.ShouldBe($"list[1]: {ErrorResource.NotNull}");
    }
}