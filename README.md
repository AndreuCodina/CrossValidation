![CrossValidation logo](https://user-images.githubusercontent.com/30506301/218343967-ba157171-c491-4a96-960c-73fb663467f0.png)

[![CI workflow state](https://github.com/AndreuCodina/CrossValidation/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/AndreuCodina/CrossValidation/actions/workflows/ci.yml)
[![Coverage Status](https://coveralls.io/repos/github/AndreuCodina/CrossValidation/badge.svg?branch=main&kill_cache=1)](https://coveralls.io/github/AndreuCodina/CrossValidation?branch=main)
[![NuGet](https://img.shields.io/nuget/v/CrossValidation?color=blue&label=nuget)](https://www.nuget.org/packages/CrossValidation)

State-of-the-art .NET library to handle errors and validate data.

## Impact of this library in your company:

- Stop delivering software without a proper error handling mechanism.
- Stop using a different privative solution to validate data in every project of your company.
- Start using modern C# instead of tricks or complex solutions.
- Use typed errors.
- Built-in common error validators.
- Built-in validators for any layer of your project.
- Transport errors from any layer to an input of the frontend.
- Same syntax to validate DTOs or variables.
- Use Minimal APIs with nullable types.

## Examples

> **Note**
> Meanwhile you can use different strategies to represent errors, as raw strings or type-safe resX files, we strongly recommend to use typed errors.

##### Table of contents
[Inline syntax](#inline-syntax)  
[Unified syntax](#unified-syntax)  
[Typed errors](#typed-errors)  
[Model validation](#model-validation)  

<a name="inline-syntax"></a>
###### Inline syntax

Raise a generic error
```csharp
var age = 15;
Validate.Must(age > 17)
```

Raise a error with an raw error message (not localized)
```csharp
var age = 15;
Validate.Must(age > 17, $"You're underage having {age}")
```

Raise a type-safe message (localized or not)
```csharp
var age = 15;
Validate.Must(age > 17, string.Format(ErrorResource.Underage, age))
```

Raise a typed error (localized or not)
```csharp
var age = 15;
Validate.Must(age > 17, new UnderageError(age))
```

<a name="unified-syntax"></a>
###### Unified syntax

Use the same built-in validators for variables and models
```csharp
var age = 15;
Validate.That(age).GreaterThan(17);
```

```csharp
var age = 15;
Validate.That(age)
    .WithMessage($"You're underage having {age}")
    .GreaterThan(17);
```

```csharp
var age = 15;
Validate.That(age)
    .WithMessage(string.Format(ErrorResource.Underage, age)
    .GreaterThan(17)
```

```csharp
var age = 15;
Validate.That(age)
    .WithError(new UnderageError(age)))
    .GreaterThan(17)
```

<a name="typed-errors"></a>
###### Typed errors
C# can't treat with errors in a proper way. Developers tend to reuse the same runtime exceptions (ArgumentException, Exception, MyServiceException...) over and over again with hardcoded messages with parameters, or reuse a general exception (AppException) that won't be logged by the global exception middleware.

Why AppException? Because it's a general exception that can be used in any layer of your application, and its main goal is to express we handled an **expected situation**. For example, if we try to:
  - Create an user with an age less or equal to zero
  - Activate an account and the link to do it has expired
  - Update an user and it doesn't exist in the database
  - Process a request and the token is not valid because the user is not administrator (unauthorized)

Depending on the project you have worked, AppException will have a unique response such as inform the user, or do nothing, but it'll never log AppException because it's not an exceptional error to be solved by the developers.

An **unexpected situation** is, for example, when you:
  - Execute a SQL query and the database is down, then you have a network error
  - Access to an array item out of bounds
  - A null reference exception.

... or when you call a RESTful service and the HTTP library throws an unauthorized exception. Yes, the same error can be exceptional or not. It depends on the context, but C# doesn't have different error mechanisms (failure channel and defect channel, Result and panic, checked and unchecked...). In those programming languages we simply have real typed errors, and if cannot recover us from a situation, we throw an exception or convert a typed error into an exception.

Our goal is to stop using general exceptions with raw strings. Our errors will have correct types since now. That receives the name of typed error in C#.

```csharp
public record UserServiceError
{
    public record UserNotFoundError : UserServiceError;
    public record NicknameNotAvailableError(string Nickname) : UserServiceError;
}
```

But not all UserService errors are validation errors.

Furthermore, C# doesn't have exhaustiveness to pattern match what's the exact UserServiceError we threw and convert it, for example, to a different HTTP status code or create a ProblemDetails with an error message for the validation errors we have in the closed hierarchy. So we have to customize the validation error in the definition (we have to inherit from ValidationError).

A domain error or service error mustn't define a presentation detail (the error message to show in the frontend), but we can't pattern match with exhaustiveness in the upper-layer.

You can throw CrossException (the built-in equivalent of AppException) when you want to return an error, but don't type it.

```csharp
public record UserServiceError(string Message) : MessageValidationError(Message)
{
    public record UserNotFoundError() : UserServiceError("Couldn't find the user")
    public record NicknameNotAvailableError(string Nickname) : UserServiceError($"'{Nickname}' is not available")
}

public class UserService
{  
    public void ChangeNickname(UserDto userDto)
    {
        var user = _context.Users.FirstOrDefault(x => x.Id == userDto.Id);
        Validate.Must(user != null, new UserNotFoundError());
    
        var isNicknameAvailable = _context.Users.Any(x => x.Nickname != userDto.Nickname);
        Validate.Must(isNicknameAvailable, new NicknameNotAvailableError(userDto.Nickname))
    
        user.Nickname = userDto.Nickname;
        _context.Update(user);
        _context.SaveChanges();
    }
}
```

This can be refactored to this:

```csharp
public class UserService
{  
    public void ChangeNickname(UserDto userDto)
    {
        var user = GetUser(userDto.Id);
        CheckNicknameIsAvailable(userDto.Nickname)
    
        user.Nickname = userDto.Nickname;
        _context.Update(user);
        _context.SaveChanges();
    }
}
```

<a name="model-validation"></a>
###### Model validation
...

###### Collect several typed errors
You can create a ValidationException with an error or with a list of errors. Then just collect the errors in a list and throw the exception.

###### Conditions
You can add conditional rules.

```csharp
public record ModelValidator : ModelValidator<Model>
{
    public override void CreateValidations(Model model)
    {
        if (model.CustomerIsPreferred)
        {
            Field(model.CustomerDiscount)
                .NotNull()
                .GreaterThan(0);
            
            Field(model.CreditCardNumber)
                .NotNull();
            
            Field(model.CustomerDiscount)
                .NotNull()
                .GreaterThan(0);
            
            Field(model.CreditCardNumber)
                .NotNull();
        }
        else
        {
            Field(model.CustomerDiscount)
                .Null();
        }
    }
}
```
