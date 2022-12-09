using System;
using System.Collections.Generic;
using System.Linq;
using CrossValidation;
using CrossValidation.Rules;
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
        var model = new CreateOrderModelBuilder()
            .WithCoupon("Coupon1")
            .Build();
        var validatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Coupon)
                .NotNull();
        });

        var exception = Record.Exception(() => validatorMock.Object.Validate(model));

        exception.ShouldBeNull();
    }

    [Fact]
    public void One_rule_fails_when_the_field_does_not_pass_the_validator()
    {
        var model = new CreateOrderModelBuilder().Build();
        var validatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Coupon)
                .NotNull();
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Code.ShouldBe("NotNull");
    }

    [Fact]
    public void One_rule_with_same_model_as_field()
    {
        var model = new CreateOrderModelBuilder().Build();
        var validatorMock = CreateOrderModelValidatorMock(validator => { validator.RuleFor(x => x); });

        var exception = Record.Exception(() => validatorMock.Object.Validate(model));

        exception.ShouldBeNull();
    }

    [Fact]
    public void Set_field_information_to_the_error()
    {
        var model = new CreateOrderModelBuilder().Build();
        var expectedFieldName = "DeliveryAddress.Number";
        var expectedFieldValue = model.DeliveryAddress.Number;
        var validatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.DeliveryAddress.Number)
                .GreaterThan(model.DeliveryAddress.Number + 1);
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().FieldName.ShouldBe(expectedFieldName);
        exception.Errors.First().FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Set_field_information_to_the_error_when_using_a_child_model_validator()
    {
        var model = new CreateOrderModelBuilder().Build();
        var expectedFieldName = "DeliveryAddress.Number";
        var expectedFieldValue = model.DeliveryAddress.Number;
        var deliveryAddressValidatorMock = CreateOrderModelAddressValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Number)
                .GreaterThan(10);
        });
        var orderValidatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.DeliveryAddress)
                .SetModelValidator(deliveryAddressValidatorMock.Object);
        });

        var action = () => orderValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().FieldName.ShouldBe(expectedFieldName);
        exception.Errors.First().FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Set_field_information_to_the_error_when_using_several_rule_in_the_same_model_validator()
    {
        var expectedFieldName = "Coupon";
        string? expectedFieldValue = null;
        var model = new CreateOrderModelBuilder().Build();
        var orderValidatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.DeliveryAddress)
                .NotNull();
            validator.RuleFor(x => x.DeliveryAddress.Number)
                .NotNull();
            validator.RuleFor(x => x.Coupon)
                .NotNull();
        });

        var action = () => orderValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().FieldName.ShouldBe(expectedFieldName);
        exception.Errors.First().FieldValue.ShouldBe(expectedFieldValue);
    }
    
    [Fact]
    public void Stop_on_first_error()
    {
        var expectedErrorCodes = new[] { "ErrorCode" };
        var model = new CreateOrderModelBuilder().Build();
        var deliveryAddressValidatorMock = CreateOrderModelAddressValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Number)
                .WithCode("UnexpectedErrorCode2")
                .GreaterThan(model.DeliveryAddress.Number - 1)
                .WithCode("UnexpectedErrorCode3")
                .GreaterThan(model.DeliveryAddress.Number - 1);
            validator.RuleFor(x => x.Information)
                .WithCode("UnexpectedErrorCode4")
                .NotNull()
                .WithCode(expectedErrorCodes[0])
                .Null()
                .WithCode("UnexpectedErrorCode5")
                .Null();
        });
        var orderValidatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Coupon)
                .WithCode("UnexpectedErrorCode1")
                .Null();
            validator.RuleFor(x => x.DeliveryAddress)
                .SetModelValidator(deliveryAddressValidatorMock.Object)
                .WithCode("UnexpectedErrorCode6")
                .NotNull();
        });

        var action = () => orderValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.Select(x => x.Code).ShouldBe(expectedErrorCodes);
    }
    

    [Fact]
    public void Accumulate_errors_modifying_the_validation_mode()
    {
        var expectedErrorCodes = new[]
        {
            "ErrorCode1", "ErrorCode2", "ErrorCode3", "ErrorCode4", "ErrorCode5", "ErrorCode6", "ErrorCode7"
        };
        var model = new CreateOrderModelBuilder().Build();
        var deliveryAddressValidatorMock = CreateOrderModelAddressValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Number)
                .WithCode(expectedErrorCodes[1])
                .GreaterThan(model.DeliveryAddress.Number + 1)
                .WithCode(expectedErrorCodes[2])
                .GreaterThan(model.DeliveryAddress.Number + 1);
            validator.RuleFor(x => x.Information)
                .WithCode("UnexpectedErrorCode")
                .NotNull()
                .WithCode(expectedErrorCodes[3])
                .Null();
        });
        var orderValidatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            validator.RuleFor(x => x.Coupon)
                .WithCode(expectedErrorCodes[0])
                .NotNull();
            validator.RuleFor(x => x.DeliveryAddress)
                .SetModelValidator(deliveryAddressValidatorMock.Object)
                .WithCode(expectedErrorCodes[4])
                .Null();
            validator.RuleFor(x => x.DeliveryAddress.Number)
                .WithCode(expectedErrorCodes[5])
                .GreaterThan(model.DeliveryAddress.Number + 1)
                .WithCode(expectedErrorCodes[6])
                .GreaterThan(model.DeliveryAddress.Number + 1);
        });

        var action = () => orderValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.Select(x => x.Code).ShouldBe(expectedErrorCodes);
    }

    [Fact]
    public void Validation_mode_cannot_be_changed_in_children_validators()
    {
        var model = new CreateOrderModelBuilder().Build();
        var deliveryAddressValidatorMock = CreateOrderModelAddressValidatorMock(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            validator.RuleFor(x => x.Number)
                .GreaterThan(10);
            validator.RuleFor(x => x.Number)
                .WithCode("UnexpectedErrorCode")
                .GreaterThan(model.DeliveryAddress.Number + 1);
        });
        var orderValidatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.DeliveryAddress)
                .SetModelValidator(deliveryAddressValidatorMock.Object);
        });

        var action = () => orderValidatorMock.Object.Validate(model);

        action.ShouldThrow<InvalidOperationException>();
    }
    
    [Fact]
    public void Transform_fails_when_the_field_does_not_pass_the_validator()
    {
        IEnumerable<string> TransformValues(IEnumerable<int> values)
        {
            return values.Select(x => x.ToString());
        }

        var colorIds = new List<int> { 1, 2, 3 };
        var expectedTransformation = TransformValues(colorIds);
        var model = new CreateOrderModelBuilder()
            .WithColorIds(colorIds)
            .Build();
        var validatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            
            validator.RuleFor(x => x.ColorIds)
                .Transform(x => TransformValues(x))
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
        var model = new CreateOrderModelBuilder().Build();
        var validatorMock = CreateOrderModelValidatorMock(validator =>
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
        var model = new CreateOrderModelBuilder().Build();
        var expectedFieldValue = model.DeliveryAddress.Number;
        var validatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.DeliveryAddress.Number)
                .GreaterThan(model.DeliveryAddress.Number + 1);
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().FieldValue.ShouldBe(expectedFieldValue);
    }

    [Fact]
    public void Validator_keeps_message_customization()
    {
        var expectedMessage = "Error message";
        var model = new CreateOrderModelBuilder().Build();
        var validatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Coupon)
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
        var model = new CreateOrderModelBuilder().Build();
        var validatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Coupon)
                .WithCode(expectedCode)
                .NotNull();
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Code.ShouldBe(expectedCode);
    }

    [Fact]
    public void Successful_validator_cleans_customizations()
    {
        var expectedMessage = "Error message";
        var expectedCode = "GreaterThan";
        var model = new CreateOrderModelBuilder().Build();
        var validatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.DeliveryAddress.Number)
                .WithCode(new Bogus.Faker().Lorem.Word())
                .WithMessage(new Bogus.Faker().Lorem.Word())
                .NotNull()
                .WithMessage(expectedMessage)
                .GreaterThan(model.DeliveryAddress.Number + 1);
        });

        var action = () => validatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Message.ShouldBe(expectedMessage);
        exception.Errors.First().Code.ShouldBe(expectedCode);
    }

    [Fact]
    public void Set_model_validator()
    {
        var expectedCode = "GreaterThan";
        var model = new CreateOrderModelBuilder().Build();
        var deliveryAddressValidatorMock = CreateOrderModelAddressValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Number)
                .GreaterThan(10);
        });
        var orderValidatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.DeliveryAddress)
                .WithMessage("Message to be cleaned")
                .SetModelValidator(deliveryAddressValidatorMock.Object);
        });

        var action = () => orderValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Code.ShouldBe(expectedCode);
    }

    [Fact]
    public void Discard_customizations_for_model_validator()
    {
        var expectedCode = "GreaterThan";
        var model = new CreateOrderModelBuilder().Build();
        var addressValidatorMock = CreateOrderModelAddressValidatorMock(validator =>
        {
            validator.RuleFor(x => x.Number)
                .GreaterThan(10);
        });
        var orderValidatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleFor(x => x.DeliveryAddress)
                .SetModelValidator(addressValidatorMock.Object);
        });

        var action = () => orderValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.First().Code.ShouldBe(expectedCode);
    }
    
    [Fact]
    public void Execute_validators_for_all_item_collection()
    {
        var model = new CreateOrderModelBuilder()
            .WithColorIds(new List<int> { 100, 90, 80 })
            .Build();
        var orderValidatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleForEach(x => x.ColorIds, x => x
                .GreaterThan(1)
                .GreaterThan(10));
        });
        
        var action = () => orderValidatorMock.Object.Validate(model);

        action.ShouldNotThrow();
    }
    
    [Fact]
    public void Execute_validators_for_all_item_collection_fails()
    {
        var model = new CreateOrderModelBuilder()
            .WithColorIds(new List<int> { 100, 1, 90, 2 })
            .Build();
        var orderValidatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.ValidationMode = ValidationMode.AccumulateErrors;
            validator.RuleForEach(x => x.ColorIds, x => x
                .GreaterThan(0)
                .GreaterThan(10));
        });
        
        var action = () => orderValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        var errors = exception.Errors.ToArray();
        errors.Length.ShouldBe(2);
        errors[0].FieldName.ShouldBe($"{nameof(model.ColorIds)}[1]");
        errors[1].FieldName.ShouldBe($"{nameof(model.ColorIds)}[3]");
    }
    
    [Fact]
    public void Index_is_represented_in_field_name_when_iterate_collection()
    {
        var model = new CreateOrderModelBuilder()
            .WithColorIds(new List<int> {100, 1, 2})
            .Build();
        var orderValidatorMock = CreateOrderModelValidatorMock(validator =>
        {
            validator.RuleForEach(x => x.ColorIds, x => x
                .GreaterThan(10));
        });

        var action = () => orderValidatorMock.Object.Validate(model);

        var exception = action.ShouldThrow<ValidationException>();
        exception.Errors.Count().ShouldBe(1);
        exception.Errors.First().FieldName.ShouldBe($"{nameof(model.ColorIds)}[1]");
    }

    private Mock<CreateOrderModelValidator> CreateOrderModelValidatorMock(Action<CreateOrderModelValidator> validator)
    {
        var validatorMock = new Mock<CreateOrderModelValidator>()
        {
            CallBase = true
        };
        validatorMock.Setup(x => x.CreateRules())
            .Callback(() => { validator(validatorMock.Object); });
        return validatorMock;
    }

    private Mock<CreateOrderModelDeliveryAddressValidator> CreateOrderModelAddressValidatorMock(
        Action<CreateOrderModelDeliveryAddressValidator> validator)
    {
        var validatorMock = new Mock<CreateOrderModelDeliveryAddressValidator>()
        {
            CallBase = true
        };
        validatorMock.Setup(x => x.CreateRules())
            .Callback(() => { validator(validatorMock.Object); });
        return validatorMock;
    }

    public class CreateOrderModelValidator : ModelValidator<CreateOrderModel>
    {
        public override void CreateRules()
        {
        }
    }

    public class CreateOrderModelDeliveryAddressValidator : ModelValidator<CreateOrderModelDeliveryAddress>
    {
        public override void CreateRules()
        {
        }
    }
}