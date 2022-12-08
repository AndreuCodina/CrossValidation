// using System;
// using CrossValidation;
// using CrossValidation.Rules;
// using CrossValidationTests.Models;
// using Shouldly;
// using Xunit;
//
// namespace CrossValidationTests;
//
// public class RuleExtensionTests
// {
//     [Fact]
//     public void NotNull()
//     {
//         var rule = new InlineRule<string>("value");
//
//         var exception = Record.Exception(() => rule.NotNull());
//         
//         exception.ShouldBeNull();
//     }
//     
//     [Fact]
//     public void NotNullFails()
//     {
//         var rule = new InlineRule<string?>(null);
//
//         var exception = Record.Exception(() => rule.NotNull());
//         
//         exception.ShouldBeOfType<ValidationException>();
//     }
//     
//     [Fact]
//     public void GreaterThan()
//     {
//         var value = 2;
//         var valueToCompare = 1;
//         var rule = new InlineRule<int>(value);
//
//         var exception = Record.Exception(() => rule.GreaterThan(valueToCompare));
//         
//         exception.ShouldBeNull();
//     }
//     
//     [Fact]
//     public void GreaterThanFails()
//     {
//         var value = 1;
//         var valueToCompare = 2;
//         var rule = new InlineRule<int>(value);
//
//         var exception = Record.Exception(() => rule.GreaterThan(valueToCompare));
//         
//         exception.ShouldBeOfType<ValidationException>();
//     }
// }