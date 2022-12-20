using System;
using System.Collections.Generic;
using System.Linq;
using CrossValidation;
using CrossValidation.Resources;
using CrossValidation.Results;
using CrossValidation.Rules;
using CrossValidationTests.Builders;
using CrossValidationTests.Models;
using Moq;
using Shouldly;
using Xunit;

namespace CrossValidationTests;

public class ModelValidatorTests
{
    private ParentModel _model;

    public ModelValidatorTests()
    {
        _model = new ParentModelBuilder().Build();
    }
    
    [Fact]
    public void One_rule_with_a_field_validator()
    {
        _model = new ParentModelBuilder()
            .WithNullableString("Coupon1")
            .Build();
        var parentModelValidator = CreateParentModelValidator(validator =>
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
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.Code.ShouldBe(nameof(ErrorResource.NotNull));
    }

    [Fact]
    public void One_rule_with_same_model_as_field()
    {
        var parentModelValidator = CreateParentModelValidator(validator =>
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
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.FieldName.ShouldBe(expectedFieldName);
        error.FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Set_field_information_to_the_error_when_using_a_child_model_validator()
    {
        var expectedFieldName = "NestedModel.Int";
        var expectedFieldValue = _model.NestedModel.Int;
        var nestedModelValidator = CreateNestedModelValidator(validator =>
        {
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
        });
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidator);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.FieldName.ShouldBe(expectedFieldName);
        error.FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Set_field_information_to_the_error_when_using_several_rule_in_the_same_model_validator()
    {
        var expectedFieldName = "NullableString";
        string? expectedFieldValue = null;

        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .NotNull();
            validator.RuleFor(x => x.NestedModel.Int)
                .NotNull();
            validator.RuleFor(x => x.NullableString)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.FieldName.ShouldBe(expectedFieldName);
        error.FieldValue.ShouldBe(expectedFieldValue);
    }
    
    [Fact]
    public void Stop_on_first_error()
    {
        var expectedCode = "ExpectedCode";
        var nestedModelValidator = CreateNestedModelValidator(validator =>
        {
            validator.RuleFor(x => x.Int)
                .WithCode("UnexpectedCode2")
                .GreaterThan(_model.NestedModel.Int - 1)
                .WithCode("UnexpectedCode3")
                .GreaterThan(_model.NestedModel.Int - 1);
            validator.RuleFor(x => x.NestedNestedModel)
                .WithCode("UnexpectedCode4")
                .NotNull()
                .WithCode(expectedCode)
                .Null()
                .WithCode("UnexpectedCode5")
                .Null();
        });
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithCode("UnexpectedCode1")
                .Null();
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidator)
                .WithCode("UnexpectedCode6")
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrow<ValidationException>().Errors;
        errors.Count.ShouldBe(1);
        errors[0].Code.ShouldBe(expectedCode);
    }
    

    [Fact]
    public void Accumulate_errors_modifying_the_validation_mode()
    {
        var expectedCodes = new[]
        {
            "ErrorCode1", "ErrorCode2", "ErrorCode3", "ErrorCode4", "ErrorCode5", "ErrorCode6", "ErrorCode7"
        };

        var nestedModelValidator = CreateNestedModelValidator(validator =>
        {
            validator.RuleFor(x => x.Int)
                .WithCode(expectedCodes[1])
                .GreaterThan(_model.NestedModel.Int)
                .WithCode(expectedCodes[2])
                .GreaterThan(_model.NestedModel.Int);
            validator.RuleFor(x => x.NestedNestedModel)
                .WithCode("UnexpectedErrorCode")
                .NotNull()
                .WithCode(expectedCodes[3])
                .Null();
        });
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            validator.RuleFor(x => x.NullableString)
                .WithCode(expectedCodes[0])
                .NotNull();
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidator)
                .WithCode(expectedCodes[4])
                .Null();
            validator.RuleFor(x => x.NestedModel.Int)
                .WithCode(expectedCodes[5])
                .GreaterThan(_model.NestedModel.Int)
                .WithCode(expectedCodes[6])
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrow<ValidationException>().Errors;
        errors.Select(x => x.Code).ShouldBe(expectedCodes);
    }

    [Fact]
    public void Validation_mode_cannot_be_changed_in_children_validators()
    {
        var nestedModelValidator = CreateNestedModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
            validator.RuleFor(x => x.Int)
                .WithCode("UnexpectedErrorCode")
                .GreaterThan(_model.NestedModel.Int);
        });
        var parentModelValidator = CreateParentModelValidator(validator =>
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
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            
            validator.RuleFor(x => x.NullableIntList)
                .Transform(x => TransformValues(x!))
                .Null();
        });
    
        var action = () => parentModelValidator.Validate(_model);
    
        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.FieldValue.ShouldBe(expectedTransformation);
    }
    
    [Fact]
    public void Field_value_is_null_when_model_and_field_selected_match()
    {
        object? expectedFieldValue = null;
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.FieldValue.ShouldBe(expectedFieldValue);
    }
    
    [Fact]
    public void Field_value_has_value_when_model_and_field_selected_do_not_match()
    {
        var expectedFieldValue = _model.NestedModel.Int;
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Validator_keeps_message_customization()
    {
        var expectedMessage = "Error message";

        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithMessage(expectedMessage)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Validator_keeps_code_customization()
    {
        var expectedCode = "MyCode";
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithCode(expectedCode)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.Code.ShouldBe(expectedCode);
    }
    
    [Fact]
    public void Validator_keeps_error_customization()
    {
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithError(new CustomErrorWithCode("COD123"))
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.ShouldBeOfType<CustomErrorWithCode>();
    }
    
    [Fact]
    public void Get_custom_error()
    {
        var comparisonValue = _model.NestedModel.Int + 1;
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .GreaterThan(comparisonValue);
        });
        
        var action = () => parentModelValidator.Validate(_model);
        
        var errors = action.ShouldThrow<ValidationException>().Errors;
        var error = errors[0].ShouldBeOfType<CommonCrossValidationError.GreaterThan<int>>();
        error.ComparisonValue.ShouldBe(comparisonValue);
        error.Code.ShouldBe("GreaterThan");
    }
    
    [Fact]
    public void CombineCustomizationWithCustomError()
    {
        var expectedMessage = "Expected message";
        var expectedError = new CustomErrorWithCode("COD123");
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithMessage(expectedMessage)
                .WithError(expectedError)
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);
        
        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.ShouldBeOfType<CustomErrorWithCode>();
        error.Code.ShouldBe(expectedError.Code);
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void CombineCustomErrorCodeWithCustomization()
    {
        var expectedMessage = "Expected message";
        var expectedError = new CustomErrorWithCode("COD123");
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithError(expectedError)
                .WithMessage(expectedMessage)
                .NotNull();
        });
        
        var action = () => parentModelValidator.Validate(_model);
        
        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.ShouldBeOfType<CustomErrorWithCode>();
        error.Code.ShouldBe(expectedError.Code);
        error.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Successful_validator_cleans_customizations()
    {
        var expectedMessage = "Error message";
        var expectedCode = "GreaterThan";
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .WithCode(new Bogus.Faker().Lorem.Word())
                .WithMessage(new Bogus.Faker().Lorem.Word())
                .NotNull()
                .WithMessage(expectedMessage)
                .GreaterThan(_model.NestedModel.Int + 1);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
        error.Code.ShouldBe(expectedCode);
        error.ShouldBeOfType<CommonCrossValidationError.GreaterThan<int>>();
    }

    [Fact]
    public void Set_model_validator()
    {
        var expectedCode = "GreaterThan";
        var nestedModelValidator = CreateNestedModelValidator(validator =>
        {
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
        });
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .WithMessage("Message to be cleaned")
                .SetModelValidator(nestedModelValidator);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.Code.ShouldBe(expectedCode);
    }

    [Fact]
    public void Discard_customizations_for_model_validator()
    {
        var expectedCode = "GreaterThan";
        var nestedModelValidator = CreateNestedModelValidator(validator =>
        {
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
        });
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidator);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.Code.ShouldBe(expectedCode);
    }
    
    [Fact]
    public void Execute_validators_for_all_item_collection()
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> { 100, 90, 80 })
            .Build();
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleForEach(x => x.NullableIntList!, x => x
                .GreaterThan(1)
                .GreaterThan(10));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Execute_validators_for_all_item_collection_fails()
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> { 100, 1, 90, 2 })
            .Build();
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            validator.RuleForEach(x => x.NullableIntList!, x => x
                .GreaterThan(0)
                .GreaterThan(10));
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrow<ValidationException>().Errors;
        errors.Count.ShouldBe(2);
        errors[0].FieldName.ShouldBe($"{nameof(_model.NullableIntList)}[1]");
        errors[1].FieldName.ShouldBe($"{nameof(_model.NullableIntList)}[3]");
    }
    
    [Fact]
    public void Index_is_represented_in_field_name_when_iterate_collection()
    {
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> {100, 1, 2})
            .Build();
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleForEach(x => x.NullableIntList!, x => x
                .GreaterThan(10));
        });

        var action = () => parentModelValidator.Validate(_model);

        var errors = action.ShouldThrow<ValidationException>().Errors;
        errors.Count.ShouldBe(1);
        errors[0].FieldName.ShouldBe($"{nameof(_model.NullableIntList)}[1]");
    }
    
    [Fact]
    public void Validator_conditional_execution()
    {
        var expectedErrorMessage = "TrueCase";
        _model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> {100, 1, 2})
            .Build();
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .When(x => false)
                .GreaterThan(_model.NestedModel.Int + 1)
                .When(x => x.NestedModel.Int == _model.NestedModel.Int)
                .WithMessage(expectedErrorMessage)
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.Message.ShouldBe(expectedErrorMessage);
    }
    
    [Fact]
    public void Replace_default_placeholders()
    {
        var template = "{FieldDisplayName} is {FieldValue}";
        var expectedMessage = $"NullableString is ";
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithMessage(template)
                .NotNull();
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Do_not_replace_placeholders_not_added()
    {
        var template = "{PlaceholderNotReplaced} is {FieldValue}";
        var expectedMessage = $"{{PlaceholderNotReplaced}} is {_model.NestedModel.Int}";
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .WithMessage(template)
                .GreaterThan(_model.NestedModel.Int);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Replace_custom_error_placeholders()
    {
        var comparisonValue = _model.NestedModel.Int;
        var template = "{ComparisonValue} is not greater than {FieldValue}";
        var expectedMessage = $"{_model.NestedModel.Int} is not greater than {comparisonValue}";
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .WithMessage(template)
                .GreaterThan(comparisonValue);
        });

        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Validate_predicate()
    {
        var parentModelValidator = CreateParentModelValidator(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .Must(x => x.NestedModel.Int > _model.NestedModel.Int);
        });
        
        var action = () => parentModelValidator.Validate(_model);

        var error = action.ShouldThrow<ValidationException>().Errors[0];
        error.ShouldBeOfType<CommonCrossValidationError.Predicate>();
    }

    private ParentModelValidator CreateParentModelValidator(Action<ParentModelValidator> validator)
    {
        var validatorMock = new Mock<ParentModelValidator>()
        {
            CallBase = true
        };
        validatorMock.Setup(x => x.CreateRules())
            .Callback(() => { validator(validatorMock.Object); });
        return validatorMock.Object;
    }

    private NestedModelValidator CreateNestedModelValidator(
        Action<NestedModelValidator> validator)
    {
        var validatorMock = new Mock<NestedModelValidator>()
        {
            CallBase = true
        };
        validatorMock.Setup(x => x.CreateRules())
            .Callback(() => { validator(validatorMock.Object); });
        return validatorMock.Object;
    }

    public class ParentModelValidator : ModelValidator<ParentModel>
    {
        public override void CreateRules()
        {
        }
    }

    public class NestedModelValidator : ModelValidator<NestedModel>
    {
        public override void CreateRules()
        {
        }
    }

    public record CustomErrorWithCode(string Code) : CrossValidationError(Code: Code);
}