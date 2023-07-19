using CrossValidation.Exceptions;

namespace CrossValidation.Tests.TestUtils;

public class TestException(
    string? code = null,
    string? details = null) :
    BusinessException(code: code, details: details);