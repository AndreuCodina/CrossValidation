<img alt="Logo" src="docs/logo.jpg" width="522" height="396">

[![Main workflow state](https://github.com/AndreuCodina/CrossValidation/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/AndreuCodina/CrossValidation/actions/workflows/main.yml)
[![Coverage Status](https://coveralls.io/repos/github/AndreuCodina/CrossValidation/badge.svg?branch=main&coveralls_badge_current_milliseconds=1696585571)](https://coveralls.io/github/AndreuCodina/CrossValidation?branch=main)
[![NuGet](https://img.shields.io/nuget/v/CrossValidation?color=blue&label=nuget)](https://www.nuget.org/packages/CrossValidation)

State-of-the-art .NET library to handle errors and validate data.

# Example <!-- omit in toc -->

Create ErrorResource.resx with the next entry:

```
EmailAlreadyExists = "The email '{email}' is already being used at {company}";
```

Create your business exception:

```csharp
public partial class EmailAlreadyExistsException(string email, string company)
  : ResxBusinessException(ErrorResource.EmailAlreadyExists)
```

Throw the exception:

```csharp
// Add CrossValidation
services.AddCrossValidation();
app.UseCrossValidation();

// Expose endpoint
app.MapPost("/users", () => throw new EmailAlreadyExistsException("alex@gmail.com", "Microsoft"));
```

Call the endpoint and this is the response:

```json
{
  "Errors":
  [
    {
      "Code": "EmailAlreadyExists",
      "Message": "The email 'alex@gmail.com' is already being used at Microsoft"
    }
  ]
}
```

Magic! And perfomant! It doesn't use reflection.

You can send the `Accept-Language` HTTP header and the message will be returned in requested language.


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
  - [#1 Hierarchy inside the service class](#1-hierarchy-inside-the-service-class)
  - [#2 Hierarchy outside the service class](#2-hierarchy-outside-the-service-class)
  - [#3 No hierarchy inside the service class](#3-no-hierarchy-inside-the-service-class)
  - [Note](#note)
- [Model validation](#model-validation)
- [Collect several typed errors](#collect-several-typed-errors)
- [Conditions](#conditions)
- [Context unification](#context-unification)
  - [Validator transformation](#validator-transformation)
  - [Get final transformation](#get-final-transformation)
  - [Model transformation](#model-transformation)
- [Context switching with Value Objects](#context-switching-with-value-objects)
- [Instantiate Value Objects](#instantiate-value-objects)
- [Naming](#naming)

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
C# can't treat with errors in a proper way. Developers tend to reuse the same runtime exceptions (ArgumentException, Exception, MyServiceException...) over and over again with hardcoded messages with parameters, or reuse a <ins>general exception</ins> (usually named `BusinessException`, AppException or DomainException).

This general exception `BusinessException` can be used in any layer of your application, and its main goal is to express we handled an **expected error** (the name is too long, the email hasn't an allowed provider, you tried to sign up with a used email, etc.), and therefore the global exception middleware will generate a custom HTTP response for the frontend. If the middleware doesn't detect an exception of type `BusinessException` or anyone inheriting from it, it'll consider it an **unexpected error** (null reference, network error, access to an array item out of bounds) and it'll be logged to be inspected later by developers. 

Once we've understood how exceptions are used in enterprise applications, we can start to speak about how to organize exceptions.

> Object-oriented developers tend to think "our application of millions of lines of code has exceptions, and we have a folder with thousands of exceptions that we can reuse".

Now we have a considerable design flaw. This is a code smell. Basically, you don't organize exceptions.

So, how can we organize our expected exceptions? Declaring them where they belong. Let's show several examples:

- If you have the Product domain entity and you try to create a product, you can have an exception because you're a seller with a bad reputation.
- If you have the User application service and you try to change your nickname, you can have an exception because the nickname is not available.

We'll continue the example with **UserService**, and we'll define two exceptions:

```csharp
public class NotFoundUserException : BusinessException;
public class NotAvailableNicknameException(string nickname) : BusinessException;
```

Now we have two typed exceptions instead of using a general exception as BusinessException.

So, how do we "attach" a group of exceptions to a class, in this case **UserService**? We can't do it in a proper way in C#, but we have some approaches.<br>And regarding to testing, we have to test those group of exceptions in **UserServiceTests**, not others from the black hole folder called _Exceptions_.

<a name="hierarchy-inside-the-service-class"></a>
### #1 Hierarchy inside the service class

```csharp
public class UserService(DatabaseContext context)
{
    public class Exception
    {
        public class NotFoundUserException()
            : BusinessException("Couldn't find the user");
        
        public class NotAvailableNicknameException(string nickname)
            : BusinessException($"'{nickname}' is not available");
    }
    
    public void ChangeNickname(UserDto userDto)
    {
        var user = context.Users.FirstOrDefault(x => x.Id == userDto.Id);
        
        if (user is null)
        {
            throw new Exception.NotFoundUserException();
        }
        
        var isNicknameAvailable = !context.Users.Any(x => x.Nickname == userDto.Nickname);
        Validate.Must(isNicknameAvailable, new Exception.NotAvailableNicknameException(userDto.Nickname))
    
        user.Nickname = userDto.Nickname;
        context.Users.Update(user);
        context.SaveChanges();
    }
```

So, when you want to handle exceptions in your business logic or test them, you simply reference those exceptions related to your service, instead of referencing an exception in a folder with thousands of them, and instead of having faith that the service will throw that exception (good luck with refactorings).

Sharing exceptions must be an exceptional case, and, as I show in my book, it causes a lot of problems as the codebase grows.

So, the service can be tested this way:

```csharp
var action = () => userService.ChangeNickname(userDto);

action.Should()
    .Throw<UserService.Exception.NotAvailableNicknameException>();
```

What happens when you create an interface because you rely on mocking? Then you just move the exceptions to the interface:

```csharp
var action = () => userService.ChangeNickname(userDto);

action.Should()
    .Throw<IUserService.Exception.NotAvailableNicknameException>();
```

This could be strange to see for first time in C# (in part because we have a convention to name interfaces), but it's absolutely common in other languages.

<a name="hierarchy-outside-the-service-class"></a>
### #2 Hierarchy outside the service class

You create it in _UserService.cs_.

```csharp
public class UserServiceException
{
    public class NotFoundUserException()
      : BusinessException("Couldn't find the user");
    
    public class NotAvailableNicknameException(string nickname)
      : BusinessException($"'{nickname}' is not available");
}

public class UserService(DatabaseContext context)
{
    public void ChangeNickname(UserDto userDto)
    {
        var user = context.Users.FirstOrDefault(x => x.Id == userDto.Id);
        
        if (user is null)
        {
            throw new UserServiceException.NotFoundUserException();
        }
        
        var isNicknameAvailable = !context.Users.Any(x => x.Nickname == userDto.Nickname);
        Validate.Must(isNicknameAvailable, new UserServiceException.NotAvailableNicknameException(userDto.Nickname))
    
        user.Nickname = userDto.Nickname;
        context.Users.Update(user);
        context.SaveChanges();
    }
```

It can be tested this way:

```csharp
var action = () => userService.ChangeNickname(userDto);

action.Should()
    .Throw<UserServiceException.NotAvailableNicknameException>();
```

<a name="no-hierarchy-inside-the-service-class"></a>
### #3 No hierarchy inside the service class

```csharp
public class UserService(DatabaseContext context)
{
    public class NotFoundUserException()
      : BusinessException("Couldn't find the user");
    
    public class NotAvailableNicknameException(string nickname)
      : BusinessException($"'{nickname}' is not available");
    
    public void ChangeNickname(UserDto userDto)
    {
        var user = context.Users.FirstOrDefault(x => x.Id == userDto.Id);
        
        if (user is null)
        {
            throw new NotFoundUserException();
        }
        
        var isNicknameAvailable = !context.Users.Any(x => x.Nickname == userDto.Nickname);
        Validate.Must(isNicknameAvailable, new NotAvailableNicknameException(userDto.Nickname))
    
        user.Nickname = userDto.Nickname;
        context.Users.Update(user);
        context.SaveChanges();
    }
```

It can be tested this way:

```csharp
var action = () => userService.ChangeNickname(userDto);

action.Should()
    .Throw<UserService.NotAvailableNicknameException>();
```

### Note

You could use `using static` to have a better usage, but my goal is to provide guidelines for a pragmatic and transparent error handling approach.

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
public class ModelValidator : ModelValidator<Model>
{
    public override void CreateValidations()
    {
        if (Model.CustomerIsPreferred)
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

<a name="naming"></a>
## Naming

The name "CrossValidation" comes from the ability to validate data in different contexts, and the ability to switch the validation context.
