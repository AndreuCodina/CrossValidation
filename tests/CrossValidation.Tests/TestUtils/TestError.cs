using CrossValidation.Errors;

namespace CrossValidation.Tests.TestUtils;

public record TestError(
    string? Code = null,
    string? Details = null) :
    CrossError(Code: Code, Details: Details);