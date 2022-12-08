# CrossValidation

State-of-the-art .NET library to use typed errors and validate data in your backend or library.

## Impact of this library in your company:

- Stop deliverying software without a proper error handling mechanism.
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
> Meanwhile you can use different strategies to represent errors, as raw strings or type-safe resx files, we strongly recommend to use typed errors.

##### Table of Contents
[Ensure syntax](#ensure-syntax)  
[Unified syntax](#unified-syntax)
[Model validation](#model-validation)
[Typed errors](#typed-errors)

<a name="ensure-syntax"></a>
###### Ensure syntax

Raise a generic error
```csharp
var age = 15;
Validate.Is(age < 18)
```

Raise a error with an raw error message (not localized)
```csharp
var age = 15;
Validate.Is(age < 18, $"You're a minor having {age}")
```

Raise a type-safe message (localized or not)
```csharp
var age = 15;
Validate.Is(age < 18, string.Format(AppError.Minor, age))
```

Raise a typed error (localized or not)
```csharp
var age = 15;
Validate.Is(age < 18, new AppError.Minor(age))
```

<a name="unified-syntax"></a>
###### Unified syntax

Use the same built-in validators for variables and DTOs
```csharp
var age = 15;
Validate.That(age).GreaterThan(17);
```

```csharp
var age = 15;
Validate.That(age)
    .WithMessage(x => $"You're a minor having {x}")
    .GreaterThan(17);
```

```csharp
var age = 15;
Validate.That(age)
    .WithMessage(x => string.Format(AppError.Minor, x))
    .GreaterThan(17)
```

```csharp
var age = 15;
Validate.That(age)
    .WithError(x => new AppError.Minor(x)))
    .GreaterThan(17)
```

<a name="model-validation"></a>
###### DTO validation

First example:
```csharp
public record UserServiceError : ValidationError
{
    public record NotFound() : UserServiceError()
    public record NicknameIsTaken() : UserServiceError()
}

Énfasis en el testing, en los types y en que haya parámetros en los mensajes de error
```

```csharp
public class UserService
{
    public record UserServiceError(string Message) : ValidationError(Message: Message)
    {
        public record NotFound() : UserServiceError("Couldn't find the user")
        public record NicknameIsNotAvailable/IsTaken(string Nickname) : UserServiceError($"'{Nick}' is not available")
    }
  
    public void ChangeNickname(UserDto userDto)
    {
        var user = _context.Users.FirstOrDefault(x => x.Id == user.Id);
        Validate.Is(user != null, () => new UserServiceError.NotFound());
    
        var isNicknameAvailable = _context.Users.Any(x => x.Nickname != user.Nickname);
        Validate.Is(isNicknameAvailable, () => new UserServideError.NicknameIsNotAvailable(userDto.Nickname))
    
        if (isNicknameAvailable)
        {
            throw new ValidationException(new UserServideError.NicknameIsNotAvailable(userDto.Nickname))
        }
    }
}
```

###### Collect several typed errors
You can create an ValidationException with an error or with list of errors. Then just collect the errors in a list and throw the exception.