<img alt="Logo" src="docs/logo.jpg" width="522" height="396">

[![Main workflow state](https://github.com/AndreuCodina/CrossValidation/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/AndreuCodina/CrossValidation/actions/workflows/main.yml)
[![Coverage Status](https://coveralls.io/repos/github/AndreuCodina/CrossValidation/badge.svg?branch=main&coveralls_badge_current_milliseconds=1687207409)](https://coveralls.io/github/AndreuCodina/CrossValidation?branch=main)
[![NuGet](https://img.shields.io/nuget/v/CrossValidation?color=blue&label=nuget)](https://www.nuget.org/packages/CrossValidation)

State-of-the-art .NET library to handle errors and validate data.

# Impact of this library in your company: <!-- omit in toc -->

- Stop delivering software without a proper error handling mechanism.
- Stop using a different privative solution to validate data in every project of your company.
- Start using modern C# instead of tricks or complex solutions.
- Use typed errors.
- Built-in common error validators.
- Built-in validators for any layer of your project.
- Transport errors from any layer to an input of the frontend.
- Same syntax to validate DTOs or variables.
- Use Minimal APIs with nullable types.

> **Note**
> Meanwhile you can use different strategies to represent errors, as raw strings or type-safe resX files, we strongly recommend to use typed errors.

# Table of contents <!-- omit in toc -->

- [Inline syntax](#inline-syntax)
- [Unified syntax](#unified-syntax)
- [Typed errors](#typed-errors)
- [Model validation](#model-validation)
- [Collect several typed errors](#collect-several-typed-errors)
- [Conditions](#conditions)
- [Context unification](#context-unification)
  - [Validator transformation](#validator-transformation)
  - [Get final transformation](#get-final-transformation)
  - [Model transformation](#model-transformation)
- [Context switching with Value Objects](#context-switching-with-value-objects)
- [Instantiate Value Objects](#instantiate-value-objects)

<a name="inline-syntax"></a>
## Inline syntax

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
## Unified syntax

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
## Typed errors
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
public class UserService
{
    public record UserNotFoundError() : MessageCrossError("Couldn't find the user")
    public record NicknameNotAvailableError(string Nickname) : MessageCrossError($"'{Nickname}' is not available")

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
## Model validation
...

<a name="collect-several-typed-errors"></a>
## Collect several typed errors
You can create a ValidationException with an error or with a list of errors. Then just collect the errors in a list and throw the exception.

<a name="conditions"></a>
## Conditions
You can add conditional rules.

```csharp
public record ModelValidator : ModelValidator<Model>
{
    public override void CreateValidations()
    {
        if (model.CustomerIsPreferred)
        {
            Field(Model.CustomerDiscount)
                .NotNull()
                .GreaterThan(0);
            
            Field(Model.CreditCardNumber)
                .NotNull();
            
            Field(Model.CustomerDiscount)
                .NotNull()
                .GreaterThan(0);
            
            Field(Model.CreditCardNumber)
                .NotNull();
        }
        else
        {
            Field(Model.CustomerDiscount)
                .Null();
        }
    }
}
```

<a name="context-unification"></a>
## Context unification

You're used to validate data in what we call a different context. For example, if you validate the user favorite color is not null in a large validation class (and probably in another file), when you go the application service, you must have faith in a file that will change over time and call unsafe code.

This is a very basic example in another library:

```csharp
// Can throw NullReferenceException if you remove the previous validator, or ignore the validation if it uses null lifting internally, and the frontend will receive a different error than there's no favorite color
Validate.Field(request.FavoriteColorId) // int?
  .NotNull() // int?
  .GreaterThan(x => x.Value > 0); // int?

myDomain.AddFavoriteColor(request.FavoriteColorId.Value); // int?
```

With CrossValidation, you can unify the validation context with different capabilities.

<a name="validator-transformation"></a>
### Validator transformation

```csharp
Validate.Field(request.FavoriteColorId) // int?
  .NotNull() // int?
  .GreaterThan(x => x > 0); // int // If you remove the previous validator, the code doesn't compile

myDomain.AddFavoriteColor(request.FavoriteColorId.Value); // int?
```

<a name="get-final-transformation"></a>
### Get final transformation

```csharp
var favoriteColorId = Validate.Field(request.FavoriteColorId)
  .NotNull()
  .GreaterThan(0)
  .Instance();

myDomain.AddFavoriteColor(favoriteColorId);
```

Another example could be

```csharp
var color = Validate.Field(request.ColorId)
  .NotNull()
  .Enum<Color>()
  .Instance();
```

<a name="model-transformation"></a>
### Model transformation

You can't get an autogenerated DTO after validating the original DTO with `ModelValidator` because it's not possible to do it in C#.

<a name="context-switching-with-value-objects"></a>
## Context switching with Value Objects

Validations should be duplicated in the frontend and the backend, so you only should need to validate the data and, not write the error message or return a generic error. You should use `That` to return a generic error from the Value Object.

```csharp
public record UserAge(int Age)
{
  public static UserAge Create(int age)
  {
    Validate.That(age)
      .Range(18, 150);
    return new(age);
  }
}

var age = UserAge.Create(request.Age);
```

In the real life you can have out of sync the validation of a field in the frontend/s and backend/s, or even between different pages of the same frontend, or simply not have frontend.

What can we do? The domain has no knowledge about the UI context (AKA DTO, AKA the contract between the client and the backend). No worries, you can switch the validation to the UI context!

Inside the Value Object, use `Field` to get proper error messages, and instantiate the Value Object with `Field` and `Instance`.

```csharp
public record UserAge(int Age)
{
  public static UserAge Create(int age)
  {
    Validate.Field(age)
      .Range(18, 150);
    return new(age);
  }
}

var age = Validate.Field(request.Age)
  .Instance(UserAge.Create);
```

It's more verbose, but with this library you have the option to switch the validation from pure domain code to the UI context.

<a name="instantiate-value-objects"></a>
## Instantiate Value Objects

### Not nullable fields <!-- omit in toc -->

Without context switching

```csharp
var email = UserEmail.Create(request.Email);

var emails = request.Emails.Select(UserEmail.Create);
```

With context switching

```csharp
var email = Validate.Field(request.Email)
  .Instance(UserEmail.Create);

var emails = Validate.Field(request.Emails)
  .InstanceMap(UserEmail.Create);
```

### Nullable fields <!-- omit in toc -->

Without context switching

```csharp
var email = request.Email.Map(UserEmail.Create);

var emails = request.Emails?.Select(UserEmail.Create);
```
