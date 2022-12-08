// using System;
// using CrossValidation;
// using CrossValidation.FieldValidators;
// using CrossValidationTests.Models;
// using Shouldly;
// using Xunit;
//
// namespace CrossValidationTests;
//
// public class ModelRuleTests
// {
//      [Fact]
//      public void NotNull()
//      {
//          var model = new CreateOrderModelBuilder()
//              .WithCoupon("Coupon1")
//              .Build();
//          var rule = new ModelRule<CreateOrderModel, string>(model, x => x.Coupon);
//          // .ExecuteValidator(new NotNullValidator);
//          
//          var exception = Record.Exception(() => rule.NotNull());
//
//          exception.ShouldBeNull();
//      }
// }