using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CrossValidation.Errors;
using CrossValidation.Resources;
using CrossValidation.ShouldlyAssertions;
using CrossValidation.Tests.TestUtils;
using CrossValidation.Tests.TestUtils.Builders;
using CrossValidation.Tests.TestUtils.Fixtures;
using CrossValidation.Tests.TestUtils.Models;
using CrossValidation.Validations;
using Shouldly;
using Xunit;

namespace CrossValidation.Tests;

public class ModelValidatorTests :
    TestBase,
    IClassFixture<CommonFixture>
{
    private readonly CommonFixture _commonFixture;
    private ParentModel _model;
    private readonly NestedModel _nestedModel;

    public ModelValidatorTests(CommonFixture commonFixture)
    {
        _commonFixture = commonFixture;
        _model = new ParentModelBuilder().Build();
        _nestedModel = _model.NestedModel;
    }
    
    [Fact]
    public void One_validation_with_a_field_validator()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("Coupon1")
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableString)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        action.ShouldNotThrow();
    }

    [Fact]
    public void Validation_fails_when_the_field_does_not_pass_the_validator()
    {
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableString)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
    }

    [Fact]
    public void Set_field_information_to_the_error()
    {
        var expectedFieldName = "NestedModel.Int";
        var expectedFieldValue = _model.NestedModel.Int;
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NestedModel.Int)
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldName.ShouldBe(expectedFieldName);
        error.FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Set_field_information_to_the_error_when_using_a_child_model_validator()
    {
        var expectedFieldName = "NestedModel.Int";
        var expectedFieldValue = _model.NestedModel.Int;
        var nestedModelValidator = _commonFixture.CreateNestedModelValidator(validator =>
        {
            validator.Field(_nestedModel.Int)
                .GreaterThan(10);
        });
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NestedModel)
                .SetModelValidator(nestedModelValidator);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldName.ShouldBe(expectedFieldName);
        error.FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Set_field_information_to_the_error_when_using_several_validation_in_the_same_model_validator()
    {
        var expectedFieldName = "NullableString";
        string? expectedFieldValue = null;

        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NestedModel)
                .Must(_commonFixture.BeValid);
            validator.Field(_model.NullableString)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldName.ShouldBe(expectedFieldName);
        error.FieldValue.ShouldBe(expectedFieldValue);
    }
    
    [Fact]
    public void Stop_on_first_error()
    {
        var expectedCode = "ExpectedCode";
        var nestedModelValidator = _commonFixture.CreateNestedModelValidator(validator =>
        {
            validator.Field(_nestedModel.Int)
                .WithCode("UnexpectedCode2")
                .GreaterThan(_model.NestedModel.Int - 1)
                .WithCode("UnexpectedCode3")
                .GreaterThan(_model.NestedModel.Int - 1);

            validator.Field(_nestedModel.NestedNestedModel)
                .WithCode("UnexpectedCode4")
                .Must(_commonFixture.BeValid)
                .WithCode(expectedCode)
                .Must(_commonFixture.NotBeValid)
                .WithCode("UnexpectedCode5")
                .Must(_commonFixture.NotBeValid);
        });
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableString)
                .WithCode("UnexpectedCode1")
                .Null();
            validator.Field(_model.NestedModel)
                .SetModelValidator(nestedModelValidator)
                .WithCode("UnexpectedCode6")
                .Must(_commonFixture.BeValid);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.Code.ShouldBe(expectedCode);
    }
    

    [Fact]
    public void Accumulate_errors_modifying_the_validation_mode()
    {
        var expectedCodes = new[]
        {
            "ErrorCode1", "ErrorCode2", "ErrorCode3", "ErrorCode4", "ErrorCode5"
        };

        var nestedModelValidator = _commonFixture.CreateNestedModelValidator(validator =>
        {
            validator.Field(_nestedModel.Int)
                .WithCode(expectedCodes[1])
                .GreaterThan(_model.NestedModel.Int)
                .WithCode("UnexpectedErrorCode")
                .GreaterThan(_model.NestedModel.Int);
            
            validator.Field(_nestedModel.NestedNestedModel)
                .WithCode("UnexpectedErrorCode")
                .Must(_commonFixture.BeValid)
                .WithCode(expectedCodes[2])
                .Must(_commonFixture.NotBeValid);
        });
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachValidation;
            
            validator.Field(_model.NullableString)
                .WithCode(expectedCodes[0])
                .NotNull();
            
            validator.Field(_model.NestedModel)
                .SetModelValidator(nestedModelValidator)
                .WithCode(expectedCodes[3])
                .Must(_commonFixture.NotBeValid);
            
            validator.Field(_model.NestedModel.Int)
                .WithCode(expectedCodes[4])
                .GreaterThan(_model.NestedModel.Int)
                .WithCode("UnexpectedErrorCode")
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrowCrossErrors();
        errors.Select(x => x.Code).ShouldBe(expectedCodes);
    }

    [Fact]
    public void Validation_mode_cannot_be_changed_in_children_validators()
    {
        var nestedModelValidator = _commonFixture.CreateNestedModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachValidation;
            validator.Field(_nestedModel.Int)
                .GreaterThan(10);
            validator.Field(_nestedModel.Int)
                .WithCode("UnexpectedErrorCode")
                .GreaterThan(_model.NestedModel.Int);
        });
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NestedModel)
                .SetModelValidator(nestedModelValidator);
        });

        var action = () => parentModelValidator.Validate(_model);

        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Transform_after_apply_NotNull_to_nullable_field()
    {
        int StringToInt(string value)
        {
            return int.Parse(value);
        }

        string? nullableString = "1";
        var expectedTransformation = StringToInt(nullableString);
        _model = new ParentModelBuilder()
            .WithNullableString(nullableString)
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachValidation;

            validator.Field(_model.NullableString)
                .NotNull()
                .Transform(StringToInt)
                .GreaterThan(expectedTransformation);
        });
    
        var action = () => parentModelValidator.Validate(_model);
    
        var error = action.ShouldThrowCrossError();
        error.FieldValue.ShouldBe(expectedTransformation);
    }
    
    [Fact]
    public void Transform_field()
    {
        IEnumerable<string>? TransformValues(IEnumerable<int>? values)
        {
            return values?.Select(x => x.ToString());
        }

        var nullableIntList = new List<int> { 1, 2, 3 };
        var expectedTransformation = TransformValues(nullableIntList);
        _model = new ParentModelBuilder()
            .WithNullableIntList(nullableIntList)
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachValidation;

            validator.Field(_model.NullableIntList)
                .Transform(x => TransformValues(x!))
                .Null();
        });
    
        var action = () => parentModelValidator.Validate(_model);
    
        var error = action.ShouldThrowCrossError();
        error.FieldValue.ShouldBe(expectedTransformation);
    }
    
    [Fact]
    public void Transform_same_model()
    {
        ParentModel ChangeValue(ParentModel model)
        {
            model.NullableString = "Nullable string";
            return model;
        }
        
        var expectedTransformation = ChangeValue(_model);
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.That(_model)
                .Transform(ChangeValue)
                .Must(_commonFixture.NotBeValid);
        });
    
        var action = () => parentModelValidator.Validate(_model);
    
        var error = action.ShouldThrowCrossError();
        error.FieldName.ShouldBeNull();
        error.FieldValue.ShouldBe(expectedTransformation);
    }
    
    [Fact]
    public void Transform_invalid_validation()
    {
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachValidation;

            validator.Field(_model.NullableInt)
                .NotNull()
                .Transform(x => x + 1)
                .Transform(x => x.ToString())
                .LengthRange(0, 10)
                .Transform(int.Parse)
                .GreaterThan(int.MaxValue);
        });
    
        var action = () => parentModelValidator.Validate(_model);
    
        action.ShouldThrowCrossError<CommonCrossError.NotNull>();
    }
    
    [Fact]
    public void Field_fails_when_model_is_selected()
    {
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model)
                .Must(_commonFixture.NotBeValid);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ArgumentException>();
    }
    
    [Fact]
    public void Field_value_has_value_when_model_and_field_selected_do_not_match()
    {
        var expectedFieldValue = _model.NestedModel.Int;
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NestedModel.Int)
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Validator_keeps_message_customization()
    {
        var expectedMessage = "Error message";

        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableString)
                .WithMessage(expectedMessage)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Validator_keeps_code_customization()
    {
        var expectedCode = "MyCode";
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableString)
                .WithCode(expectedCode)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.Code.ShouldBe(expectedCode);
    }
    
    [Fact]
    public void Validator_keeps_error_customization()
    {
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableString)
                .WithError(new CustomErrorWithCode("COD123"))
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);

        action.ShouldThrowCrossError<CustomErrorWithCode>();
    }
    
    [Fact]
    public void Validator_keeps_field_display_name_customization()
    {
        var expectedDisplayName = "My display name";
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableString)
                .WithFieldDisplayName(expectedDisplayName)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldDisplayName.ShouldBe(expectedDisplayName);
    }
    
    [Fact]
    public void Get_custom_error()
    {
        var comparisonValue = _model.NestedModel.Int + 1;
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NestedModel.Int)
                .GreaterThan(comparisonValue);
        });
        
        var action = () => parentModelValidator.Validate(_model);
        
        var error = action.ShouldThrowCrossError<CommonCrossError.GreaterThan<int>>();
        error.ComparisonValue.ShouldBe(comparisonValue);
        error.Code.ShouldBe("GreaterThan");
    }
    
    [Fact]
    public void CombineCustomizationWithCustomError()
    {
        var expectedMessage = "Expected message";
        var expectedError = new CustomErrorWithCode("COD123");
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableString)
                .WithError(expectedError)
                .WithMessage(expectedMessage)
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);
        
        var error = action.ShouldThrowCrossError<CustomErrorWithCode>();
        error.Code.ShouldBe(expectedError.Code);
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void CombineCustomErrorCodeWithCustomization()
    {
        var expectedMessage = "Expected message";
        var expectedError = new CustomErrorWithCode("COD123");
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableString)
                .WithMessage(expectedMessage)
                .WithError(expectedError)
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);
        
        var error = action.ShouldThrowCrossError<CustomErrorWithCode>();
        error.Code.ShouldBe(expectedError.Code);
        error.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Successful_validator_cleans_customization()
    {
        int? nullableInt = 1;
        _model = new ParentModelBuilder()
            .WithNullableInt(nullableInt)
            .Build();
        var expectedMessage = "Expected message";
        var expectedCode = nameof(CommonCrossError.GreaterThan<int>);
        var expectedDetails = "Expected details";
        var expectedHttpStatusCode = HttpStatusCode.Accepted;
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableInt)
                .WithCode(new Bogus.Faker().Lorem.Word())
                .WithMessage(new Bogus.Faker().Lorem.Word())
                .WithDetails(new Bogus.Faker().Lorem.Word())
                .WithHttpStatusCode(HttpStatusCode.Created)
                .NotNull()
                .WithMessage(expectedMessage)
                .WithDetails(expectedDetails)
                .WithHttpStatusCode(expectedHttpStatusCode)
                .GreaterThan(nullableInt.Value);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError<CommonCrossError.GreaterThan<int>>();
        error.Message.ShouldBe(expectedMessage);
        error.Code.ShouldBe(expectedCode);
        error.Details.ShouldBe(expectedDetails);
        error.HttpStatusCode.ShouldBe(expectedHttpStatusCode);
    }
    
    [Fact]
    public void New_validation_overrides_field_display_name_customization()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableInt)
                .WithFieldDisplayName("Field display name")
                .NotNull();

            validator.Field(_model.NullableString)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldName.ShouldBe("NullableString");
        error.FieldDisplayName.ShouldBe("NullableString");
    }

    [Fact]
    public void Set_model_validator()
    {
        var expectedCode = "GreaterThan";
        var nestedModelValidator = _commonFixture.CreateNestedModelValidator(validator =>
        {
            validator.Field(_nestedModel.Int)
                .GreaterThan(10);
        });
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NestedModel)
                .WithMessage("Message to be cleaned")
                .SetModelValidator(nestedModelValidator);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.Code.ShouldBe(expectedCode);
    }

    [Fact]
    public void Discard_customizations_for_model_validator()
    {
        var expectedCode = "GreaterThan";
        var nestedModelValidator = _commonFixture.CreateNestedModelValidator(validator =>
        {
            validator.Field(_nestedModel.Int)
                .GreaterThan(10);
        });
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NestedModel)
                .SetModelValidator(nestedModelValidator);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.Code.ShouldBe(expectedCode);
    }

    [Fact]
    public void Replace_default_placeholders()
    {
        var template = "{FieldDisplayName} is {FieldValue}";
        var expectedMessage = $"NullableString is ";
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NullableString)
                .WithMessage(template)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Do_not_replace_placeholders_not_added()
    {
        var template = "{PlaceholderNotReplaced} is {FieldValue}";
        var expectedMessage = $"{{PlaceholderNotReplaced}} is {_model.NestedModel.Int}";
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NestedModel.Int)
                .WithMessage(template)
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Replace_custom_error_placeholders()
    {
        var comparisonValue = _model.NestedModel.Int;
        var template = "{ComparisonValue} is not greater than {FieldValue}";
        var expectedMessage = $"{_model.NestedModel.Int} is not greater than {comparisonValue}";
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NestedModel.Int)
                .WithMessage(template)
                .GreaterThan(comparisonValue);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Validate_predicate()
    {
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.Field(_model.NestedModel)
                .Must(x => x.Int > _model.NestedModel.Int);
        });
        
        var action = () => parentModelValidator.Validate(_model);

        action.ShouldThrowCrossError<CommonCrossError.Predicate>();
    }
    
    [Fact]
    public void Add_child_model_validator_errors_to_existing_ones()
    {
        var nestedModelValidator = _commonFixture.CreateNestedModelValidator(validator =>
        {
            validator.Field(_model.Int)
                .Must(_commonFixture.NotBeValid);
        });
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachValidation;

            validator.Field(_model.Int)
                .Must(_commonFixture.NotBeValid);
            
            validator.Field(_model.NestedModel)
                .SetModelValidator(nestedModelValidator);
            
            validator.Field(_model.Int)
                .Must(_commonFixture.NotBeValid);
        });

        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrowCrossErrors();
        errors.Count.ShouldBe(3);
    }
    
    [Fact]
    public void ValidateThat_returns_null_as_field_name()
    {
        var parentModelValidator = _commonFixture.CreateParentModelValidator(validator =>
        {
            validator.That(_model.NullableInt)
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrowCrossError();
        error.FieldName.ShouldBeNull();
    }

    private record CustomErrorWithCode(string Code) : CrossError(Code: Code);
}