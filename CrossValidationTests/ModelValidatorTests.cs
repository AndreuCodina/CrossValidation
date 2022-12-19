using System;
using System.Collections.Generic;
using System.Linq;
using CrossValidation;
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
    [Fact]
    public void One_rule_with_a_field_validator()
    {
        var model = new ParentModelBuilder()
            .WithNullableString("Coupon1")
            .Build();
        var validatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .NotNull();
        });

        var exception = Record.Exception(() => validatorMock.Object.Validate(model));

        exception.ShouldBeNull();
    }

    [Fact]
    public void One_rule_fails_when_the_field_does_not_pass_the_validator()
    {
        var model = new ParentModelBuilder().Build();
        var validatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .NotNull();
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Code.ShouldBe("NotNull");
    }

    [Fact]
    public void One_rule_with_same_model_as_field()
    {
        var model = new ParentModelBuilder().Build();
        var validatorMock = ParentModelValidatorMock(validator => { validator.RuleFor(x => x); });

        var exception = Record.Exception(() => validatorMock.Object.Validate(model));

        exception.ShouldBeNull();
    }

    [Fact]
    public void Set_field_information_to_the_error()
    {
        var model = new ParentModelBuilder().Build();
        var expectedFieldName = "NestedModel.Int";
        var expectedFieldValue = model.NestedModel.Int;
        var validatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .GreaterThan(model.NestedModel.Int + 1);
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().FieldName.ShouldBe(expectedFieldName);
        exception.Errors.First().FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Set_field_information_to_the_error_when_using_a_child_model_validator()
    {
        var model = new ParentModelBuilder().Build();
        var expectedFieldName = "NestedModel.Int";
        var expectedFieldValue = model.NestedModel.Int;
        var nestedModelValidatorMock = NestedModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
        });
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidatorMock.Object);
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().FieldName.ShouldBe(expectedFieldName);
        exception.Errors.First().FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Set_field_information_to_the_error_when_using_several_rule_in_the_same_model_validator()
    {
        var expectedFieldName = "NullableString";
        string? expectedFieldValue = null;
        var model = new ParentModelBuilder().Build();
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .NotNull();
            validator.RuleFor(x => x.NestedModel.Int)
                .NotNull();
            validator.RuleFor(x => x.NullableString)
                .NotNull();
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().FieldName.ShouldBe(expectedFieldName);
        exception.Errors.First().FieldValue.ShouldBe(expectedFieldValue);
    }
    
    [Fact]
    public void Stop_on_first_error()
    {
        var expectedCode = "ExpectedCode";
        var model = new ParentModelBuilder().Build();
        var nestedModelValidatorMock = NestedModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Int)
                .WithCode("UnexpectedCode2")
                .GreaterThan(model.NestedModel.Int - 1)
                .WithCode("UnexpectedCode3")
                .GreaterThan(model.NestedModel.Int - 1);
            validator.RuleFor(x => x.NestedNestedModel)
                .WithCode("UnexpectedCode4")
                .NotNull()
                .WithCode(expectedCode)
                .Null()
                .WithCode("UnexpectedCode5")
                .Null();
        });
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithCode("UnexpectedCode1")
                .Null();
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidatorMock.Object)
                .WithCode("UnexpectedCode6")
                .NotNull();
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.Count().ShouldBe(1);
        exception.Errors.First().Code.ShouldBe(expectedCode);
    }
    

    [Fact]
    public void Accumulate_errors_modifying_the_validation_mode()
    {
        var expectedCodes = new[]
        {
            "ErrorCode1", "ErrorCode2", "ErrorCode3", "ErrorCode4", "ErrorCode5", "ErrorCode6", "ErrorCode7"
        };
        var model = new ParentModelBuilder().Build();
        var nestedModelValidatorMock = NestedModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Int)
                .WithCode(expectedCodes[1])
                .GreaterThan(model.NestedModel.Int + 1)
                .WithCode(expectedCodes[2])
                .GreaterThan(model.NestedModel.Int + 1);
            validator.RuleFor(x => x.NestedNestedModel)
                .WithCode("UnexpectedErrorCode")
                .NotNull()
                .WithCode(expectedCodes[3])
                .Null();
        });
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            validator.RuleFor(x => x.NullableString)
                .WithCode(expectedCodes[0])
                .NotNull();
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidatorMock.Object)
                .WithCode(expectedCodes[4])
                .Null();
            validator.RuleFor(x => x.NestedModel.Int)
                .WithCode(expectedCodes[5])
                .GreaterThan(model.NestedModel.Int + 1)
                .WithCode(expectedCodes[6])
                .GreaterThan(model.NestedModel.Int + 1);
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.Select(x => x.Code).ShouldBe(expectedCodes);
    }

    [Fact]
    public void Validation_mode_cannot_be_changed_in_children_validators()
    {
        var model = new ParentModelBuilder().Build();
        var nestedModelValidatorMock = NestedModelValidatorMock(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
            validator.RuleFor(x => x.Int)
                .WithCode("UnexpectedErrorCode")
                .GreaterThan(model.NestedModel.Int + 1);
        });
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(nestedModelValidatorMock.Object);
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

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
        var model = new ParentModelBuilder()
            .WithNullableIntList(nullableIntList)
            .Build();
        var validatorMock = ParentModelValidatorMock(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            
            validator.RuleFor(x => x.NullableIntList)
                .Transform(x => TransformValues(x!))
                .Null();
        });
    
        var action = () => validatorMock.Object.Validate(model);
    
        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().FieldValue.ShouldBe(expectedTransformation);
    }
    
    [Fact]
    public void Field_value_is_null_when_model_and_field_selected_match()
    {
        object? expectedFieldValue = null;
        var model = new ParentModelBuilder().Build();
        var validatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x)
                .NotNull();
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().FieldValue.ShouldBe(expectedFieldValue);
    }
    
    [Fact]
    public void Field_value_has_value_when_model_and_field_selected_do_not_match()
    {
        var model = new ParentModelBuilder().Build();
        var expectedFieldValue = model.NestedModel.Int;
        var validatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .GreaterThan(model.NestedModel.Int + 1);
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Validator_keeps_message_customization()
    {
        var expectedMessage = "Error message";
        var model = new ParentModelBuilder().Build();
        var validatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithMessage(expectedMessage)
                .NotNull();
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Validator_keeps_code_customization()
    {
        var expectedCode = "MyCode";
        var model = new ParentModelBuilder().Build();
        var validatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithCode(expectedCode)
                .NotNull();
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Code.ShouldBe(expectedCode);
    }
    
    [Fact]
    public void Validator_keeps_error_customization()
    {
        var expectedError = new CustomErrorWithCode("COD123");
        var model = new ParentModelBuilder().Build();
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithError(expectedError)
                .NotNull();
        });
        
        var action = () => parentModelValidatorMock.Object.Validate(model);
        
        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().ShouldBeOfType<CustomErrorWithCode>();
    }
    
    [Fact]
    public void Get_custom_error()
    {
        var model = new ParentModelBuilder().Build();
        var comparisonValue = model.NestedModel.Int + 1;
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .GreaterThan(comparisonValue);
        });
        
        var action = () => parentModelValidatorMock.Object.Validate(model);
        
        var exception = action.ShouldThrow<ValidationException>();
        var error = exception.Errors.First().ShouldBeOfType<CommonValidationError.GreaterThan<int>>();
        error.ComparisonValue.ShouldBe(comparisonValue);
        error.Code.ShouldBe("GreaterThan");
    }
    
    [Fact]
    public void CombineCustomizationWithCustomError()
    {
        var expectedMessage = "Expected message";
        var expectedError = new CustomErrorWithCode("COD123");
        var model = new ParentModelBuilder().Build();
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithMessage(expectedMessage)
                .WithError(expectedError)
                .NotNull();
        });
        
        var action = () => parentModelValidatorMock.Object.Validate(model);
        
        var exception = action.ShouldThrow<ValidationException>();
        var error = exception.Errors.First().ShouldBeOfType<CustomErrorWithCode>();
        error.Code.ShouldBe(expectedError.Code);
        error.Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void CombineCustomErrorCodeWithCustomization()
    {
        var expectedMessage = "Expected message";
        var expectedError = new CustomErrorWithCode("COD123");
        var model = new ParentModelBuilder().Build();
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithError(expectedError)
                .WithMessage(expectedMessage)
                .NotNull();
        });
        
        var action = () => parentModelValidatorMock.Object.Validate(model);
        
        var exception = action.ShouldThrow<ValidationException>();
        var error = exception.Errors.First().ShouldBeOfType<CustomErrorWithCode>();
        error.Code.ShouldBe(expectedError.Code);
        error.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Successful_validator_cleans_customizations()
    {
        var expectedMessage = "Error message";
        var expectedCode = "GreaterThan";
        var model = new ParentModelBuilder().Build();
        var validatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .WithCode(new Bogus.Faker().Lorem.Word())
                .WithMessage(new Bogus.Faker().Lorem.Word())
                .NotNull()
                .WithMessage(expectedMessage)
                .GreaterThan(model.NestedModel.Int + 1);
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Message.ShouldBe(expectedMessage);
        exception.Errors.First().Code.ShouldBe(expectedCode);
        exception.Errors.First().ShouldBeOfType<CommonValidationError.GreaterThan<int>>();
    }

    [Fact]
    public void Set_model_validator()
    {
        var expectedCode = "GreaterThan";
        var model = new ParentModelBuilder().Build();
        var nestedModelValidatorMock = NestedModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
        });
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .WithMessage("Message to be cleaned")
                .SetModelValidator(nestedModelValidatorMock.Object);
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Code.ShouldBe(expectedCode);
    }

    [Fact]
    public void Discard_customizations_for_model_validator()
    {
        var expectedCode = "GreaterThan";
        var model = new ParentModelBuilder().Build();
        var addressValidatorMock = NestedModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Int)
                .GreaterThan(10);
        });
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel)
                .SetModelValidator(addressValidatorMock.Object);
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Code.ShouldBe(expectedCode);
    }
    
    [Fact]
    public void Execute_validators_for_all_item_collection()
    {
        var model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> { 100, 90, 80 })
            .Build();
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleForEach(x => x.NullableIntList!, x => x
                .GreaterThan(1)
                .GreaterThan(10));
        });
        
        var action = () => parentModelValidatorMock.Object.Validate(model);

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Execute_validators_for_all_item_collection_fails()
    {
        var model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> { 100, 1, 90, 2 })
            .Build();
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            validator.RuleForEach(x => x.NullableIntList!, x => x
                .GreaterThan(0)
                .GreaterThan(10));
        });
        
        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        var errors = exception.Errors.ToArray();
        errors.Length.ShouldBe(2);
        errors[0].FieldName.ShouldBe($"{nameof(model.NullableIntList)}[1]");
        errors[1].FieldName.ShouldBe($"{nameof(model.NullableIntList)}[3]");
    }
    
    [Fact]
    public void Index_is_represented_in_field_name_when_iterate_collection()
    {
        var model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> {100, 1, 2})
            .Build();
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleForEach(x => x.NullableIntList!, x => x
                .GreaterThan(10));
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.Count().ShouldBe(1);
        exception.Errors.First().FieldName.ShouldBe($"{nameof(model.NullableIntList)}[1]");
    }
    
    [Fact]
    public void Validator_conditional_execution()
    {
        var expectedErrorMessage = "TrueCase";
        var model = new ParentModelBuilder()
            .WithNullableIntList(new List<int> {100, 1, 2})
            .Build();
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .When(x => false)
                .GreaterThan(model.NestedModel.Int + 1)
                .When(x => x.NestedModel.Int == model.NestedModel.Int)
                .WithMessage(expectedErrorMessage)
                .GreaterThan(model.NestedModel.Int);
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Message.ShouldBe(expectedErrorMessage);
    }
    
    [Fact]
    public void Replace_default_placeholders()
    {
        var model = new ParentModelBuilder().Build();
        var template = "{FieldDisplayName} is {FieldValue}";
        var expectedMessage = $"NullableString is ";
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NullableString)
                .WithMessage(template)
                .NotNull();
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Message.ShouldBe(expectedMessage);
    }
    
    [Fact]
    public void Do_not_replace_placeholders_not_added()
    {
        var model = new ParentModelBuilder().Build();
        var template = "{PlaceholderNotReplaced} is {FieldValue}";
        var expectedMessage = $"{{PlaceholderNotReplaced}} is {model.NestedModel.Int}";
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .WithMessage(template)
                .GreaterThan(model.NestedModel.Int);
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Replace_custom_error_placeholders()
    {
        var model = new ParentModelBuilder().Build();
        var comparisonValue = model.NestedModel.Int;
        var template = "{ComparisonValue} is not greater than {FieldValue}";
        var expectedMessage = $"{model.NestedModel.Int} is not greater than {comparisonValue}";
        var parentModelValidatorMock = ParentModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.NestedModel.Int)
                .WithMessage(template)
                .GreaterThan(comparisonValue);
        });

        var action = () => parentModelValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Message.ShouldBe(expectedMessage);
    }

    private Mock<ParentModelValidator> ParentModelValidatorMock(Action<ParentModelValidator> validator)
    {
        var validatorMock = new Mock<ParentModelValidator>()
        {
            CallBase = true
        };
        validatorMock.Setup(x => x.CreateRules())
            .Callback(() => { validator(validatorMock.Object); });
        return validatorMock;
    }

    private Mock<NestedModelValidator> NestedModelValidatorMock(
        Action<NestedModelValidator> validator)
    {
        var validatorMock = new Mock<NestedModelValidator>()
        {
            CallBase = true
        };
        validatorMock.Setup(x => x.CreateRules())
            .Callback(() => { validator(validatorMock.Object); });
        return validatorMock;
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

    public record CustomErrorWithCode(string Code) : ValidationError(Code: Code);
}