using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Exceptions;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.Builders;
using CrossValidation.Tests.Fixtures;
using CrossValidation.Tests.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests.Validations.ValidationExtensions;

public class ForEachTests : IClassFixture<CommonFixture>
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
    [InlineData(ValidationMode.StopValidationOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrorEachValidation)]
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

        var error = action.ShouldThrowValidationError();
        error.FieldName.ShouldBe("NullableIntList[1]");
    }
    
    [Theory]
    [InlineData(ValidationMode.StopValidationOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrorEachValidation)]
    [InlineData(ValidationMode.AccumulateFirstErrorEachValidationAndAllFirstErrorsCollectionIteration)]
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

        var error = action.ShouldThrowValidationError();
        error.FieldName.ShouldBe("NullableIntList[0]");
    }
    
    [Theory]
    [InlineData(ValidationMode.StopValidationOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrorEachValidation)]
    [InlineData(ValidationMode.AccumulateFirstErrorEachValidationAndAllFirstErrorsCollectionIteration)]
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

        var error = action.ShouldThrowValidationError();
        error.FieldName.ShouldBe("NullableIntList[1]");
    }
    
    [Fact]
    public void Collect_transformed_values()
    {
        string ApplyTransformation(int value) => value.ToString();
        int UnapplyTransformation(string value) => int.Parse(value);
        
        var stringList = Validate.That(_model.IntList)
            .ForEach(x => x
                .Transform(ApplyTransformation))
            .Instance();
        
        stringList.Select(UnapplyTransformation)
            .ShouldBe(_model.IntList);
    }
    
    [Theory]
    [InlineData(ValidationMode.StopValidationOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrorEachValidation)]
    [InlineData(ValidationMode.AccumulateFirstErrorEachValidationAndAllFirstErrorsCollectionIteration)]
    public void Field_keep_settings_with_error_accumulation(ValidationMode validationMode)
    {
        var expectedFieldDisplayName = "Expected field display name";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var expectedCode = "Expected code";
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

        var error = action.ShouldThrowValidationError();
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
    [InlineData(ValidationMode.StopValidationOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrorEachValidation)]
    [InlineData(ValidationMode.AccumulateFirstErrorEachValidationAndAllFirstErrorsCollectionIteration)]
    public void That_keeps_settings_with_error_accumulation(ValidationMode validationMode)
    {
        var expectedFieldDisplayName = "Expected field display name";
        var expectedHttpStatusCode = HttpStatusCode.Created;
        var expectedCode = "Expected code";
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

        var error = action.ShouldThrowValidationError();
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
    [InlineData(ValidationMode.StopValidationOnFirstError)]
    [InlineData(ValidationMode.AccumulateFirstErrorEachValidation)]
    [InlineData(ValidationMode.AccumulateFirstErrorEachValidationAndAllFirstErrorsCollectionIteration)]
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

        var error = action.ShouldThrowValidationError();
        error.Details.ShouldBeNull();
    }
    
    [Theory]
    [InlineData(
        ValidationMode.StopValidationOnFirstError,
        new[] {"1"})]
    [InlineData(
        ValidationMode.AccumulateFirstErrorEachValidation,
        new[] {"1", "3"})]
    [InlineData(
        ValidationMode.AccumulateFirstErrorEachValidationAndAllFirstErrorsCollectionIteration,
        new[] {"1", "2", "3"})]
    public void Throw_errors_with_model_validator(
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
        
        var action = () => parentModelValidator.Validate(_model);

        var exception = action.ShouldThrow<Exception>();

        if (exception is ValidationListException validationListException)
        {
            validationListException.Errors
                .Select(x => x.Code)
                .SequenceEqual(expectedErrorCodes);
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


        var error = action.ShouldThrowValidationError();
        error.Code.ShouldBe(expectedErrorCode);
    }

    private record ErrorTest : CrossError;
}