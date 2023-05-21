using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class ForEachTests :
    TestBase,
    IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;

    public ForEachTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void Execute_validators_for_all_item_collection()
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> { 100, 90, 80 })
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableIntList)
                .NotNull()
                .ForEach(x => x
                    .GreaterThan(1)
                    .GreaterThan(10));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        action.ShouldNotThrow();
    }
    
    [Theory]
    [InlineData(ValidationMode.StopOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrors)]
    public void Execute_validators_for_all_item_collection_fails(ValidationMode validationMode)
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> { 100, 1, 90, 2 })
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = validationMode;
            
            validator.Field(_model.NullableIntList)
                .NotNull()
                .ForEach(x => x
                    .GreaterThan(0)
                    .GreaterThan(10));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldName.ShouldBe("NullableIntList[1]");
    }
    
    [Theory]
    [InlineData(ValidationMode.StopOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrors)]
    [InlineData(ValidationMode.AccumulateFirstErrorsAndAllIterationFirstErrors)]
    public void Keep_field_name_when_the_field_is_transformed_in_a_collection(ValidationMode validationMode)
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> { 1 })
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = validationMode;
            
            validator.Field(_model.NullableIntList)
                .NotNull()
                .ForEach(x => x
                    .Transform(Convert.ToDouble)
                    .GreaterThan(10d));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldName.ShouldBe("NullableIntList[0]");
    }
    
    [Theory]
    [InlineData(ValidationMode.StopOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrors)]
    [InlineData(ValidationMode.AccumulateFirstErrorsAndAllIterationFirstErrors)]
    public void Index_is_represented_in_field_name_when_iterate_collection(ValidationMode validationMode)
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> {100, 1, 20})
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = validationMode;
            
            validator.Field(_model.NullableIntList)
                .NotNull()
                .ForEach(x => x
                    .GreaterThan(10));
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldName.ShouldBe("NullableIntList[1]");
    }
    
    [Fact]
    public void Collect_transformed_instance()
    {
        string ApplyTransformation(int value) => value.ToString();
        int UnapplyTransformation(string value) => int.Parse(value);
        
        var stringList = Validate.That(_model.IntList)
            .Transform(x => x.Select(ApplyTransformation))
            .Instance();
        
        stringList.Select(UnapplyTransformation)
            .ShouldBe(_model.IntList);
    }
    
    [Theory]
    [InlineData(ValidationMode.StopOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrors)]
    [InlineData(ValidationMode.AccumulateFirstErrorsAndAllIterationFirstErrors)]
    public void Field_keep_settings_with_error_accumulation(ValidationMode validationMode)
    {
        var expectedFieldDisplayName = "Expected field display name";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var expectedCode = "ExpectedCode";
        var expectedMessage = "Expected message";
        var expectedDetails = "Expected details";
        var intList = new List<int> {30, 20, 10};
        _model = new ParentModelBuilder()
            .WithIntList(intList)
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = validationMode;
            
            validator.Field(_model.IntList)
                .ForEach(x => x
                    .WithFieldDisplayName("Not expected field display name")
                    .Must(_commonFixture.BeValid)
                    .WithFieldDisplayName(expectedFieldDisplayName)
                    .WithCode(expectedCode)
                    .WithMessage(expectedMessage)
                    .WithError(new ErrorTest())
                    .WithHttpStatusCode(HttpStatusCode.Created)
                    .WithDetails(expectedDetails)
                    .GreaterThan(intList[2]));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldName.ShouldBe("IntList[2]");
        error.FieldDisplayName.ShouldBe(expectedFieldDisplayName);
        error.ShouldBeOfType<ErrorTest>();
        error.Code.ShouldBe(expectedCode);
        error.Message.ShouldBe(expectedMessage);
        error.Details.ShouldBe(expectedDetails);
        error.PlaceholderValues.ShouldNotBeEmpty();
        error.HttpStatusCode.ShouldBe(expectedHttpStatusCode);
    }
    
    [Theory]
    [InlineData(ValidationMode.StopOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrors)]
    [InlineData(ValidationMode.AccumulateFirstErrorsAndAllIterationFirstErrors)]
    public void That_keeps_customizations_with_error_accumulation(ValidationMode validationMode)
    {
        var expectedFieldDisplayName = "Expected field display name";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var expectedCode = "ExpectedCode";
        var expectedMessage = "Expected message";
        var expectedDetails = "Expected details";
        var intList = new List<int> {30, 20, 10};
        _model = new ParentModelBuilder()
            .WithIntList(intList)
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = validationMode;
            
            validator.That(_model.IntList)
                .ForEach(x => x
                    .WithFieldDisplayName(expectedFieldDisplayName)
                    .WithCode(expectedCode)
                    .WithMessage(expectedMessage)
                    .WithError(new ErrorTest())
                    .WithHttpStatusCode(HttpStatusCode.Created)
                    .WithDetails(expectedDetails)
                    .GreaterThan(intList[2]));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldName.ShouldBeNull();
        error.FieldDisplayName.ShouldBe(expectedFieldDisplayName);
        error.ShouldBeOfType<ErrorTest>();
        error.Code.ShouldBe(expectedCode);
        error.Message.ShouldBe(expectedMessage);
        error.Details.ShouldBe(expectedDetails);
        error.PlaceholderValues.ShouldNotBeEmpty();
        error.HttpStatusCode.ShouldBe(expectedHttpStatusCode);
    }
    
    [Theory]
    [InlineData(ValidationMode.StopOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrors)]
    [InlineData(ValidationMode.AccumulateFirstErrorsAndAllIterationFirstErrors)]
    public void Do_not_take_previous_customizations(ValidationMode validationMode)
    {
        var intList = new List<int> {1};
        _model = new ParentModelBuilder()
            .WithIntList(intList)
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = validationMode;
            
            validator.That(_model.IntList)
                .WithDetails("Unexpected details")
                .ForEach(x => x
                    .GreaterThan(intList[0]));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.Details.ShouldBeNull();
    }
    
    [Theory]
    [InlineData(
        ValidationMode.StopOnFirstError,
        new[] {"1"})]
    [InlineData(
        ValidationMode.AccumulateFirstErrors,
        new[] {"1", "3"})]
    [InlineData(
        ValidationMode.AccumulateFirstErrorsAndAllIterationFirstErrors,
        new[] {"1", "2", "3"})]
    public async Task Throw_errors_with_model_validator(
        ValidationMode validationMode,
        string[] expectedErrorCodes)
    {
        var intList = new List<int> {100, 20, 30, 100};
        _model = new ParentModelBuilder()
            .WithNullableIntList(intList)
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = validationMode;
            
            validator.Field(_model.NullableIntList)
                .MustAsync(_commonFixture.BeValidAsync)
                .NotNull()
                .ForEach(x => x
                    .GreaterThan(0)
                    .WithCode("1")
                    .GreaterThan(intList[1])
                    .WithCode("2")
                    .GreaterThan(intList[2]))
                .Must(_commonFixture.NotBeValid);

            validator.That(_model)
                .WithCode("3")
                .Must(_commonFixture.NotBeValid);
        });
        
        var action = () => parentModelValidator.ValidateAsync(_model);

        var exception = await action.ShouldThrowAsync<Exception>();

        if (exception is ValidationListException validationListException)
        {
            validationListException.Errors
                .Select(x => x.Code)
                .SequenceEqual(expectedErrorCodes)
                .ShouldBeTrue();
        }
        else if (exception is CrossException crossException)
        {
            crossException.Error
                .Code
                .ShouldBe(expectedErrorCodes[0]);
        }
        else
        {
            throw new Exception("Unexpected exception type");
        }
    }
    
    [Fact]
    public void Throw_errors_with_inline_validation()
    {
        var expectedErrorCode = "1";
        var intList = new List<int> {100, 20, 30, 100};
        _model = new ParentModelBuilder()
            .WithNullableIntList(intList)
            .Build();

        var action = () => Validate.Field(_model.IntList)
            .NotNull()
            .ForEach(x => x
                .GreaterThan(0)
                .WithCode(expectedErrorCode)
                .GreaterThan(intList[1])
                .WithCode("2")
                .GreaterThan(intList[2]))
            .Must(_commonFixture.NotBeValid);


        var error = action.ShouldThrowCrossError();
        error.Code.ShouldBe(expectedErrorCode);
    }
    
    [Fact]
    public void Nested_ForEach_with_AccumulateFirstErrorsAndAllIterationFirstErrors_validation_mode_returns_errors()
    {
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorsAndAllIterationFirstErrors;

            validator.Field(_model.IntListList)
                .ForEach(x => x
                    .ForEach(x => x
                        .Must(x => x >= 2)));
        });
        var action = () => parentModelValidator.Validate(_model);
        
        var errors = action.ShouldThrowCrossErrors();
        errors.Count.ShouldBe(3);
    }
    
    [Fact]
    public async Task Accumulate_operations()
    {
        var expectedIntList = new List<int> { 1, 2, 3 };
        var expectedMessage = "Expected message";
        _model = new ParentModelBuilder()
            .WithIntList(expectedIntList)
            .Build();
        var action = () => Validate.That(_model.IntList)
            .ForEach(x => x
                .MustAsync(_commonFixture.BeValidAsync)
                .Must(_commonFixture.BeValid)
                .WithMessage(expectedMessage)
                .MustAsync(_commonFixture.NotBeValidAsync)
                .Must(_commonFixture.ThrowException))
            .Must(_commonFixture.ThrowException)
            .ValidateAsync();

        var error = await action.ShouldThrowCrossErrorAsync<CommonCrossError.Predicate>();
        
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Theory]
    [InlineData(ValidationMode.StopOnFirstError, null)]
    [InlineData(ValidationMode.AccumulateFirstErrors, null)]
    [InlineData(ValidationMode.AccumulateFirstErrorsAndAllIterationFirstErrors, 2)]
    public void Synchronous_WhenNotNull_works_inside_ForEach(ValidationMode validationMode, int? numberOfErrors)
    {
        _model = new ParentModelBuilder()
            .WithIntList(new() { 1, 10, 1 })
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = validationMode;
            
            validator.That(_model.IntList)
                .ForEach(x => x
                    .Transform(x => (int?)x)
                    .WhenNotNull(x => x
                        .Must(x => x > 1)))
                .Must(_commonFixture.ThrowException);
        });
        var action = () => parentModelValidator.Validate(_model);

        if (validationMode is ValidationMode.StopOnFirstError or ValidationMode.AccumulateFirstErrors)
        {
            action.ShouldThrowCrossError();
        }
        else
        {
            var errors = action.ShouldThrowCrossErrors();
            errors.Count.ShouldBe(numberOfErrors!.Value);
        }
    }
    
    [Theory]
    [InlineData(ValidationMode.StopOnFirstError, null)]
    [InlineData(ValidationMode.AccumulateFirstErrors, null)]
    [InlineData(ValidationMode.AccumulateFirstErrorsAndAllIterationFirstErrors, 2)]
    public async Task Asynchronous_WhenNotNull_works_inside_ForEach(ValidationMode validationMode, int? numberOfErrors)
    {
        _model = new ParentModelBuilder()
            .WithIntList(new() { 1, 10, 1 })
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = validationMode;

            validator.That(_model.IntList)
                .ForEach(x => x
                    .MustAsync(_commonFixture.BeValidAsync)
                    .Transform(x => (int?)x)
                    .WhenNotNull(x => x
                        .MustAsync(_commonFixture.BeValidAsync)
                        .Must(x => x > 1)))
                .Must(_commonFixture.ThrowException);
        });
        var action = () => parentModelValidator.ValidateAsync(_model);

        if (validationMode is ValidationMode.StopOnFirstError or ValidationMode.AccumulateFirstErrors)
        {
            await action.ShouldThrowCrossErrorAsync();
        }
        else
        {
            var errors = await action.ShouldThrowCrossErrorsAsync();
            errors.Count.ShouldBe(numberOfErrors!.Value);
        }
    }
    
    [Fact]
    public void Get_field_name()
    {
        _model = new ParentModelBuilder()
            .WithIntListList(new()
            {
                new() { 1, 10, 1 },
                new() { 10 }

            })
            .Build();
        var nestedModel = _model.NestedModel;
        var nestedModelValidator = _commonFixture.CreateNestedModelValidator(validator =>
        {
            validator.Field(nestedModel.Int)
                .GreaterThan(nestedModel.Int);
        });
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorsAndAllIterationFirstErrors;

            validator.Field(_model.IntListList)
                .ForEach(x => x
                    .ForEach(x => x
                        .Must(x => x == 1)));
            
            validator.Field(_model.NestedModelList)
                .ForEach(x => x
                    .SetModelValidator(nestedModelValidator));
        });
        var action = () => parentModelValidator.Validate(_model);
        
        var errors = action.ShouldThrowCrossErrors();
        errors[0].FieldName.ShouldBe("IntListList[0][1]");
        errors[1].FieldName.ShouldBe("IntListList[1][0]");
        errors[2].FieldName.ShouldBe("NestedModelList[0].Int");
    }

    private record ErrorTest : CrossError;
}