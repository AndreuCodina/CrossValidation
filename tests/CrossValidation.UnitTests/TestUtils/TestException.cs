using CrossValidation.Exceptions;

namespace CrossValidation.UnitTests.TestUtils;

public class TestException(
    string? code = null,
    string? details = null) :
    BusinessException(code: code, details: details);