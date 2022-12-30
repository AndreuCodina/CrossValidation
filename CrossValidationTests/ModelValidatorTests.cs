using System;
using System.Collections.Generic;
using System.Linq;
using CrossValidation;
using CrossValidation.Resources;
using CrossValidation.Results;
using CrossValidation.Rules;
using CrossValidationTests.Builders;
using CrossValidationTests.Fixtures;
using CrossValidationTests.Models;
using Shouldly;
using Xunit;

namespace CrossValidationTests;

public class ModelValidatorTests : IClassFixture<ModelValidatorFixture>
{
    private readonly ModelValidatorFixture _modelValidatorFixture;
    private ParentModel _model;

    public ModelValidatorTests(ModelValidatorFixture modelValidatorFixture)
    {
        _modelValidatorFixture = modelValidatorFixture;
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void One_rule_with_a_field_validator()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("Coupon1")
            .Build();
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        action.ShouldNotThrow();
    }

    [Fact]
    public void One_rule_fails_when_the_field_does_not_pass_the_validator()
    {
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
    }

    [Fact]
    public void One_rule_with_same_model_as_field()
    {
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x);
        });

        var action = () => parentModelValidator.Validate(_model);

        action.ShouldNotThrow();
    }

    [Fact]
    public void Set_field_information_to_the_error()
    {
        var expectedFieldName = "NestedModel.Int";
        var expectedFieldValue = _model.NestedModel.Int;
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.FieldName.ShouldBe(expectedFieldName);
        error.FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Set_field_information_to_the_error_when_using_a_child_model_validator()
    {
        var expectedFieldName = "NestedModel.Int";
        var expectedFieldValue = _model.NestedModel.Int;
        var nestedModelValidator = _modelValidatorFixture.CreateNestedModelValidator(validator =>
        {
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
        });
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidator);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.FieldName.ShouldBe(expectedFieldName);
        error.FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Set_field_information_to_the_error_when_using_several_rule_in_the_same_model_validator()
    {
        var expectedFieldName = "NullableString";
        string? expectedFieldValue = null;

        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .Must(_ => true);
            validator.RuleFor(x => x.NullableString)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.FieldName.ShouldBe(expectedFieldName);
        error.FieldValue.ShouldBe(expectedFieldValue);
    }
    
    [Fact]
    public void Stop_on_first_error()
    {
        var expectedCode = "ExpectedCode";
        var nestedModelValidator = _modelValidatorFixture.CreateNestedModelValidator(validator =>
        {
            validator.RuleFor(x => x.Int)
                .WithCode("UnexpectedCode2")
                .GreaterThan(_model.NestedModel.Int - 1)
                .WithCode("UnexpectedCode3")
                .GreaterThan(_model.NestedModel.Int - 1);

            validator.RuleFor(x => x.NestedNestedModel)
                .WithCode("UnexpectedCode4")
                .Must(_ => true)
                .WithCode(expectedCode)
                .Must(_ => false)
                .WithCode("UnexpectedCode5")
                .Must(_ => false);
        });
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithCode("UnexpectedCode1")
                .Null();
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidator)
                .WithCode("UnexpectedCode6")
                .Must(_ => true);
        });

        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrow<CrossValidationException>().Errors;
        errors.Count.ShouldBe(1);
        errors[0].Code.ShouldBe(expectedCode);
    }
    

    [Fact]
    public void Accumulate_errors_modifying_the_validation_mode()
    {
        var expectedCodes = new[]
        {
            "ErrorCode1", "ErrorCode2", "ErrorCode3", "ErrorCode4", "ErrorCode5"
        };

        var nestedModelValidator = _modelValidatorFixture.CreateNestedModelValidator(validator =>
        {
            validator.RuleFor(x => x.Int)
                .WithCode(expectedCodes[1])
                .GreaterThan(_model.NestedModel.Int)
                .WithCode("UnexpectedErrorCode")
                .GreaterThan(_model.NestedModel.Int);
            validator.RuleFor(x => x.NestedNestedModel)
                .WithCode("UnexpectedErrorCode")
                .Must(_ => true)
                .WithCode(expectedCodes[2])
                .Must(_ => false);
        });
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachRule;
            validator.RuleFor(x => x.NullableString)
                .WithCode(expectedCodes[0])
                .NotNull();
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidator)
                .WithCode(expectedCodes[3])
                .Must(_ => false);
            validator.RuleFor(x => x.NestedModel.Int)
                .WithCode(expectedCodes[4])
                .GreaterThan(_model.NestedModel.Int)
                .WithCode("UnexpectedErrorCode")
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrow<CrossValidationException>().Errors;
        errors.Select(x => x.Code).ShouldBe(expectedCodes);
    }

    [Fact]
    public void Validation_mode_cannot_be_changed_in_children_validators()
    {
        var nestedModelValidator = _modelValidatorFixture.CreateNestedModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachRule;
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
            validator.RuleFor(x => x.Int)
                .WithCode("UnexpectedErrorCode")
                .GreaterThan(_model.NestedModel.Int);
        });
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidator);
        });

        var action = () => parentModelValidator.Validate(_model);

        action.ShouldThrow<InvalidOperationException>();
    }
    
    [Fact]
    public void Transform_fails_when_the_field_does_not_pass_the_validator()
    {
        IEnumerable<string> TransformValues(IEnumerable<int> values)
        {
            return values.Select(x => x.ToString());
        }

        var nullableIntList = new List<int> { 1, 2, 3 };
        var expectedTransformation = TransformValues(nullableIntList);
        _model = new ParentModelBuilder()
            .WithNullableIntList(nullableIntList)
            .Build();
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateFirstErrorEachRule;
            
            validator.RuleFor(x => x.NullableIntList)
                .Transform(x => TransformValues(x!))
                .Null();
        });
    
        var action = () => parentModelValidator.Validate(_model);
    
        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.FieldValue.ShouldBe(expectedTransformation);
    }
    
    [Fact]
    public void Cannot_transform_same_model()
    {
        ParentModel ChangeValues(ParentModel model)
        {
            model.NullableString = "Nullable string";
            return model;
        }
        
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x)
                .Transform(x => ChangeValues(x!))
                .Null();
        });
    
        var action = () => parentModelValidator.Validate(_model);
    
        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.FieldName.ShouldBe("");
    }
    
    [Fact]
    public void Field_value_has_value_when_model_and_field_selected_match()
    {
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x)
                .Must(_ => false);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.FieldValue.ShouldNotBeNull();
    }
    
    [Fact]
    public void Field_value_has_value_when_model_and_field_selected_do_not_match()
    {
        var expectedFieldValue = _model.NestedModel.Int;
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Validator_keeps_message_customization()
    {
        var expectedMessage = "Error message";

        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithMessage(expectedMessage)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Validator_keeps_code_customization()
    {
        var expectedCode = "MyCode";
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithCode(expectedCode)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.Code.ShouldBe(expectedCode);
    }
    
    [Fact]
    public void Validator_keeps_error_customization()
    {
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithError(new CustomErrorWithCode("COD123"))
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.ShouldBeOfType<CustomErrorWithCode>();
    }
    
    [Fact]
    public void Validator_keeps_field_display_name_customization()
    {
        var expectedDisplayName = "My display name";
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithFieldDisplayName(expectedDisplayName)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.FieldDisplayName.ShouldBe(expectedDisplayName);
    }
    
    [Fact]
    public void Get_custom_error()
    {
        var comparisonValue = _model.NestedModel.Int + 1;
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .GreaterThan(comparisonValue);
        });
        
        var action = () => parentModelValidator.Validate(_model);
        
        var errors = action.ShouldThrow<CrossValidationException>().Errors;
        var error = errors[0].ShouldBeOfType<CommonCrossValidationError.GreaterThan<int>>();
        error.ComparisonValue.ShouldBe(comparisonValue);
        error.Code.ShouldBe("GreaterThan");
    }
    
    [Fact]
    public void CombineCustomizationWithCustomError()
    {
        var expectedMessage = "Expected message";
        var expectedError = new CustomErrorWithCode("COD123");
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithError(expectedError)
                .WithMessage(expectedMessage)
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);
        
        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.ShouldBeOfType<CustomErrorWithCode>();
        error.Code.ShouldBe(expectedError.Code);
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void CombineCustomErrorCodeWithCustomization()
    {
        var expectedMessage = "Expected message";
        var expectedError = new CustomErrorWithCode("COD123");
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithMessage(expectedMessage)
                .WithError(expectedError)
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);
        
        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.ShouldBeOfType<CustomErrorWithCode>();
        error.Code.ShouldBe(expectedError.Code);
        error.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Successful_validator_cleans_customization()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        var expectedMessage = "Error message";
        var expectedCode = "GreaterThan";
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableInt)
                .WithCode(new Bogus.Faker().Lorem.Word())
                .WithMessage(new Bogus.Faker().Lorem.Word())
                // .NotNull()
                // .WithMessage(expectedMessage)
                // .GreaterThan(_model.NestedModel.Int + 1);
                .NotNull()
                .WithMessage(expectedMessage)
                .GreaterThan(_model.NestedModel.Int + 1);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
        error.Code.ShouldBe(expectedCode);
        error.ShouldBeOfType<CommonCrossValidationError.GreaterThan<int>>();
    }
    
    [Fact]
    public void New_rule_overrides_field_display_name_customization()
    {
        _model = new ParentModelBuilder()
            .WithNullableInt(1)
            .Build();
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableInt)
                .WithFieldDisplayName("Field display name")
                .NotNull();

            validator.RuleFor(x => x.NullableString)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.FieldName.ShouldBe("NullableString");
        error.FieldDisplayName.ShouldBe("NullableString");
    }

    [Fact]
    public void Set_model_validator()
    {
        var expectedCode = "GreaterThan";
        var nestedModelValidator = _modelValidatorFixture.CreateNestedModelValidator(validator =>
        {
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
        });
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .WithMessage("Message to be cleaned")
                .SetModelValidator(nestedModelValidator);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.Code.ShouldBe(expectedCode);
    }

    [Fact]
    public void Discard_customizations_for_model_validator()
    {
        var expectedCode = "GreaterThan";
        var nestedModelValidator = _modelValidatorFixture.CreateNestedModelValidator(validator =>
        {
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
        });
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidator);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.Code.ShouldBe(expectedCode);
    }

    [Fact]
    public void Validator_conditional_execution()
    {
        var expectedErrorMessage = "TrueCase";
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> {100, 1, 2})
            .Build();
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .When(_ => false)
                .GreaterThan(_model.NestedModel.Int + 1)
                .When(x => x == _model.NestedModel.Int)
                .WithMessage(expectedErrorMessage)
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.Message.ShouldBe(expectedErrorMessage);
    }
    
    [Fact]
    public void Replace_default_placeholders()
    {
        var template = "{FieldDisplayName} is {FieldValue}";
        var expectedMessage = $"NullableString is ";
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithMessage(template)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Do_not_replace_placeholders_not_added()
    {
        var template = "{PlaceholderNotReplaced} is {FieldValue}";
        var expectedMessage = $"{{PlaceholderNotReplaced}} is {_model.NestedModel.Int}";
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .WithMessage(template)
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Replace_custom_error_placeholders()
    {
        var comparisonValue = _model.NestedModel.Int;
        var template = "{ComparisonValue} is not greater than {FieldValue}";
        var expectedMessage = $"{_model.NestedModel.Int} is not greater than {comparisonValue}";
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .WithMessage(template)
                .GreaterThan(comparisonValue);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Validate_predicate()
    {
        var parentModelValidator = _modelValidatorFixture.CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .Must(x => x.Int > _model.NestedModel.Int);
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<CrossValidationException>().Errors[0];
        error.ShouldBeOfType<CommonCrossValidationError.Predicate>();
    }

    

    public record CustomErrorWithCode(string Code) : CrossValidationError(Code: Code);
}